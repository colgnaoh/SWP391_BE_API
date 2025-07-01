namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PaymentReq
{
    public class PayOSRequest
    {
        public string orderCode { get; set; }
        public int amount { get; set; }
        public string description { get; set; }
        public string returnUrl { get; set; }
        public string cancelUrl { get; set; }
    }
}
