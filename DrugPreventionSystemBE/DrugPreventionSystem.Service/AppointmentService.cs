using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BookingReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ConsultantModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;


namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class AppointmentService : IAppointmentService
    {
        private readonly DrugPreventionDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppointmentService(DrugPreventionDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> BookWithScheduleAsync(Guid userId, BookingDirectRequest request)
        {
            //// Tìm consultant rảnh
            //var availableConsultant = await _context.consultants
            //    .Where(c => !_context.Appointments.Any(a =>
            //        a.ConsultantId == c.Id &&
            //        a.AppointmentTime == request.AppointmentTime &&
            //        a.Status != AppointmentStatus.Completed))
            //    .FirstOrDefaultAsync();

            //if (availableConsultant == null)
            //{
            //    return new BadRequestObjectResult("Không có chuyên viên nào sẵn sàng tại thời điểm này.");
            //}

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                //ConsultantId = availableConsultant.Id,
                AppointmentTime = request.AppointmentTime,
                Note = request.Note,
                Name = request.Name,
                Phone = request.Phone,
                Address = request.Address,
                Status = AppointmentStatus.Pending
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                Message = "Đặt lịch thành công.",
                Data = new
                {
                    AppointmentId = appointment.Id,
                    //ConsultantId = availableConsultant.Id,
                    appointment.AppointmentTime,
                    appointment.Name,
                    appointment.Phone,
                    appointment.Address
                }
            });
        }

        public async Task<IActionResult> AssignConsultantAsync(AssignConsultantRequest request)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == request.AppointmentId);
            if (appointment == null)
                return new NotFoundObjectResult("Cuộc hẹn không tồn tại.");

            // Only block reassignment if the appointment is completed or cancelled
            if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Canceled)
                return new BadRequestObjectResult("Không thể gán chuyên viên cho cuộc hẹn đã hoàn tất hoặc đã hủy.");

            // Kiểm tra consultant có tồn tại không
            var consultantExists = await _context.consultants.AnyAsync(c => c.Id == request.ConsultantUserId);
            if (!consultantExists)
                return new NotFoundObjectResult("Không tìm thấy chuyên viên tư vấn.");

            appointment.ConsultantId = request.ConsultantUserId;

            // Update status to Assigned if not already
            if (appointment.Status != AppointmentStatus.Assigned)
            {
                appointment.Status = AppointmentStatus.Assigned;
            }

            await _context.SaveChangesAsync();
            return new OkObjectResult("Đã gán chuyên viên thành công.");
        }



        //public async Task<IActionResult> MarkAsCompletedAsync(Guid consultantId, Guid appointmentId)
        //{
        //    var appointment = await _context.Appointments.FirstOrDefaultAsync(a =>
        //        a.Id == appointmentId &&
        //        a.ConsultantId == consultantId &&
        //        a.Status == AppointmentStatus.Confirmed);

        //    if (appointment == null)
        //        return new NotFoundObjectResult("Không tìm thấy cuộc hẹn hoặc đã hoàn thành.");

        //    appointment.Status = AppointmentStatus.Completed;
        //    await _context.SaveChangesAsync();

        //    return new OkObjectResult("Xác nhận hoàn thành cuộc hẹn thành công.");
        //}

        public async Task<IActionResult> ChangeAppointmentStatusAsync(Guid appointmentId, AppointmentStatus newStatus)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return new NotFoundObjectResult("Không tìm thấy cuộc hẹn.");

            appointment.Status = newStatus;
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                Message = $"Đã cập nhật trạng thái cuộc hẹn thành công.",
                AppointmentId = appointment.Id,
                NewStatus = newStatus.ToString()
            });
        }

        public async Task<GetAppointmentsByUserResponse> GetAppointmentsByFilterAsync(
    string userId,
    List<string> roles,
    AppointmentStatus? status = null,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    int pageNumber = 1,
    int pageSize = 12)
        {
            var safePageNumber = Math.Max(1, pageNumber);
            var safePageSize = Math.Max(1, pageSize);

            var query = _context.Appointments
                .Include(a => a.Consultant)
                .AsQueryable();

            // Role-based filtering
            if (roles.Contains("Admin") || roles.Contains("Manager"))
            {
                // No filter
            }
            else if (roles.Contains("Consultant"))
            {
                if (!Guid.TryParse(userId, out var consultantUserGuid))
                {
                    throw new ArgumentException("ID người dùng không hợp lệ.");
                }

                var consultant = await _context.consultants
                    .FirstOrDefaultAsync(c => c.UserId == consultantUserGuid);

                if (consultant == null)
                {
                    throw new InvalidOperationException("Tài khoản chưa được gán với chuyên viên.");
                }

                query = query.Where(a => a.ConsultantId == consultant.Id);
            }
            else
            {
                if (!Guid.TryParse(userId, out var userGuid))
                {
                    throw new ArgumentException("ID người dùng không hợp lệ.");
                }

                query = query.Where(a => a.UserId == userGuid);
            }

            // Filter by status and time
            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            if (fromDate.HasValue)
                query = query.Where(a => a.AppointmentTime >= fromDate.Value.Date);

            if (toDate.HasValue)
            {
                var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.AppointmentTime <= endOfDay);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / safePageSize);

            // Lấy dữ liệu về trước
            var appointmentsRaw = await query
                .OrderByDescending(a => a.AppointmentTime)
                .ThenByDescending(a => a.Id)
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync();

            // Map thủ công sang response model, xử lý Split trong RAM
            var appointments = appointmentsRaw.Select(a => new AppointmentResponseModel
            {
                Id = a.Id,
                AppointmentTime = a.AppointmentTime,
                Status = a.Status,
                Note = a.Note,
                Name = a.Name,
                Consultant = a.Consultant == null ? null : new ConsultantResponseModel
                {
                    Id = a.Consultant.Id,
                    FullName = a.Consultant.FullName,
                    Qualifications = string.IsNullOrEmpty(a.Consultant.Qualifications)
                        ? new List<string>()
                        : a.Consultant.Qualifications.Split(',').Select(q => q.Trim()).ToList()
                }
            }).ToList();

            return new GetAppointmentsByUserResponse
            {
                Success = true,
                TotalCount = totalCount,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalPages = totalPages,
                Data = appointments
            };
        }



        public async Task<IActionResult> CancelAppointmentAsync(Guid appointmentId)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return new NotFoundObjectResult("Cuộc hẹn không tồn tại.");
            }

            if (appointment.Status == AppointmentStatus.Canceled)
            {
                return new BadRequestObjectResult("Cuộc hẹn đã bị hủy trước đó.");
            }

            if (appointment.Status == AppointmentStatus.Completed)
            {
                return new BadRequestObjectResult("Cuộc hẹn đã hoàn thành và không thể hủy.");
            }

            appointment.Status = AppointmentStatus.Canceled;
            //appointment.UpdatedAt = DateTime.UtcNow; // nếu có dùng thời gian cập nhật
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                Success = true,
                Message = "Hủy cuộc hẹn thành công."
            });
        }

    }
}   
