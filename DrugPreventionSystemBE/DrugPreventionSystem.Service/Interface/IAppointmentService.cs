using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BookingReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IAppointmentService
    {
        Task<IActionResult> BookWithScheduleAsync(Guid userId, BookingDirectRequest request);

        Task<IActionResult> AssignConsultantAsync(AssignConsultantRequest request);

        //Task<IActionResult> MarkAsCompletedAsync(Guid consultantId, Guid appointmentId);

        Task<IActionResult> ChangeAppointmentStatusAsync(Guid appointmentId, AppointmentStatus newStatus);

        Task<GetAppointmentsByUserResponse> GetAppointmentsByFilterAsync(
    string userId,
    List<string> roles,
    AppointmentStatus? status = null,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    int pageNumber = 1,
    int pageSize = 12);

        Task<IActionResult> CancelAppointmentAsync(Guid appointmentId);
    }
}
