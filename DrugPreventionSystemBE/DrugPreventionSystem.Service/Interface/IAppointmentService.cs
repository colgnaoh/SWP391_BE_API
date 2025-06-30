using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BookingReqModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IAppointmentService
    {
        Task<IActionResult> BookWithScheduleAsync(Guid userId, BookingDirectRequest request);
        Task<IActionResult> BookWithoutScheduleAsync(Guid userId, BookingRequest request);
        Task<IActionResult> AssignConsultantAsync(AssignConsultantRequest request);
        Task<IActionResult> MarkAsCompletedAsync(Guid consultantId, Guid appointmentId);
        //Task<IActionResult> AddReviewAsync(Guid userId, Guid appointmentId, AppointmentReviewRequest request);
    }

}
