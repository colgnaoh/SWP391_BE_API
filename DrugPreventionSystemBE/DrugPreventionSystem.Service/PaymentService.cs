using CloudinaryDotNet.Actions;
using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PaymentReq;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using DrugPreventionSystemBE.DrugPreventionSystem.Core;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly DrugPreventionDbContext _context;
        private readonly IdServices _idServices;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PayOS _payOS;

        public PaymentService(DrugPreventionDbContext context, IdServices idServices, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, PayOS payOS)
        {
            _context = context;
            _idServices = idServices;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _payOS = payOS;
        }

        private async Task<string> CreatePayOSPaymentAsync(Payment payment)
        {
            try
            {
                ItemData item = new ItemData("Thanh toán đơn hàng DPC", 1, (int)payment.Amount);
                List<ItemData> items = new List<ItemData> { item };

                long timestampMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                int randomSuffix = new Random().Next(0, 999);
                int payOSOrderCode = (int)(timestampMs % 1_000_000_000) * 1000 + randomSuffix;

                PaymentData paymentData = new PaymentData(
                    orderCode: payOSOrderCode,
                    amount: (int)payment.Amount,
                    description: $"Thanh toán đơn hàng {payment.OrderId}",
                    items: items,
                    cancelUrl: _configuration["PayOS:CancelUrl"],
                    returnUrl: _configuration["PayOS:ReturnUrl"]
                );

                Console.WriteLine("===== CALLING PAYOS createPaymentLink() =====");
                Console.WriteLine(JsonSerializer.Serialize(paymentData));

                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                if (createPayment?.checkoutUrl != null)
                {
                    payment.ExternalTransactionId = createPayment.orderCode.ToString();
                    payment.PayOSCheckoutUrl = createPayment.checkoutUrl;
                    return createPayment.checkoutUrl;
                }
                else
                {
                    throw new Exception("Không nhận được đường dẫn thanh toán từ PayOS.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("====== PAYOS ERROR ======");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in token.");
        }

        private string GetCurrentUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
        }

        private async Task AddOrderLogAsync(Guid orderId, Guid? cartId, string action, string? note, Guid? courseId, Guid? userId)
        {
            var orderLog = new OrderLog
            {
                Id = _idServices.GenerateNextId(),
                OrderId = orderId,
                CartId = cartId,
                Action = action, // Chỉ lưu hành động
                Note = note,
                CourseId = courseId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.OrderLogs.Add(orderLog);
            await _context.SaveChangesAsync();
        }
        public async Task<IActionResult> CreatePaymentForOrderAsync(CreatePaymentRequest request)
        {
            var order = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == request.OrderId);
            if (order == null)
            {
                return new BadRequestObjectResult(new BaseResponse { Success = false, Message = "Đơn hàng không tồn tại." });
            }

            if (order.UserId != request.UserId)
            {
                return new UnauthorizedObjectResult(new BaseResponse { Success = false, Message = "Người dùng không có quyền tạo thanh toán cho đơn hàng này." });
            }

            if (order.TotalAmount != request.Amount)
            {
                return new BadRequestObjectResult(new BaseResponse { Success = false, Message = "Số tiền thanh toán không khớp với tổng giá trị đơn hàng." });
            }

            var orderCartItems = await _context.Carts
                                                .Where(c => c.OrderId == request.OrderId && c.Status == CartStatus.Pending)
                                                .Include(c => c.Course)
                                                .ToListAsync();

            foreach (var cartItem in orderCartItems)
            {
                if (cartItem.CourseId.HasValue)
                {
                    var userAlreadyOwnsCourse = await _context.OrderLogs
                        .AnyAsync(ol => ol.UserId == request.UserId &&
                                         ol.CourseId == cartItem.CourseId.Value &&
                                         ol.Action == "Cart Item Status Update"); 
                    if (userAlreadyOwnsCourse)
                    {
                        return new ConflictObjectResult(new BaseResponse
                        {
                            Success = false,
                            Message = $"Bạn đã sở hữu khóa học '{cartItem.Course?.Name}'. Không thể mua lại."
                        });
                    }
                }
            }

            var existingPayment = await _context.Payments
                                                .FirstOrDefaultAsync(p => p.OrderId == request.OrderId && !p.IsDeleted);

            if (existingPayment != null)
            {
                if (existingPayment.Status == PaymentStatus.Success)
                {
                    return new ConflictObjectResult(new BaseResponse { Success = false, Message = "Đơn hàng này đã được thanh toán thành công." });
                }
                else if (existingPayment.Status == PaymentStatus.Pending)
                {
                    string? payUrlToReturn = null;

                    if (!string.IsNullOrEmpty(existingPayment.PayOSCheckoutUrl))
                    {
                        payUrlToReturn = existingPayment.PayOSCheckoutUrl;
                    }
                    else
                    {
                        try
                        {
                            payUrlToReturn = await CreatePayOSPaymentAsync(existingPayment);
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            return new ConflictObjectResult(new BaseResponse { Success = false, Message = $"Đơn hàng đang chờ thanh toán nhưng không thể truy xuất hoặc tạo lại liên kết. Vui lòng liên hệ hỗ trợ. Lỗi: {ex.Message}" });
                        }
                    }

                    if (!string.IsNullOrEmpty(payUrlToReturn))
                    {
                        return new OkObjectResult(new BaseResponse
                        {
                            Success = true,
                            Message = "Đơn hàng đang chờ thanh toán. Vui lòng sử dụng liên kết hiện có để hoàn tất thanh toán.",
                            Data = new
                            {
                                PaymentId = existingPayment.Id,
                                PaymentNo = existingPayment.PaymentNo,
                                PayUrl = payUrlToReturn
                            }
                        });
                    }
                    else
                    {
                        existingPayment.IsDeleted = true;
                        existingPayment.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }
                else if (existingPayment.Status == PaymentStatus.Failed)
                {
                    existingPayment.IsDeleted = true;
                    existingPayment.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return new ConflictObjectResult(new BaseResponse { Success = false, Message = $"Đơn hàng đang ở trạng thái '{existingPayment.Status}'. Không thể tạo thanh toán mới." });
                }
            }

            if (order.Status != OrderStatus.Pending)
            {
                return new BadRequestObjectResult(new BaseResponse { Success = false, Message = $"Đơn hàng đang ở trạng thái '{order.Status}', không thể thanh toán." });
            }

            var nextPaymentId = _idServices.GenerateNextId();

            var payment = new Payment
            {
                Id = nextPaymentId,
                PaymentNo = $"PAY-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                UserId = request.UserId,
                OrderId = request.OrderId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            string? generatedPayUrl = null;

            if (request.PaymentMethod == PaymentMethod.BankTransfer)
            {
                payment.Status = PaymentStatus.Pending;
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // **Không ghi log nếu chỉ là pending, vì yêu cầu là chỉ ghi log khi thanh toán thành công**
                // await AddOrderLogAsync(payment.OrderId.Value, null, "Payment Creation", null, PaymentStatus.Pending.ToLogStatus(), "Tạo thanh toán qua PayOS", null, payment.UserId);
                // await AddOrderLogAsync(payment.OrderId.Value, item.Id, "Cart Item Status Update", CartStatus.Pending.ToLogStatus(), CartStatus.Pending.ToLogStatus(), "Mục giỏ hàng đang chờ thanh toán PayOS", item.CourseId, payment.UserId);

                generatedPayUrl = await CreatePayOSPaymentAsync(payment);
            }
            else
            {
                payment.Status = PaymentStatus.Success;
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                order.Status = OrderStatus.Paid;
                order.UpdatedAt = DateTime.UtcNow;

                // **Chỉ ghi log khi thanh toán thành công**
                await AddOrderLogAsync(payment.OrderId.Value, null, "Payment Completed", "Thanh toán thành công trực tiếp", null, payment.UserId); // **ĐÃ SỬA**
                await AddOrderLogAsync(order.Id, null, "Order Completed", "Đơn hàng đã được thanh toán", null, order.UserId); // **ĐÃ SỬA**

                var cartItemsToUpdate = await _context.Carts
                                                     .Where(c => c.OrderId == request.OrderId && c.Status == CartStatus.Pending)
                                                     .Include(c => c.Course)
                                                     .ToListAsync();

                foreach (var item in cartItemsToUpdate)
                {
                    item.Status = CartStatus.Completed;
                    item.UpdatedAt = DateTime.UtcNow;
                    // **Chỉ ghi log khi mục giỏ hàng hoàn thành**
                    await AddOrderLogAsync(payment.OrderId.Value, item.Id, "Cart Item Completed", "Mục giỏ hàng hoàn thành sau thanh toán", item.CourseId, payment.UserId); // **ĐÃ SỬA**
                }
                await _context.SaveChangesAsync();
            }

            if (payment.Status == PaymentStatus.Success)
            {
                return new OkObjectResult(new BaseResponse
                {
                    Success = true,
                    Message = "Tạo thanh toán thành công và đơn hàng đã được thanh toán.",
                    Data = new
                    {
                        PaymentId = payment.Id,
                        PaymentNo = payment.PaymentNo,
                        Status = payment.Status.ToString()
                    }
                });
            }
            else
            {
                return new OkObjectResult(new BaseResponse
                {
                    Success = true,
                    Message = "Tạo thanh toán thành công. Vui lòng truy cập liên kết để thanh toán.",
                    Data = new
                    {
                        PaymentId = payment.Id,
                        PaymentNo = payment.PaymentNo,
                        PayUrl = generatedPayUrl
                    }
                });
            }
        }
        public async Task<IActionResult> GetPaymentByIdAsync(Guid paymentId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var payment = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted);

            if (payment == null)
            {
                return new NotFoundObjectResult(new BaseResponse { Success = false, Message = "Không tìm thấy thanh toán." });
            }

            if (currentUserRole != Enum.Role.Admin.ToString() && payment.UserId != currentUserId)
            {
                return new UnauthorizedObjectResult(new BaseResponse { Success = false, Message = "Bạn không có quyền truy cập thanh toán này." });
            }

            var response = new PaymentResponse
            {
                PaymentId = payment.Id,
                PaymentNo = payment.PaymentNo,
                UserId = payment.UserId,
                UserName = $"{payment.User.LastName} {payment.User.FirstName}".Trim(),
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt
            };

            return new OkObjectResult(new BaseResponse { Success = true, Data = response });
        }

        public async Task<IActionResult> GetAllPaymentsAsync()
        {
            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != Enum.Role.Admin.ToString())
            {
                return new UnauthorizedObjectResult(new BaseResponse { Success = false, Message = "Bạn không có quyền truy cập tất cả các thanh toán." });
            }

            var payments = await _context.Payments
                .Where(p => !p.IsDeleted)
                .Include(p => p.User)
                .Include(p => p.Order)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PaymentResponse
                {
                    PaymentId = p.Id,
                    PaymentNo = p.PaymentNo,
                    UserId = p.UserId,
                    UserName = $"{p.User.LastName} {p.User.FirstName}".Trim(),
                    OrderId = p.OrderId,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            if (!payments.Any())
            {
                return new NotFoundObjectResult(new BaseResponse { Success = false, Message = "Không tìm thấy thanh toán nào." });
            }

            return new OkObjectResult(new BaseResponse
            {
                Success = true,
                Data = payments
            });
        }

        public async Task<IActionResult> GetPaymentsByUserIdAsync(Guid userId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            if (currentUserRole != Enum.Role.Admin.ToString() && userId != currentUserId)
            {
                return new UnauthorizedObjectResult(new BaseResponse { Success = false, Message = "Bạn không có quyền xem thanh toán của người dùng này." });
            }

            var payments = await _context.Payments
                .Where(p => !p.IsDeleted && p.UserId == userId)
                .Include(p => p.User)
                .Include(p => p.Order)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PaymentResponse
                {
                    PaymentId = p.Id,
                    PaymentNo = p.PaymentNo,
                    UserId = p.UserId,
                    UserName = $"{p.User.LastName} {p.User.FirstName}".Trim(),
                    OrderId = p.OrderId,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            if (!payments.Any())
            {
                return new NotFoundObjectResult(new BaseResponse { Success = false, Message = $"Không tìm thấy thanh toán nào cho người dùng có ID '{userId}'." });
            }

            return new OkObjectResult(new BaseResponse
            {
                Success = true,
                Data = payments
            });
        }

        public async Task<IActionResult> GetPaymentHistoryByUserIdAsync(Guid userId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            if (currentUserRole != Enum.Role.Admin.ToString() && userId != currentUserId)
            {
                return new UnauthorizedObjectResult(new BaseResponse { Success = false, Message = "Bạn không có quyền xem lịch sử giao dịch của người dùng này." });
            }

            var payments = await _context.Payments
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .Include(p => p.User)
                .Include(p => p.Order)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PaymentResponse
                {
                    PaymentId = p.Id,
                    PaymentNo = p.PaymentNo,
                    UserId = p.UserId,
                    UserName = $"{p.User.LastName} {p.User.FirstName}".Trim(),
                    OrderId = p.OrderId,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    ExternalTransactionId = p.ExternalTransactionId,
                    PayOSCheckoutUrl = p.PayOSCheckoutUrl
                })
                .ToListAsync();

            if (!payments.Any())
            {
                return new NotFoundObjectResult(new BaseResponse { Success = false, Message = $"Không tìm thấy lịch sử giao dịch nào cho người dùng có ID '{userId}'." });
            }

            return new OkObjectResult(new BaseResponse
            {
                Success = true,
                Message = $"Lịch sử giao dịch của người dùng ID '{userId}' đã được truy xuất thành công.",
                Data = payments
            });
        }

        public async Task<IActionResult> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus newStatus)
        {
            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != Enum.Role.Admin.ToString())
            {
                return new UnauthorizedObjectResult(new BaseResponse { Success = false, Message = "Bạn không có quyền cập nhật trạng thái thanh toán." });
            }

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted);
            if (payment == null)
            {
                return new NotFoundObjectResult(new BaseResponse { Success = false, Message = "Không tìm thấy thanh toán." });
            }

            var oldPaymentStatus = payment.Status;

            if (payment.Status == PaymentStatus.Failed && newStatus != PaymentStatus.Success && newStatus != PaymentStatus.Pending)
            {
                return new BadRequestObjectResult(new BaseResponse { Success = false, Message = "Thanh toán đã thất bại, chỉ có thể chuyển về trạng thái 'Success' để thử lại hoặc 'Pending' để tạo lại liên kết." });
            }

            payment.Status = newStatus;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await AddOrderLogAsync(payment.OrderId.Value, null, "Payment Status Update", $"Cập nhật trạng thái thanh toán sang '{newStatus}'", null, payment.UserId);

            if (payment.OrderId.HasValue)
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == payment.OrderId);
                if (order != null)
                {
                    var oldOrderStatus = order.Status;
                    if (newStatus == PaymentStatus.Success)
                    {
                        order.Status = OrderStatus.Paid;
                        await AddOrderLogAsync(order.Id, null, "Order Status Update", "Đơn hàng đã được thanh toán", null, order.UserId);

                        var cartItemsToUpdate = await _context.Carts
                                                                .Where(c => c.OrderId == order.Id && c.Status == CartStatus.Pending)
                                                                .ToListAsync();
                        foreach (var item in cartItemsToUpdate)
                        {
                            var oldCartStatus = item.Status;
                            item.Status = CartStatus.Completed;
                            item.UpdatedAt = DateTime.UtcNow;
                            await AddOrderLogAsync(order.Id, item.Id, "Cart Item Status Update", "Mục giỏ hàng hoàn thành do thanh toán thành công", item.CourseId, payment.UserId);
                        }
                        await _context.SaveChangesAsync();
                    }
                    else if (newStatus == PaymentStatus.Failed)
                    {
                        order.Status = OrderStatus.Failed;
                        await AddOrderLogAsync(order.Id, null, "Order Status Update", "Đơn hàng thất bại do thanh toán thất bại", null, order.UserId);

                        var cartItemsToUpdate = await _context.Carts
                                                                .Where(c => c.OrderId == order.Id && c.Status == CartStatus.Pending)
                                                                .ToListAsync();
                        foreach (var item in cartItemsToUpdate)
                        {
                            var oldCartStatus = item.Status;
                            item.Status = CartStatus.Canceled;
                            item.UpdatedAt = DateTime.UtcNow;
                            await AddOrderLogAsync(order.Id, item.Id, "Cart Item Status Update", "Mục giỏ hàng bị hủy do thanh toán thất bại", item.CourseId, payment.UserId);
                        }
                        await _context.SaveChangesAsync();
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return new OkObjectResult(new BaseResponse { Success = true, Message = $"Trạng thái thanh toán '{payment.PaymentNo}' đã được cập nhật thành '{newStatus}'." });
        }
    }
}