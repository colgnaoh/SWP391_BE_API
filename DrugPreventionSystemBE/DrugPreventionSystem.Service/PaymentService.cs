using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PaymentReq;
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

        public async Task<IActionResult> ProcessPaymentAsync(Guid userId, PaymentRequest request)
        {
            var order = await _context.Orders
                .Include(o => o.Cart) // Đảm bảo rằng Cart được load để truy cập UserId
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.Status == Enum.OrderStatus.Pending);

            if (order == null)
            {
                return new NotFoundObjectResult("Đơn hàng không tồn tại hoặc đã được thanh toán.");
            }

            // Kiểm tra xem đơn hàng có thuộc về người dùng hiện tại không (bảo mật)
            if (order.Cart?.UserId != userId)
            {
                return new ForbidResult(); // Người dùng không có quyền truy cập đơn hàng này
            }

            // Bước 1: Giả lập quá trình thanh toán (thực tế sẽ gọi đến cổng thanh toán bên thứ ba)
            // ... (Logic tích hợp cổng thanh toán như Stripe, PayPal, VNPay, MoMo,...)
            // Sau khi cổng thanh toán phản hồi thành công, ta mới tạo các bản ghi dưới đây.
            bool paymentSuccessful = true; // Giả lập thanh toán thành công

            if (!paymentSuccessful)
            {
                return new BadRequestObjectResult("Thanh toán thất bại. Vui lòng thử lại.");
            }

            // Bước 2: Cập nhật trạng thái đơn hàng và tạo bản ghi Payment/Transaction
            order.Status = Enum.OrderStatus.Paid;
            order.OrderDate = DateTime.UtcNow; // Cập nhật thời gian hoàn thành đơn hàng

            var newPayment = new Payment
            {
                Id = _idServices.GenerateNextId(),
                PaymentNo = $"PAY-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                Status = Enum.paymentStatus.Valid,
                UserId = userId,
                Amount = order.TotalAmount,
                PaymentMethod = request.PaymentMethod,
                OrganizationShare = order.TotalAmount * 0.7m, // Ví dụ: Tổ chức giữ 70%
                ConsultantShare = order.TotalAmount * 0.3m, // Ví dụ: Tư vấn viên nhận 30%
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            _context.Payments.Add(newPayment);

            foreach (var orderDetail in order.OrderDetails)
            {
                // Giả định Transaction liên kết trực tiếp với Course (từ OrderDetail)
                // Cần lấy CourseId từ Cart ban đầu hoặc từ OrderDetail nếu có.
                // Với DB hiện tại, OrderDetail không có CourseId, nên việc này phức tạp hơn.
                // Để đơn giản, giả sử CourseId được truyền qua một cách nào đó, hoặc ta sẽ tìm lại từ Cart.
                // Cách tốt nhất là OrderDetail cũng nên lưu CourseId hoặc có cách map rõ ràng.

                // Tìm CourseId từ Cart ban đầu (nếu CartId trong OrderDetails là CartItem Id)
                // Hoặc tìm CourseId dựa vào orderDetails.ServiceType và tìm khóa học tương ứng
                var cartItemForOrderDetail = await _context.Carts
                    .FirstOrDefaultAsync(c => c.Id == order.CartId && c.Status == Enum.CartStatus.Completed); // Tìm cartItem đã được chuyển sang order

                Guid? courseId = cartItemForOrderDetail?.CourseId; // Lấy CourseId từ cartItem

                // Lấy ConsultantId từ Course (giả định mỗi khóa học có một người tạo/tư vấn viên)
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
                Guid? consultantId = course?.UserId; // UserId của người tạo khóa học có thể là Consultant

                var newTransaction = new Transaction
                {
                    Id = _idServices.GenerateNextId(),
                    ConsultantId = consultantId, // Sẽ cần logic để xác định consultantId
                    Amount = orderDetail.Amount,
                    Status = Enum.TransactionStatus.Approved,
                    ServiceType = orderDetail.ServiceType,
                    PaymentId = newPayment.Id,
                    CourseId = courseId, // Gán CourseId
                    ProgramId = null, // Nếu đây là thanh toán khóa học
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                _context.Transactions.Add(newTransaction);

                orderDetail.TransactionId = newTransaction.Id; // Cập nhật liên kết transaction cho orderDetail
                orderDetail.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult("Thanh toán thành công. Đơn hàng của bạn đã được xác nhận.");
        }
    }
}
