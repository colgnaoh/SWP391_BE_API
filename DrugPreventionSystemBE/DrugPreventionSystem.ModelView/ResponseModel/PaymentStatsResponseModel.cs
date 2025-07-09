namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class PaymentStatsResponseModel
    {
        public int TotalPayments { get; set; }
        public int SuccessfulPayments { get; set; }
        public int PendingPayments { get; set; }
        public int FailedPayments { get; set; }
    }
}
