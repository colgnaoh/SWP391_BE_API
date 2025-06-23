using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Helpers;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AuthModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.UserSearchModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Services
{
    public class UserService : IUserService
    {
        private readonly DrugPreventionDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UserService(DrugPreventionDbContext context,
             IEmailService emailService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<IEnumerable<UserResponseModel>> GetAllUsersAsync()
        {
            return await _context.Users
        .Where(u => !u.IsDeleted)
        .Select(u => new UserResponseModel
        {
            Id = u.Id,
            FullName = u.FirstName + " " + u.LastName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Gender = u.Gender,
            Dob = u.Dob,
            ProfilePicUrl = u.ProfilePicUrl,
            Role = u.Role.ToString()
        })
        .ToListAsync();
        }


        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<IActionResult> UpdateUserProfileAsync(UserProfileUpdateRequest request)
        {
            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            if (userPrincipal == null)
            {
                return new UnauthorizedResult();
            }
            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return new BadRequestObjectResult("Không tìm thấy ID người dùng.");
            }
            var updateUser = await _context.Users.FindAsync(userId);
            if (updateUser == null)
            {
                return new NotFoundObjectResult("Người dùng không tồn tại.");
            }
            // Cập nhật thông tin (chỉ những trường được cung cấp)
            if (!string.IsNullOrEmpty(request.FirstName))
            {
                updateUser.FirstName = request.FirstName;
            }
            if (!string.IsNullOrEmpty(request.LastName))
            {
                updateUser.LastName = request.LastName;
            }
            // Kiểm tra và cập nhật Email (nếu thay đổi email, có thể cần xác minh lại)
            if (!string.IsNullOrEmpty(request.Email) && updateUser.Email != request.Email)
            {
                if (!ValidationHelper.IsValidEmail(request.Email))
                {
                    return new BadRequestObjectResult("Email không đúng định dạng.");
                }
                if (_context.Users.Any(u => u.Email == request.Email && u.Id != userId))
                {
                    return new BadRequestObjectResult("Email đã tồn tại với người dùng khác.");
                }
                updateUser.Email = request.Email;
                updateUser.IsVerified = false; // Đặt lại trạng thái xác thực nếu email thay đổi
                updateUser.VerificationToken = Guid.NewGuid().ToString(); // Gán token mới
                updateUser.VerificationTokenExpires = DateTime.UtcNow.AddHours(24);

                // Gửi email xác thực lại nếu email thay đổi
                var confirmationUrl = $"{_configuration["Frontend:BaseUrl"]}/confirm-email?token={updateUser.VerificationToken}";
                var emailBody = $"<p>Email của bạn đã được cập nhật. Vui lòng xác nhận email mới của bạn bằng cách nhấn vào liên kết sau: <a href='{confirmationUrl}'>Xác nhận email</a></p>";
                await _emailService.SendEmailAsync(request.Email, "Xác thực email", emailBody);
            }

            if (_context.Users.Any(u => u.PhoneNumber == request.PhoneNumber && u.Id != userId))
            {
                return new BadRequestObjectResult("Số điện thoại đã tồn tại với người dùng khác.");
            }
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                if (!ValidationHelper.IsValidPhoneNumber(request.PhoneNumber))
                {
                    return new BadRequestObjectResult("Số điện thoại không đúng định dạng.");
                }
                updateUser.PhoneNumber = request.PhoneNumber;
            }
            if (!string.IsNullOrEmpty(request.Address))
            {
                updateUser.Address = request.Address;
            }
            if (!string.IsNullOrEmpty(request.Gender)) // Kiểm tra nếu Gender có giá trị
            {
                updateUser.Gender = request.Gender;
            }
            if (request.Dob.HasValue) // Kiểm tra nếu Dob có giá trị
            {
                var minSqlDate = new DateTime(1753, 1, 1);
                var maxSqlDate = new DateTime(9999, 12, 31);
                if (request.Dob.Value < minSqlDate || request.Dob.Value > maxSqlDate)
                {
                    return new BadRequestObjectResult($"Ngày sinh phải không hợp lệ");
                }
                if (request.Dob.Value > DateTime.UtcNow)
                {
                    return new BadRequestObjectResult("Ngày sinh không được lớn hơn ngày hiện tại.");
                }
                updateUser.Dob = request.Dob.Value;
                updateUser.AgeGroup = AgeGroupHelper.GetAgeGroup(DateTime.UtcNow.Year - updateUser.Dob.Year); // Cập nhật nhóm tuổi
            }
            if (!string.IsNullOrEmpty(request.ProfilePicUrl))
            {
                updateUser.ProfilePicUrl = request.ProfilePicUrl;
            }

            updateUser.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian thay đổi

            try
            {
                if (!string.IsNullOrEmpty(request.Email))
                {
                    await _context.SaveChangesAsync();
                    return new OkObjectResult("Cập nhật thông tin người dùng thành công, vui lòng kiếm tra email để xác nhận!");
                }
                await _context.SaveChangesAsync();
                return new OkObjectResult("Cập nhật thông tin người dùng thành công.");
            }
            catch (DbUpdateException ex)
            {
                return new ObjectResult("Lỗi khi cập nhật dữ liệu người dùng") { StatusCode = 500 };
            }
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            user.IsDeleted = true; // Soft delete
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }


        public bool UserExists(Guid id)
        {
            return _context.Users.Any(u => u.Id == id);
        }


        public async Task<SearchUserResponseModel> SearchUsersAsync(UserSearchModel search)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search.FullName))
                query = query.Where(u => (u.FirstName + " " + u.LastName).Contains(search.FullName));

            if (!string.IsNullOrEmpty(search.Email))
                query = query.Where(u => u.Email!.Contains(search.Email));

            if (!string.IsNullOrEmpty(search.PhoneNumber))
                query = query.Where(u => u.PhoneNumber!.Contains(search.PhoneNumber));

            if (!string.IsNullOrEmpty(search.Gender))
                query = query.Where(u => u.Gender == search.Gender);

            if (!string.IsNullOrEmpty(search.AgeGroup))
                query = query.Where(u => u.AgeGroup.ToString() == search.AgeGroup);

            if (search.IsDeleted.HasValue)
            {
                query = query.Where(u => u.IsDeleted == search.IsDeleted);
            }
            else
            {
                query = query.Where(u => !u.IsDeleted); // default behavior
            }

            var totalCount = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrEmpty(search.SortBy))
            {
                query = search.SortBy switch
                {
                    "FirstName" => search.IsDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
                    "LastName" => search.IsDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
                    _ => search.IsDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
                };
            }

            var users = await query.Skip((search.PageNumber - 1) * search.PageSize)
                                   .Take(search.PageSize)
                                   .ToListAsync();

            var result = users.Select(u => new UserResponseModel
            {
                Id = u.Id,
                FullName = u.FirstName + " " + u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Gender = u.Gender,
                Dob = u.Dob,
                ProfilePicUrl = u.ProfilePicUrl,
                Role = u.Role.ToString()
            }).ToList();

            return new SearchUserResponseModel
            {
                Success = true,
                Data = result,
                TotalCount = totalCount,
                PageNumber = search.PageNumber,
                PageSize = search.PageSize
            };
        }
    }
}
