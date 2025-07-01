namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PayOS
{
    public class PayOSTransactionDetail
    {
        public string? reference { get; set; }
        public decimal amount
        {
            get; set;
        }

        public class PayOSResponseData
        {
            public string? checkoutUrl { get; set; }
            public List<PayOSTransactionDetail>? transactions { get; set; }
            public long orderCode { get; set; }
            public decimal amount { get; set; }
            public string? description { get; set; }
            public string? cancelUrl { get; set; }
            public string? returnUrl { get; set; }
            public string? status { get; set; }
        }

        public class PayOSResponse
        {
            public string? code { get; set; }
            public string? desc { get; set; }
            public PayOSResponseData? data { get; set; }
        }
    }
}