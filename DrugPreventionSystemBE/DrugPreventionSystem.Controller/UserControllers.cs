using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView;
using DrugPreventionSystemBE.DrugPreventionSystem.Service;
using Microsoft.EntityFrameworkCore;
using DrugPreventionSystemBE.DrugPreventionSystem.Service;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using System.Data.SqlClient;


namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("/api/user")]
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
            var nextId = await _idServices.GenerateNextUserIdAsync();

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
                Dob = request.Dob, // 
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
            var user = _context.Users.FirstOrDefault(u => u.VerificationToken == token);
            if (user == null)
                return BadRequest("Token không hợp lệ.");

            user.IsVerified = true;
            user.VerificationToken = null;
            await _context.SaveChangesAsync();

            return Ok("Xác thực email thành công.");
        }

        private string GetAgeGroup(DateTime dob)
        {
            int age = DateTime.Now.Year - dob.Year;
            if (dob > DateTime.Now.AddYears(-age)) age--;

            if (age < 18) return "Student";
            else if (age < 25) return "CollegeStudent";
            else if (age < 60) return "Parent";
            else return "Senior";
        }
    }
}
