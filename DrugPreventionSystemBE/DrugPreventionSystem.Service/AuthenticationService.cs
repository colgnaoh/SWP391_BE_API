using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.Helpers;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class AuthenticationService : IAuthenticationService
    {
        public readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly DrugPreventionDbContext _context;
        private readonly IdServices _idServices;

        public AuthenticationService(
            DrugPreventionDbContext context,
            IEmailService emailService,
            IConfiguration configuration,
            IdServices idServices)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _idServices = idServices;
        }

        public async Task<IActionResult> RegisterUserAsync(UserRegisterRequest request)
        {
            var minSqlDate = new DateTime(1753, 1, 1);
            var maxSqlDate = new DateTime(9999, 12, 31);

            if (request.Dob < minSqlDate || request.Dob > maxSqlDate)
            {
                return new BadRequestObjectResult($"Ngày sinh phải nằm trong khoảng từ {minSqlDate:yyyy-MM-dd} đến {maxSqlDate:yyyy-MM-dd}");
            }

            // Hoặc nếu bạn muốn kiểm tra không cho DOB trong tương lai:
            if (request.Dob > DateTime.UtcNow)
            {
                return new BadRequestObjectResult("Ngày sinh không được lớn hơn ngày hiện tại.");
            }
            if (_context.Users.Any(u => u.Email == request.Email))
                return new BadRequestObjectResult("Email đã tồn tại.");

            var token = Guid.NewGuid().ToString();
            var nextId = _idServices.GenerateNextUserId();

            var age = DateTime.UtcNow.Year - request.Dob.Year;
            var ageGroup = AgeGroupHelper.GetAgeGroup(age);
            var role = Role.Customer; // Mặc định là Cus, có thể thay đổi nếu cần
            var createDate = DateTime.UtcNow;
            var updatedAt = DateTime.UtcNow;

            var user = new User
            {
                Id = nextId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Gender = request.Gender,
                Role = role, // Mặc định là Cus
                Dob = request.Dob, // 
                AgeGroup = ageGroup,
                VerificationToken = token,
                VerificationTokenExpires = DateTime.UtcNow.AddHours(24), //
                IsVerified = false,
                CreatedAt = createDate,
                UpdatedAt = updatedAt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var confirmationUrl = $"{_configuration["Frontend:BaseUrl"]}/confirm-email?token={token}";
            var emailBody = $"<p>Vui lòng xác nhận email của bạn bằng cách nhấn vào liên kết sau: <a href='{confirmationUrl}'>Xác nhận email</a></p>";

            await _emailService.SendEmailAsync(request.Email, "Xác thực email", emailBody);

            return new OkObjectResult("Đăng ký thành công. Vui lòng kiểm tra email để xác thực.");
        }

        public async Task<IActionResult> ConfirmEmailAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return new BadRequestObjectResult("Token không hợp lệ.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.VerificationToken == token);

            if (user == null)
            {
                return new BadRequestObjectResult("Token không hợp lệ.");
            }

            if (user.VerificationTokenExpires < DateTime.UtcNow)
            {
                return new BadRequestObjectResult("Token đã hết hạn.");
            }

            user.IsVerified = true;
            user.VerificationToken = null;
            user.VerificationTokenExpires = null;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return new ObjectResult("Lỗi khi lưu dữ liệu: " + ex.Message) { StatusCode = 500 };
            }

            return new OkObjectResult("Xác thực email thành công.");
        }

        public async Task<IActionResult> LoginUserAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || user.Password != request.Password)
            {
                return new UnauthorizedObjectResult("Email hoặc mật khẩu không đúng.");
            }

            if (!user.IsVerified)
            {
                return new UnauthorizedObjectResult("Email chưa được xác thực. Vui lòng kiểm tra email của bạn.");
            }

            // Optionally: Generate JWT or session token here

            return new OkObjectResult(new
            {
                message = "Đăng nhập thành công.",
                user = new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Gender,
                    user.Dob,
                    user.PhoneNumber
                }
            });
        }

        public async Task<IActionResult> ResendVerificationTokenAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return new BadRequestObjectResult("Email không được để trống.");
            }

            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // Return a generic message to prevent user enumeration (don't reveal if email exists or not)
                return new BadRequestObjectResult("Nếu tài khoản tồn tại, một email xác thực mới đã được gửi.");
            }

            // Check if user is already verified
            if (user.IsVerified)
            {
                return new BadRequestObjectResult("Email này đã được xác thực trước đó.");
            }

            // Generate new verification token and update user
            var newToken = Guid.NewGuid().ToString();
            user.VerificationToken = newToken;
            user.VerificationTokenExpires = DateTime.UtcNow.AddHours(24); // Token valid for 24 hours

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Log the exception
                return new ObjectResult("Lỗi khi cập nhật dữ liệu: " + ex.Message) { StatusCode = 500 };
            }

            // Send new confirmation email
            var confirmationUrl = $"{_configuration["Frontend:BaseUrl"]}/confirm-email?token={newToken}";
            var emailBody = $"<p>Bạn đã yêu cầu gửi lại email xác thực. Vui lòng xác nhận email của bạn bằng cách nhấn vào liên kết sau: <a href='{confirmationUrl}'>Xác nhận email</a></p>";

            await _emailService.SendEmailAsync(email, "Yêu cầu xác thực email mới", emailBody);

            return new OkObjectResult("Email xác thực mới đã được gửi. Vui lòng kiểm tra hộp thư của bạn.");
        }

    }
}
