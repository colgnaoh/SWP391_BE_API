    using CloudinaryDotNet.Actions; 
    using DrugPreventionSystemBE.DrugPreventionSystem.Core;
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
    using Stripe;
    using Stripe.Checkout;
    using System.Security.Claims;
    using System.Text.Json; 

    namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
    {
        public class PaymentService : IPaymentService
        {
            private readonly DrugPreventionDbContext _context;
            private readonly IdServices _idServices;
            private readonly IConfiguration _configuration;
            private readonly IHttpContextAccessor _httpContextAccessor;

            public PaymentService(DrugPreventionDbContext context, IdServices idServices, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
            {
                _context = context;
                _idServices = idServices;
                _configuration = configuration;
                _httpContextAccessor = httpContextAccessor;
            }

            private async Task<string> CreateStripeCheckoutSessionAsync(Payment payment, Order order)
            {
                try
                {
                    var userId = GetCurrentUserId();

                    var options = new SessionCreateOptions
                    {
                        PaymentMethodTypes = new List<string> { "card" }, 

                        Mode = "payment",
                        LineItems = new List<SessionLineItemOptions>
                        {
                            new SessionLineItemOptions
                            {
                                PriceData = new SessionLineItemPriceDataOptions
                                {
                                    Currency = "vnd", 
                                    UnitAmount = (long)(payment.Amount),
                                    ProductData = new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name = $"Thanh toán đơn hàng DPC - Mã: {order.Id}",
                                        Description = $"Thanh toán cho đơn hàng với tổng số tiền {payment.Amount:N0} VND"
                                    },
                                },
                                Quantity = 1,
                            },
                        },
                        SuccessUrl = _configuration["Stripe:SuccessUrl"] + $"?session_id={{CHECKOUT_SESSION_ID}}&orderId={payment.OrderId}&paymentId={payment.Id}",
                        CancelUrl = _configuration["Stripe:CancelUrl"] + $"?orderId={payment.OrderId}&paymentId={payment.Id}",
                        // ...
                        Metadata = new Dictionary<string, string>
                        {
                            { "orderId", payment.OrderId.ToString() },
                            { "paymentId", payment.Id.ToString() },
                            { "userId", userId.ToString() }
                        }
                    };

                    var service = new Stripe.Checkout.SessionService();
                    Stripe.Checkout.Session session = await service.CreateAsync(options);

                    if (!string.IsNullOrEmpty(session.Url))
                    {
                        payment.ExternalTransactionId = session.Id;
                        payment.StripeCheckoutUrl = session.Url;
                    await _context.SaveChangesAsync();
                    return session.Url;
                    }
                    else
                    {
                        throw new Exception("Không nhận được đường dẫn thanh toán từ Stripe.");
                    }
                }
                catch (StripeException ex)
                {
                    Console.WriteLine($"====== STRIPE ERROR (CreateCheckoutSession) ======");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StripeError?.Message);
                    throw new Exception($"Lỗi khi tạo phiên thanh toán Stripe: {ex.StripeError?.Message}", ex);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"====== GENERAL ERROR (CreateCheckoutSession) ======");
                    Console.WriteLine(ex.Message);
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
                    Action = action,
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
                                            ol.Action == "Cart Item Completed");
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

                        // Nếu payment hiện tại là BankTransfer, cố gắng tạo lại session Stripe
                        if (existingPayment.PaymentMethod == Enum.PaymentMethod.BankTransfer)
                        {
                            if (!string.IsNullOrEmpty(existingPayment.StripeCheckoutUrl))
                            {
                                payUrlToReturn = existingPayment.StripeCheckoutUrl;
                            }
                            else // Nếu chưa có URL hoặc URL bị mất, cố gắng tạo lại bằng Stripe
                            {
                                try
                                {
                                    payUrlToReturn = await CreateStripeCheckoutSessionAsync(existingPayment, order);

                                    if (!string.IsNullOrEmpty(payUrlToReturn))
                                    {
                                        await _context.SaveChangesAsync();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return new ConflictObjectResult(new BaseResponse { Success = false, Message = $"Đơn hàng đang chờ thanh toán nhưng không thể truy xuất hoặc tạo lại liên kết. Vui lòng liên hệ hỗ trợ. Lỗi: {ex.Message}" });
                                }
                            }
                        }
                        else if (existingPayment.PaymentMethod == Enum.PaymentMethod.Cash)
                        {
                            // Nếu là thanh toán tiền mặt, không có URL, chỉ trả về trạng thái
                            return new OkObjectResult(new BaseResponse
                            {
                                Success = true,
                                Message = "Đơn hàng đang chờ thanh toán tiền mặt. Vui lòng liên hệ hỗ trợ để xác nhận.",
                                Data = new
                                {
                                    PaymentId = existingPayment.Id,
                                    PaymentNo = existingPayment.PaymentNo,
                                    Status = existingPayment.Status.ToString()
                                }
                            });
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
                            // Nếu không có URL và không thể tạo lại, đánh dấu payment cũ là bị xóa và cho phép tạo mới
                            existingPayment.IsDeleted = true;
                            existingPayment.UpdatedAt = DateTime.UtcNow;
                            await _context.SaveChangesAsync();
                        }
                    }
                    else if (existingPayment.Status == PaymentStatus.Failed)
                    {
                        // Nếu payment cũ đã thất bại, đánh dấu là bị xóa và cho phép tạo mới
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
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                string? generatedPayUrl = null;

                // Xử lý logic tạo thanh toán dựa trên PaymentMethod
                if (request.PaymentMethod == Enum.PaymentMethod.BankTransfer)
                {
                    _context.Payments.Add(payment); // Thêm payment vào DB trước khi tạo session Stripe
                    await _context.SaveChangesAsync();
                    generatedPayUrl = await CreateStripeCheckoutSessionAsync(payment, order);
                }
                else if (request.PaymentMethod == Enum.PaymentMethod.Cash) // Thanh toán tiền mặt
                {
                    payment.Status = PaymentStatus.Success; // Tiền mặt thường được coi là thành công ngay sau khi tạo
                    _context.Payments.Add(payment);
                    await _context.SaveChangesAsync();

                    order.Status = OrderStatus.Paid;
                    order.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await AddOrderLogAsync(payment.OrderId.Value, null, "Payment Completed (Cash)", "Thanh toán tiền mặt thành công.", null, payment.UserId);
                    await AddOrderLogAsync(order.Id, null, "Order Completed", "Đơn hàng đã được thanh toán.", null, order.UserId);

                    var cartItemsToUpdate = await _context.Carts
                                                            .Where(c => c.OrderId == request.OrderId && c.Status == CartStatus.Pending)
                                                            .Include(c => c.Course)
                                                            .ToListAsync();

                    foreach (var item in cartItemsToUpdate)
                    {
                        item.Status = CartStatus.Completed;
                        item.UpdatedAt = DateTime.UtcNow;
                        await AddOrderLogAsync(payment.OrderId.Value, item.Id, "Cart Item Completed", "Mục giỏ hàng hoàn thành sau thanh toán tiền mặt.", item.CourseId, payment.UserId);
                    }
                    await _context.SaveChangesAsync();

                    return new OkObjectResult(new BaseResponse
                    {
                        Success = true,
                        Message = "Tạo thanh toán thành công và đơn hàng đã được thanh toán (Tiền mặt).",
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
                    return new BadRequestObjectResult(new BaseResponse { Success = false, Message = "Phương thức thanh toán không hợp lệ." });
                }

                if (!string.IsNullOrEmpty(generatedPayUrl))
                {
                    await _context.SaveChangesAsync(); // Lưu lại StripeCheckoutUrl sau khi có

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
                else
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
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
                    CreatedAt = payment.CreatedAt,
                    ExternalTransactionId = payment.ExternalTransactionId,
                    StripeCheckoutUrl = payment.StripeCheckoutUrl
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
                        CreatedAt = p.CreatedAt,
                        ExternalTransactionId = p.ExternalTransactionId,
                        StripeCheckoutUrl = p.StripeCheckoutUrl
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
                        CreatedAt = p.CreatedAt,
                        ExternalTransactionId = p.ExternalTransactionId,
                        StripeCheckoutUrl = p.StripeCheckoutUrl
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
                        StripeCheckoutUrl = p.StripeCheckoutUrl
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
                if (newStatus == PaymentStatus.Pending && payment.Status == PaymentStatus.Success)
                {
                    return new BadRequestObjectResult(new BaseResponse { Success = false, Message = "Không thể chuyển thanh toán đã thành công về trạng thái 'Pending'." });
                }
                if (newStatus == PaymentStatus.Success && payment.Status == PaymentStatus.Failed)
                {
                    return new BadRequestObjectResult(new BaseResponse { Success = false, Message = "Không thể chuyển thanh toán đã hủy về trạng thái 'Success'." });
                }

                payment.Status = newStatus;
                payment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await AddOrderLogAsync(payment.OrderId.Value, null, "Payment Status Updated", $"Trạng thái thanh toán cập nhật từ '{oldPaymentStatus}' sang '{newStatus}'", null, payment.UserId);

                if (payment.OrderId.HasValue)
                {
                    var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == payment.OrderId);
                    if (order != null)
                    {
                        if (newStatus == PaymentStatus.Success)
                        {
                            if (order.Status != OrderStatus.Paid)
                            {
                                order.Status = OrderStatus.Paid;
                                order.UpdatedAt = DateTime.UtcNow;
                                await AddOrderLogAsync(order.Id, null, "Order Status Updated", "Đơn hàng được đánh dấu đã thanh toán thành công.", null, order.UserId);

                                var cartItemsToUpdate = await _context.Carts
                                    .Where(c => c.OrderId == order.Id && c.Status == CartStatus.Pending)
                                    .ToListAsync();
                                foreach (var item in cartItemsToUpdate)
                                {
                                    item.Status = CartStatus.Completed;
                                    item.UpdatedAt = DateTime.UtcNow;
                                    await AddOrderLogAsync(order.Id, item.Id, "Cart Item Status Updated", "Mục giỏ hàng hoàn thành do thanh toán thành công.", item.CourseId, payment.UserId);
                                }
                                await _context.SaveChangesAsync();
                            }
                        }
                        else if (newStatus == PaymentStatus.Failed || newStatus == PaymentStatus.Failed)
                        {
                            if (order.Status != OrderStatus.Failed && order.Status != OrderStatus.Failed)
                            {
                                order.Status = OrderStatus.Failed;
                                order.UpdatedAt = DateTime.UtcNow;
                                await AddOrderLogAsync(order.Id, null, "Order Status Updated", $"Đơn hàng thất bại do thanh toán '{newStatus}'.", null, order.UserId);

                                var cartItemsToUpdate = await _context.Carts
                                    .Where(c => c.OrderId == order.Id && c.Status == CartStatus.Pending)
                                    .ToListAsync();
                                foreach (var item in cartItemsToUpdate)
                                {
                                    item.Status = CartStatus.Canceled;
                                    item.UpdatedAt = DateTime.UtcNow;
                                    await AddOrderLogAsync(order.Id, item.Id, "Cart Item Status Updated", "Mục giỏ hàng bị hủy do thanh toán thất bại.", item.CourseId, payment.UserId);
                                }
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }

                return new OkObjectResult(new BaseResponse { Success = true, Message = $"Trạng thái thanh toán '{payment.PaymentNo}' đã được cập nhật thành '{newStatus}'." });
            }
        public async Task<IActionResult> HandleStripeWebhookAsync(string json)
        {
            var stripeWebhookSecret = _configuration["Stripe:WebhookSecret"];
            if (string.IsNullOrEmpty(stripeWebhookSecret))
            {
                Console.WriteLine("Stripe Webhook Secret is not configured.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            Stripe.Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ParseEvent(json);
            }
            catch (StripeException e)
            {
                Console.WriteLine($"Stripe Webhook Error: {e.Message}");
                return new BadRequestObjectResult(new BaseResponse { Success = false, Message = $"Webhook Error: {e.Message}" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Webhook Error: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    {
                        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                        if (session == null || session.Metadata == null)
                        {
                            Console.WriteLine("Checkout Session or Metadata is null.");
                            return new BadRequestObjectResult(new BaseResponse { Success = false, Message = "Dữ liệu session hoặc metadata không hợp lệ." });
                        }

                        if (session.Metadata.TryGetValue("paymentId", out string paymentIdStr) &&
                            Guid.TryParse(paymentIdStr, out Guid paymentId)) // Ensure paymentId is assigned here
                        {
                            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);

                            if (payment == null)
                            {
                                Console.WriteLine($"Payment with ID {paymentId} not found for checkout.session.completed.");
                                return new NotFoundObjectResult(new BaseResponse { Success = false, Message = "Không tìm thấy thanh toán." });
                            }

                            if (payment.Status != PaymentStatus.Success)
                            {
                                var updateResult = await UpdatePaymentStatusAsync(payment.Id, PaymentStatus.Success);
                                if (updateResult is OkObjectResult okResult && ((BaseResponse)okResult.Value).Success)
                                {
                                    Console.WriteLine($"Payment {payment.PaymentNo} (Order {payment.OrderId}) successfully updated to Success via webhook.");
                                    return new OkObjectResult(new BaseResponse { Success = true, Message = "Xử lý webhook checkout.session.completed thành công." });
                                }
                                else
                                {
                                    Console.WriteLine($"Failed to update payment {payment.PaymentNo} to Success: {((BaseResponse)((ObjectResult)updateResult).Value).Message}");
                                    return updateResult;
                                }
                            }
                            Console.WriteLine($"Payment {payment.PaymentNo} already in Success state. No update needed.");
                            return new OkObjectResult(new BaseResponse { Success = true, Message = "Thanh toán đã ở trạng thái thành công." });
                        }
                        else
                        {
                            Console.WriteLine("Missing paymentId in metadata for checkout.session.completed.");
                            return new BadRequestObjectResult(new BaseResponse { Success = false, Message = "Thiếu paymentId trong metadata." });
                        }
                    }

                default:
                    {
                        Console.WriteLine($"Unhandled Stripe event type: {stripeEvent.Type}");
                        return new OkObjectResult(new BaseResponse { Success = true, Message = $"Đã nhận sự kiện Stripe: {stripeEvent.Type} nhưng không có hành động cụ thể." });
                    }
            }
        }
    }
    }