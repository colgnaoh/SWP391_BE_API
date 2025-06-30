using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BookingReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;


namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class AppointmentService : IAppointmentService
    {
        private readonly DrugPreventionDbContext _context;

        public AppointmentService(DrugPreventionDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> BookWithScheduleAsync(Guid userId, BookingDirectRequest request)
        {
            var isOccupied = await _context.Appointments
                .AnyAsync(a => a.ConsultantId == request.ConsultantUserId &&
                               a.AppointmentTime == request.AppointmentTime &&
                               a.Status != AppointmentStatus.Completed);

            if (isOccupied)
                return new BadRequestObjectResult("Không thể đặt lịch. Có thể lịch này đã bị đặt bởi người khác.");

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ConsultantId = request.ConsultantUserId,
                AppointmentTime = request.AppointmentTime,
                Note = request.Note,
                Status = AppointmentStatus.Confirmed
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                Message = "Đặt lịch thành công.",
                Data = new { request.ConsultantUserId, request.AppointmentTime }
            });
        }

        public async Task<IActionResult> BookWithoutScheduleAsync(Guid userId, BookingRequest request)
        {
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ConsultantId = null,
                AppointmentTime = null,
                Note = request.Note,
                Status = AppointmentStatus.Pending
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                Message = "Tạo yêu cầu tư vấn thành công.",
                AppointmentId = appointment.Id
            });
        }

        public async Task<IActionResult> AssignConsultantAsync(AssignConsultantRequest request)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == request.AppointmentId);
            if (appointment == null || appointment.Status != AppointmentStatus.Pending)
                return new NotFoundObjectResult("Cuộc hẹn không tồn tại hoặc đã được xử lý.");

            appointment.ConsultantId = request.ConsultantUserId;
            appointment.AppointmentTime = request.AppointmentTime;
            appointment.Status = AppointmentStatus.Confirmed;

            await _context.SaveChangesAsync();
            return new OkObjectResult("Đã gán chuyên viên thành công.");
        }

        public async Task<IActionResult> MarkAsCompletedAsync(Guid consultantId, Guid appointmentId)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a =>
                a.Id == appointmentId &&
                a.ConsultantId == consultantId &&
                a.Status == AppointmentStatus.Confirmed);

            if (appointment == null)
                return new NotFoundObjectResult("Không tìm thấy cuộc hẹn hoặc đã hoàn thành.");

            appointment.Status = AppointmentStatus.Completed;
            await _context.SaveChangesAsync();

            return new OkObjectResult("Xác nhận hoàn thành cuộc hẹn thành công.");
        }

        //public async Task<IActionResult> AddReviewAsync(Guid userId, Guid appointmentId, AppointmentReviewRequest request)
        //{
        //    var appointment = await _context.Appointments
        //        .Include(a => a.User)
        //        .FirstOrDefaultAsync(a => a.Id == appointmentId && a.UserId == userId && a.Status == AppointmentStatus.Completed);

        //    if (appointment == null)
        //        return new NotFoundObjectResult("Không tìm thấy cuộc hẹn phù hợp để đánh giá.");

        //    var review = new AppointmentReview
        //    {
        //        Id = Guid.NewGuid(),
        //        AppointmentId = appointmentId,
        //        Rating = request.Rating,
        //        Comment = request.Comment,
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    _context.AppointmentReviews.Add(review);
        //    await _context.SaveChangesAsync();

        //    return new OkObjectResult("Đánh giá cuộc hẹn thành công.");
        }
    }   
