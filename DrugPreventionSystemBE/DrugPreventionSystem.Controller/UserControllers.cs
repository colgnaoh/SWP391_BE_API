using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView;
using DrugPreventionSystemBE.DrugPreventionSystem.Service;
using Microsoft.EntityFrameworkCore;
using DrugPreventionSystemBE.DrugPreventionSystem.Service;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using System.Data.SqlClient;
using DrugPreventionSystemBE.DrugPreventionSystem.Helpers;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;


namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("/api/auth")]
    public class UserControllers : ControllerBase
    {
        public readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly DrugPreventionDbContext _context;
        private readonly IdServices _idServices;

        public UserControllers(
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            var minSqlDate = new DateTime(1753, 1, 1);
            var maxSqlDate = new DateTime(9999, 12, 31);

            if (request.Dob < minSqlDate || request.Dob > maxSqlDate)
            {
                return BadRequest($"Ngày sinh phải nằm trong khoảng từ {minSqlDate:yyyy-MM-dd} đến {maxSqlDate:yyyy-MM-dd}");
            }

            // Hoặc nếu bạn muốn kiểm tra không cho DOB trong tương lai:
            if (request.Dob > DateTime.UtcNow)
            {
                return BadRequest("Ngày sinh không được lớn hơn ngày hiện tại.");
            }
            if (_context.Users.Any(u => u.Email == request.Email))
                return BadRequest("Email đã tồn tại.");

            var token = Guid.NewGuid().ToString();
            var nextId = _idServices.GenerateNextUserId();

            var age = DateTime.UtcNow.Year - request.Dob.Year;
            var ageGroup = AgeGroupHelper.GetAgeGroup(age);
            var role = Role.Customer; // Mặc định là Cus, có thể thay đổi nếu cần
            var createDate = DateTime.UtcNow;

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
                IsVerified = false
                
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var confirmationUrl = $"{_configuration["Frontend:BaseUrl"]}/confirm-email?token={token}";
            var emailBody = $"<p>Vui lòng xác nhận email của bạn bằng cách nhấn vào liên kết sau: <a href='{confirmationUrl}'>Xác nhận email</a></p>";

            await _emailService.SendEmailAsync(request.Email, "Xác thực email", emailBody);

            return Ok("Đăng ký thành công. Vui lòng kiểm tra email để xác thực.");
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token không hợp lệ.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.VerificationToken == token);

            if (user == null)
            {
                return BadRequest("Token không hợp lệ.");
            }

            if (user.VerificationTokenExpires < DateTime.UtcNow)
            {
                return BadRequest("Token đã hết hạn.");
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
                return StatusCode(500, "Lỗi khi lưu dữ liệu: " + ex.Message);
            }

            return Ok("Xác thực email thành công.");
        }

        
    }
}
