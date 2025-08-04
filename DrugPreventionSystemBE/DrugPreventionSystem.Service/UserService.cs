using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.Helpers;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AuthModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.UserSearchModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        private readonly IdServices _idServices;

        public UserService(DrugPreventionDbContext context,
            IEmailService emailService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IdServices idServices)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _emailService = emailService;
            _idServices = idServices;
        }

        public async Task<IActionResult> CreateUserAsync(UserRegisterRequest request)
        {
            // Kiểm tra ràng buộc ngày tháng
            var minSqlDate = new DateTime(1753, 1, 1);
            var maxSqlDate = new DateTime(9999, 12, 31);

            if (request.Dob < minSqlDate || request.Dob > maxSqlDate)
            {
                return new BadRequestObjectResult($"Ngày sinh phải nằm trong khoảng từ {minSqlDate:yyyy-MM-dd} đến {maxSqlDate:yyyy-MM-dd}");
            }

            if (request.Dob > DateTime.UtcNow)
            {
                return new BadRequestObjectResult("Ngày sinh không được lớn hơn ngày hiện tại.");
            }

            // Kiểm tra định dạng email và số điện thoại
            if (!ValidationHelper.IsValidEmail(request.Email))
            {
                return new BadRequestObjectResult("Email không đúng định dạng.");
            }
            if (!ValidationHelper.IsValidPhoneNumber(request.PhoneNumber))
            {
                return new BadRequestObjectResult("Số điện thoại không đúng định dạng.");
            }

            // Kiểm tra email và số điện thoại đã tồn tại
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return new BadRequestObjectResult("Email đã tồn tại.");
            }
            if (_context.Users.Any(u => u.PhoneNumber == request.PhoneNumber))
            {
                return new BadRequestObjectResult("Số điện thoại đã tồn tại.");
            }

            // Phân tích vai trò (Role)
            if (!System.Enum.TryParse<Role>(request.Role, true, out Role parsedRole))
            {
                return new BadRequestObjectResult("Vai trò không hợp lệ.");
            }

            var nextId = _idServices.GenerateNextId();
            var token = Guid.NewGuid().ToString();
            var age = DateTime.UtcNow.Year - request.Dob.Year;
            var ageGroup = AgeGroupHelper.GetAgeGroup(age);
            var createDate = DateTime.UtcNow;

            var user = new User
            {
                Id = nextId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Gender = request.Gender,
                Role = parsedRole,
                Dob = request.Dob,
                AgeGroup = ageGroup,
                VerificationToken = token,
                ProfilePicUrl = request.ProfilePicUrl,
                VerificationTokenExpires = DateTime.UtcNow.AddHours(24),
                IsVerified = false,
                CreatedAt = createDate,
                UpdatedAt = createDate
            };

            _context.Users.Add(user);

            // Xử lý tạo thông tin Consultant nếu vai trò là Consultant
            if (parsedRole == Role.Consultant)
            {
                var consultant = new Consultants
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FullName = user.FirstName + " " + user.LastName,
                    Email = user.Email,
                    Qualifications = "",
                    JobTitle = null,
                    HireDate = null,
                    Salary = null,
                    Status = ConsultantStatus.Active, // Mặc định là Active
                    CreatedAt = createDate
                };

                _context.consultants.Add(consultant);
            }

            try
            {
                await _context.SaveChangesAsync();
                var confirmationUrl = $"{_configuration["Frontend:BaseUrl"]}/confirm-email?token={token}";
                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9;'>
                        <div style='max-width: 600px; margin: auto; background: white; border-radius: 8px; overflow: hidden;'>
                            <div style='background: #4CAF50; color: white; padding: 20px; text-align: center;'>
                                <h2>Xác Thực Tài Khoản</h2>
                            </div>
                            <div style='padding: 30px; color: #333;'>
                                <p>Chào {request.FirstName},</p>
                                <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>Drug Prevention System</strong>.</p>
                                <p>Vui lòng nhấn vào nút bên dưới để xác nhận email của bạn:</p>
                                <div style='text-align: center; margin: 20px 0;'>
                                    <a href='{confirmationUrl}' style='background: #4CAF50; color: white; padding: 12px 20px; text-decoration: none; border-radius: 5px;'>Xác Nhận Email</a>
                                </div>
                                <p>Nếu bạn không đăng ký tài khoản, vui lòng bỏ qua email này.</p>
                                <p>Trân trọng,<br/><strong>Drug Prevention System Team</strong></p>
                            </div>
                        </div>
                    </div>";
                await _emailService.SendEmailAsync(request.Email, "Xác thực email", emailBody);

                return new OkObjectResult("Đăng ký thành công. Vui lòng kiểm tra email để xác thực.");
            }
            catch (DbUpdateException ex)
            {
                return new ObjectResult($"Lỗi khi tạo người dùng: {ex.Message}") { StatusCode = 500 };
            }
        }

        public async Task<GetUserByPageResponseModel> GetUsersByPageAsync(int pageNumber, int pageSize, Role? role = null, string? searchCondition = null, bool? isVerified = null)
        {
            var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
            var safePageSize = pageSize < 1 ? 10 : pageSize;

            var query = _context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted);
            if (role != null)
                query = query.Where(u => u.Role.Equals(role));

            if (isVerified.HasValue)
                query = query.Where(u => u.IsVerified == isVerified);

            if (!string.IsNullOrEmpty(searchCondition))
            {
                query = query.Where(u =>
                    u.PhoneNumber.Contains(searchCondition) ||
                    u.Email.Contains(searchCondition) ||
                    u.FirstName.Contains(searchCondition) ||
                    u.LastName.Contains(searchCondition));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / safePageSize);

            var users = await query
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .Select(u => new UserResponseModel
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Gender = u.Gender,
                    Dob = u.Dob,
                    ProfilePicUrl = u.ProfilePicUrl,
                    Role = u.Role.ToString(),
                    IsVerified = u.IsVerified
                })
                .ToListAsync();

            return new GetUserByPageResponseModel
            {
                Success = true,
                Data = users,
                TotalCount = totalCount,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalPages = totalPages
            };
        }




        public async Task<SingleUserResponseModel> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted && u.Id == id)
                .Select(u => new UserResponseModel
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Gender = u.Gender,
                    Dob = u.Dob,
                    ProfilePicUrl = u.ProfilePicUrl,
                    Role = u.Role.ToString()
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return new SingleUserResponseModel
                {
                    Success = false
                };
            }

            return new SingleUserResponseModel
            {
                Success = true,
                Data = user
            };
        }


        public async Task<IActionResult> UpdateUserProfileAsync(UserProfileUpdateRequest request)
        {
            bool emailUpdated = false;
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
                updateUser.IsVerified = false; // Reset trạng thái xác thực
                updateUser.VerificationToken = Guid.NewGuid().ToString(); // Gán token mới
                updateUser.VerificationTokenExpires = DateTime.UtcNow.AddHours(24);
                emailUpdated = true;
            }

            if (!string.IsNullOrEmpty(request.PhoneNumber) && updateUser.PhoneNumber != request.PhoneNumber)
            {
                if (!ValidationHelper.IsValidPhoneNumber(request.PhoneNumber))
                {
                    return new BadRequestObjectResult("Số điện thoại không đúng định dạng.");
                }
                if (_context.Users.Any(u => u.PhoneNumber == request.PhoneNumber && u.Id != userId))
                {
                    return new BadRequestObjectResult("Số điện thoại đã tồn tại với người dùng khác.");
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
                // Lưu thay đổi vào database
                await _context.SaveChangesAsync();

                // Gửi email xác thực nếu email đã được cập nhật
                if (emailUpdated)
                {
                    var confirmationUrl = $"{_configuration["Frontend:BaseUrl"]}/confirm-email?token={updateUser.VerificationToken}";
                    var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9;'>
                        <div style='max-width: 600px; margin: auto; background: white; border-radius: 8px; overflow: hidden;'>
                            <div style='background: #4CAF50; color: white; padding: 20px; text-align: center;'>
                                <h2>Đặt lại mật khẩu</h2>
                            </div>
                            <div style='padding: 30px; color: #333;'>
                                <p>Chào {request.FirstName},</p>
                                <p>Hệ thống <strong>Drug Prevention System</strong>.</p>
                                <p>Vui lòng nhấn vào nút bên dưới để xác nhận đặt lại mật khẩu của bạn:</p>
                                <div style='text-align: center; margin: 20px 0;'>
                                    <a href='{confirmationUrl}' style='background: #4CAF50; color: white; padding: 12px 20px; text-decoration: none; border-radius: 5px;'>Xác Nhận Email</a>
                                </div>
                                <p>Nếu bạn không đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                                <p>Trân trọng,<br/><strong>Drug Prevention System Team</strong></p>
                            </div>
                        </div>
                    </div>";
                    await _emailService.SendEmailAsync(updateUser.Email!, "Xác thực email", emailBody);
                    return new OkObjectResult("Cập nhật thông tin người dùng thành công, vui lòng kiểm tra email để xác nhận!");
                }

                return new OkObjectResult("Cập nhật thông tin người dùng thành công.");
            }
            catch (DbUpdateException ex)
            {
                return new ObjectResult($"Lỗi khi cập nhật dữ liệu người dùng: {ex.Message}") { StatusCode = 500 };
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


        public async Task<IActionResult> ChangePasswordAsync(string currentPassword, string newPassword)
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

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new NotFoundObjectResult("Người dùng không tồn tại.");
            }

            // Xác minh mật khẩu hiện tại
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
            {
                return new BadRequestObjectResult("Mật khẩu hiện tại không đúng.");
            }

            // Kiểm tra nếu mật khẩu mới trùng với mật khẩu hiện tại
            if (currentPassword == newPassword)
            {
                return new BadRequestObjectResult("Mật khẩu mới không được giống mật khẩu hiện tại.");
            }

            // Hash và cập nhật mật khẩu mới
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return new OkObjectResult("Mật khẩu đã được thay đổi thành công.");
            }
            catch (DbUpdateException ex)
            {
                return new ObjectResult($"Lỗi khi cập nhật mật khẩu: {ex.Message}") { StatusCode = 500 };
            }
        }

    }
}
