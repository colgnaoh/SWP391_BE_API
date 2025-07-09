namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class RevenueStatsResponseModel
    {
        public decimal TotalRevenue { get; set; }
        public Dictionary<string, decimal> RevenueByCategory { get; set; }
    }
}
