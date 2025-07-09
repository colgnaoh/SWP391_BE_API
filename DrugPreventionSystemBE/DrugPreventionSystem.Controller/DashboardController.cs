using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("overall")]
        public async Task<IActionResult> GetOverallDashboardStats()
        {
           return await _dashboardService.GetOverallSystemStatisticsAsync();
        }

        [HttpGet("payments")]
        public async Task<IActionResult> GetPaymentStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            return await _dashboardService.GetPaymentStatusStatisticsAsync(startDate, endDate);
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            return await _dashboardService.GetRevenueStatisticsAsync(startDate, endDate);
        }
        [HttpGet("appointments")]
        public async Task<IActionResult> GetAppointmentStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            return await _dashboardService.GetAppointmentStatusStatisticsAsync(startDate, endDate);
        }
    }
}
