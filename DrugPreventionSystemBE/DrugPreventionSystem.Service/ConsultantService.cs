using Azure.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ConsultantModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using DrugPreventionSystemBE.DrugPreventionSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class ConsultantService : IConsultantService
    {
        private readonly DrugPreventionDbContext _context;
        private readonly IUserService _userService;
        public ConsultantService(DrugPreventionDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }
        public async Task<IActionResult> CreateConsultantAsync(ConsultantCreateRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
            if (user == null)
            {
                return new BadRequestObjectResult("User ID không tồn tại.");
            }
        
            // Kiểm tra xem đã có Consultant nào với User ID này chưa
            if (await _context.consultants.AnyAsync(c => c.UserId == request.UserId))
            {
                return new ConflictObjectResult("Người dùng này đã có hồ sơ tư vấn viên.");
            }
            if (user.Role != Role.Consultant)
            {
                user.Role = Role.Consultant;
                _context.Users.Update(user); // Đánh dấu user là đã thay đổi
            }

            var FullName = $"{user.FirstName} {user.LastName}";

            var consultant = new Consultants
            {
                Id = Guid.NewGuid(), // Tạo GUID mới cho Consultant
                UserId = request.UserId,
                FullName = FullName,
                Email = user.Email,
                Qualifications = string.Join("|||", request.Qualifications ?? new List<string>()),
                JobTitle = request.JobTitle,
                HireDate = request.HireDate,
                Salary = request.Salary,
                Status = request.Status ?? DrugPreventionSystemBE.DrugPreventionSystem.Enum.ConsultantStatus.Active, // Giá trị mặc định
                CreatedAt = DateTime.UtcNow
            };

            _context.consultants.Add(consultant);
            try
            {
                await _context.SaveChangesAsync();
                var response = new ConsultantResponseModel
                {
                    Id = (Guid)consultant.Id, // .Value vì Id là Guid?
                    UserId = consultant.UserId.Value,
                    FullName = consultant.FullName,
                    Email = consultant.Email,
                    Qualifications = request.Qualifications ?? new List<string>(),
                    //Qualifications = string.Join("|||", request.Qualifications)
                    JobTitle = consultant.JobTitle,
                    HireDate = (DateTime)consultant.HireDate,
                    Salary = (decimal)consultant.Salary,
                    Status = (Enum.ConsultantStatus)consultant.Status,
                    CreatedAt = (DateTime)consultant.CreatedAt,
                    ProfilePicUrl = user.ProfilePicUrl ?? string.Empty
                };
                return new CreatedAtActionResult("GetConsultantById", "Consultant", new { id = consultant.Id }, response);
            }
            catch (DbUpdateException ex)
            {
                return new ObjectResult($"Lỗi khi tạo tư vấn viên: {ex.Message}") { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> GetConsultantByIdAsync(Guid id)
        {
            var consultant = await _context.consultants
                                                .Include(c => c.User) // Tải dữ liệu người dùng
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(c => c.Id == id);
            if (consultant == null)
            {
                return new NotFoundObjectResult($"Không tìm thấy tư vấn viên với ID: {id}");
            }
            var qualificationsList = consultant.Qualifications?.Split("|||", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            return new OkObjectResult(new GetConsultantByIdResponse
            {
                Success = true,
                Data = new ConsultantResponseModel
                {
                    Id = id,
                    UserId = (Guid)consultant.UserId,
                    FullName = consultant.FullName,
                    JobTitle = consultant.JobTitle,
                    HireDate = (DateTime)consultant.HireDate,
                    Salary = (decimal)consultant.Salary,
                    Status = (Enum.ConsultantStatus)consultant.Status,
                    Email = consultant.Email,
                    CreatedAt = (DateTime)consultant.CreatedAt,
                    Qualifications = qualificationsList,
                    ProfilePicUrl = consultant.User?.ProfilePicUrl ?? string.Empty,
                    PhoneNumber = consultant.User.PhoneNumber
                }
            });
        }

        public async Task<ListConsultantResponse> GetAllConsultantsAsync()
        {
            try
            {
                var consultants = await _context.consultants
                    .Include(c => c.User) //tải dữ liệu từ bảng Users
                    .AsNoTracking()
                    .ToListAsync();

                var consultantResponseModels = consultants.Select(c => new ConsultantResponseModel
                {
                    Id = (Guid)c.Id,
                    UserId = (Guid)c.UserId, // Xử lý null cho Guid?
                    FullName = c.FullName,
                    Email = c.Email,
                    Qualifications = c.Qualifications?.Split("|||", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                    JobTitle = c.JobTitle,
                    HireDate = (DateTime)c.HireDate,
                    Salary = c.Salary ?? 0m, 
                    Status = (ConsultantStatus)c.Status,// Xử lý null cho Enum? (chọn một giá trị mặc định phù hợp)
                    CreatedAt = (DateTime)c.CreatedAt,
                    ProfilePicUrl = c.User?.ProfilePicUrl,
                    PhoneNumber = c.User.PhoneNumber
                }).ToList();

                return new ListConsultantResponse
                {
                    Success = true,
                    Data = consultantResponseModels
                };
            }
            catch (Exception ex)
            {
                // Log lỗi (nên dùng một thư viện logging chuyên nghiệp hơn như ILogger)
                Console.WriteLine($"Error in GetAllConsultantsAsync: {ex.Message}");

                return new ListConsultantResponse
                {
                    Success = false,
                    Data = null
                };
            }
        }

        // --- DELETE ---
        public async Task<IActionResult> DeleteConsultantAsync(Guid id)
        {
            var consultant = await _context.consultants.FindAsync(id);
            if (consultant == null)
            {
                return new NotFoundObjectResult($"Không tìm thấy tư vấn viên với ID: {id}");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == consultant.UserId);
            if (user != null && user.Role == Role.Consultant)
            {
                user.Role = Role.Customer;
                _context.Users.Update(user);
            }

            _context.consultants.Remove(consultant);

            _context.consultants.Remove(consultant);
            try
            {
                await _context.SaveChangesAsync();
                return new OkObjectResult("Xóa tư vấn viên thành công.");
            }
            catch (DbUpdateException ex)
            {
                return new ObjectResult($"Lỗi khi xóa tư vấn viên: {ex.Message}") { StatusCode = 500 };
            }
        }
            
        // --- HELPER METHOD ---
        public async Task<bool> ConsultantExists(Guid id)
        {
            return await _context.consultants.AnyAsync(e => e.Id == id);
        }

        public async Task<IActionResult> UpdateConsultantAsync(Guid id, ConsultantUpdateRequest request) // Thêm Guid id
        {
            var consultant = await _context.consultants.FirstOrDefaultAsync(c => c.Id == id);

            if (consultant == null)
            {
                return new NotFoundObjectResult($"Không tìm thấy tư vấn viên với ID: {id}");
            }

            if (request.Qualifications != null) 
            {
                consultant.Qualifications = string.Join("|||", request.Qualifications);
            }


            if (request.JobTitle != null) // string?
            {
                consultant.JobTitle = request.JobTitle;
            }

            if (request.HireDate.HasValue) // DateTime?
            {
                consultant.HireDate = request.HireDate.Value;
            }

            if (request.Salary.HasValue) // decimal?
            {
                consultant.Salary = request.Salary.Value;
            }

            if (request.Status.HasValue) // ConsultantStatus?
            {
                consultant.Status = request.Status.Value;
            }

            _context.consultants.Update(consultant); 
            try
            {
                await _context.SaveChangesAsync();

                var qualificationsList = consultant.Qualifications?.Split("|||", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
                return new OkObjectResult(new GetConsultantByIdResponse
                {
                    Success = true,
                    Data = new ConsultantResponseModel
                    {
                        JobTitle = consultant.JobTitle,
                        HireDate = (DateTime)consultant.HireDate,
                        Salary = (decimal)consultant.Salary,
                        Status = (Enum.ConsultantStatus)consultant.Status,
                        Qualifications = qualificationsList
                    }
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ConsultantExists(id))
                {
                    return new NotFoundObjectResult($"Không tìm thấy tư vấn viên với ID: {id}");
                }
                else
                {
                    return new ConflictObjectResult($"Lỗi đồng thời khi cập nhật tư vấn viên với ID: {id}");
                }
            }
            catch (DbUpdateException ex)
            {
                return new ObjectResult($"Lỗi khi cập nhật tư vấn viên: {ex.Message}") { StatusCode = 500 };
            }
        }
    }
}
