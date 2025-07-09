using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IDashboardService
    {
        Task<IActionResult> GetOverallSystemStatisticsAsync();
        Task<IActionResult> GetPaymentStatusStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null); 
        Task<IActionResult> GetRevenueStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null); 
        Task<IActionResult> GetAppointmentStatusStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null); 
    }
}
