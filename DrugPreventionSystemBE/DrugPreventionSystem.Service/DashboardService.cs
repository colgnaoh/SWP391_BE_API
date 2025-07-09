using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly DrugPreventionDbContext _context;

        public DashboardService(DrugPreventionDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetOverallSystemStatisticsAsync()
        {
            var stats = new DashboardStatsResponseModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalCourses = await _context.Courses.CountAsync(),
                TotalCommunityPrograms = await _context.Programs.CountAsync(),
                TotalConsultants = await _context.consultants.CountAsync(),
                TotalBlogs = await _context.Blogs.CountAsync(),
                TotalSurveys = await _context.Surveys.CountAsync(),
            };

            return new OkObjectResult(new BaseResponse(true, "Tổng quan hệ thộng.", stats));
        }

        public async Task<IActionResult> GetPaymentStatusStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Payments.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= endDate.Value.AddDays(1));
            }

            var payments = await query.ToListAsync();

            var rs = new PaymentStatsResponseModel
            {
                TotalPayments = payments.Count,
                SuccessfulPayments = payments.Count(p => p.Status == PaymentStatus.Success),
                PendingPayments = payments.Count(p => p.Status == PaymentStatus.Pending),
                FailedPayments = payments.Count(p => p.Status == PaymentStatus.Success)
            };
            return new OkObjectResult(new BaseResponse(true, "Thống kê trạng thái thanh toán.", rs));
        }

        public async Task<IActionResult> GetRevenueStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders
                                .Where(o => o.Status == OrderStatus.Paid)
                                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value.AddDays(1));
            }

            var orders = await query
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(od => od.Course)
                                .ToListAsync();

            var totalRevenue = orders.Sum(o => o.TotalAmount);

            var revenueByCategory = new Dictionary<string, decimal>();

            foreach (var order in orders)
            {
                foreach (var detail in order.OrderDetails)
                {
                    var serviceType = detail.ServiceType.ToString();
                    if (revenueByCategory.ContainsKey(serviceType))
                    {
                        revenueByCategory[serviceType] += (decimal)detail.Amount;
                    }
                    else
                    {
                        revenueByCategory.Add(serviceType, (decimal)detail.Amount);
                    }
                }
            }

            var rs = new RevenueStatsResponseModel
            {
                TotalRevenue = totalRevenue,
                RevenueByCategory = revenueByCategory
            };
            return new OkObjectResult(new BaseResponse(true, "Thống kê doanh thu.", rs));
        }

        public async Task<IActionResult> GetAppointmentStatusStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Appointments.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(a => a.AppointmentTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(a => a.AppointmentTime <= endDate.Value.AddDays(1));
            }

            var appointments = await query
                                    .Include(a => a.Consultant)
                                    .ToListAsync();

            var appointmentsByStatus = new AppointmentStatsResponseModel
            {
                TotalAppointments = appointments.Count,
                PendingAppointments = appointments.Count(a => a.Status == AppointmentStatus.Pending),
                AssignedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Assigned),
                ConfirmedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Canceled),
                AppointmentsByConsultant = appointments
                                                .Where(a => a.Consultant != null)
                                                .GroupBy(a => a.Consultant.FullName)
                                                .ToDictionary(g => g.Key, g => g.Count())
            };

            return new OkObjectResult(new BaseResponse(true, "Thống kê đặc lịch tư vắn của hệ thống.", appointmentsByStatus));
        }
    }
}
