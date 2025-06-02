using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.Helpers;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AuthModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


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
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return new OkObjectResult(new UserResponseLogin
            {
                Success = true,
                Data = new UserResponseData
                {
                    Token = jwtToken,
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Gender = user.Gender,
                    Dob = user.Dob,
                    PhoneNumber = user.PhoneNumber
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

        public async Task<IActionResult> RequestPasswordResetAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return new BadRequestObjectResult("Email không được để trống.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // QUAN TRỌNG: TRẢ VỀ THÔNG BÁO CHUNG ĐỂ TRÁNH TIẾT LỘ THÔNG TIN NẾU EMAIL KHÔNG TỒN TẠI
            // Điều này ngăn chặn kẻ tấn công biết được email nào có tài khoản trên hệ thống.
            if (user == null)
            {
                return new OkObjectResult("Nếu tài khoản tồn tại, một liên kết đặt lại mật khẩu đã được gửi đến email của bạn.");
            }

            // Tạo token đặt lại mật khẩu mới
            var resetToken = Guid.NewGuid().ToString();
            user.PasswordResetToken = resetToken;
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(2); // Token có hiệu lực trong 2 giờ

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return new ObjectResult("Lỗi khi lưu dữ liệu đặt lại mật khẩu: " + ex.Message) { StatusCode = 500 };
            }

            // Gửi email chứa liên kết đặt lại mật khẩu
            var resetUrl = $"{_configuration["Frontend:BaseUrl"]}/reset-password?token={resetToken}&email={user.Email}";
            var emailBody = $"<p>Bạn đã yêu cầu đặt lại mật khẩu. Vui lòng nhấp vào liên kết sau để đặt lại mật khẩu của bạn: <a href='{resetUrl}'>Đặt lại mật khẩu</a></p>" +
                            $"<p>Liên kết này sẽ hết hạn sau 1 giờ.</p>";

            await _emailService.SendEmailAsync(user.Email, "Đặt lại mật khẩu của bạn", emailBody);

            return new OkObjectResult("Nếu tài khoản tồn tại, một liên kết đặt lại mật khẩu đã được gửi đến email của bạn.");
        }

        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
            {
                return new BadRequestObjectResult("Email, Token và Mật khẩu mới không được để trống.");
            }

            // Tìm người dùng bằng email và token đặt lại mật khẩu
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.PasswordResetToken == request.Token);

            if (user == null)
            {
                return new BadRequestObjectResult("Token hoặc Email không hợp lệ.");
            }

            // Kiểm tra xem token đã hết hạn chưa
            if (user.ResetTokenExpires < DateTime.UtcNow)
            {
                return new BadRequestObjectResult("Token đặt lại mật khẩu đã hết hạn.");
            }

            // CẢNH BÁO QUAN TRỌNG: Trong một ứng dụng thực tế, bạn PHẢI băm mật khẩu mới trước khi lưu vào cơ sở dữ liệu.
            // Ví dụ: user.Password = _passwordHasher.HashPassword(request.NewPassword);
            user.Password = request.NewPassword; // Đây chỉ là ví dụ, KHÔNG AN TOÀN cho sản phẩm thực tế

            // Xóa token đặt lại mật khẩu sau khi sử dụng để ngăn chặn việc sử dụng lại
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;
            user.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian cuối cùng được cập nhật

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Ghi nhật ký ngoại lệ
                return new ObjectResult("Lỗi khi cập nhật mật khẩu: " + ex.Message) { StatusCode = 500 };
            }

            return new OkObjectResult("Mật khẩu của bạn đã được đặt lại thành công.");
        }

        public async Task<IActionResult> LoginWithFacebookAsync(ExternalLoginInfoModel externalInfo)
        {
            // Kiểm tra email từ Facebook
            if (string.IsNullOrEmpty(externalInfo.Email))
            {
                return new BadRequestObjectResult("Không thể lấy email từ tài khoản Facebook.");
            }

            // Tìm người dùng dựa trên email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == externalInfo.Email);

            if (user == null)
            {
                // Nếu chưa có tài khoản, tạo mới user từ thông tin Facebook
                var nextId = _idServices.GenerateNextUserId();

                var nameParts = (externalInfo.FullName ?? "").Split(" ", StringSplitOptions.RemoveEmptyEntries);
                var firstName = nameParts.Length > 1 ? nameParts[0] : externalInfo.FullName;
                var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

                user = new User
                {
                    Id = nextId,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = externalInfo.Email,
                    Password = null, // Không cần mật khẩu nếu đăng nhập bằng Facebook
                    IsVerified = true,
                    Role = Role.Customer,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Sinh JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return new OkObjectResult(new UserResponseLogin
            {
                Success = true,
                Data = new UserResponseData
                {
                    Token = jwtToken,
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Gender = user.Gender,
                    Dob = user.Dob,
                    PhoneNumber = user.PhoneNumber
                }
            });
        }


    }
}
