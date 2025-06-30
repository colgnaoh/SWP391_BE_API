using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PaymentReq;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

            var existingSuccessPayment = await _context.Payments.AnyAsync(p => p.OrderId == request.OrderId && p.Status == PaymentStatus.Success && !p.IsDeleted);
            if (existingSuccessPayment)
            {
                return new ConflictObjectResult(new BaseResponse { Success = false, Message = "Đơn hàng này đã được thanh toán thành công." });
            }

            // Kiểm tra xem đơn hàng có đang ở trạng thái cho phép thanh toán không (ví dụ: Pending)
            if (order.Status != OrderStatus.Pending)
            {
                return new BadRequestObjectResult(new BaseResponse { Success = false, Message = $"Đơn hàng đang ở trạng thái '{order.Status}', không thể thanh toán." });
            }

            order.Status = OrderStatus.Paid; 
            order.UpdatedAt = DateTime.UtcNow;

            var nextPaymentId = _idServices.GenerateNextId();

            var payment = new Payment
            {
                Id = nextPaymentId,
                PaymentNo = $"PAY-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}", // Tạo mã thanh toán
                UserId = request.UserId,
                OrderId = request.OrderId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                Status = PaymentStatus.Success, // Giả định thanh toán luôn thành công khi tạo thủ công/giả lập
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new BaseResponse
            {
                Success = true,
                Message = "Thanh toán đã được ghi nhận thành công cho đơn hàng.",
                Data = new PaymentResponse
                {
                    PaymentId = payment.Id,
                    PaymentNo = payment.PaymentNo,
                    UserId = payment.UserId,
                    UserName = order.User?.LastName, // Lấy tên user từ order
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    PaymentMethod = payment.PaymentMethod,
                    Status = payment.Status,
                    CreatedAt = payment.CreatedAt
                }
            });
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

            if (currentUserRole != Role.Admin.ToString() && payment.UserId != currentUserId)
            {
                return new UnauthorizedObjectResult(new BaseResponse { Success = false, Message = "Bạn không có quyền truy cập thanh toán này." });
            }

            var response = new PaymentResponse
            {
                PaymentId = payment.Id,
                PaymentNo = payment.PaymentNo,
                UserId = payment.UserId,
                UserName = payment.User?.LastName,
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
            if (currentUserRole != Role.Admin.ToString())
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
                    UserName = p.User.LastName,
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

            if (currentUserRole != Role.Admin.ToString() && userId != currentUserId)
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
                    UserName = p.User.LastName,
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

        public async Task<IActionResult> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus newStatus)
        {
            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != Role.Admin.ToString())
            {
                return new UnauthorizedObjectResult(new BaseResponse { Success = false, Message = "Bạn không có quyền cập nhật trạng thái thanh toán." });
            }

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted);
            if (payment == null)
            {
                return new NotFoundObjectResult(new BaseResponse { Success = false, Message = "Không tìm thấy thanh toán." });
            }
                        
            if (payment.Status == PaymentStatus.Failed && newStatus != PaymentStatus.Success)
            {
                return new BadRequestObjectResult(new BaseResponse { Success = false, Message = "Thanh toán đã thất bại, chỉ có thể chuyển về trạng thái 'Pending' để thử lại." });
            }

            payment.Status = newStatus;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Cập nhật trạng thái Order tương ứng nếu Payment là Success/Failed/Refunded
            if (payment.OrderId.HasValue)
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == payment.OrderId);
                if (order != null)
                {
                    if (newStatus == PaymentStatus.Success)
                    {
                        order.Status = OrderStatus.Paid; 
                    }
                    else if (newStatus == PaymentStatus.Failed)
                    {
                        order.Status = OrderStatus.Failed;
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return new OkObjectResult(new BaseResponse { Success = true, Message = $"Trạng thái thanh toán '{payment.PaymentNo}' đã được cập nhật thành '{newStatus}'." });
        }
    }
}
