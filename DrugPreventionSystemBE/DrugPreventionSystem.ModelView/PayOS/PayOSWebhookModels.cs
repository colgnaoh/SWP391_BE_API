
namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PayOS
{
    public class PayOSWebhookPayload
    {
        public int code { get; set; }
        public string desc { get; set; }
        public WebhookData data { get; set; }
        public string signature { get; set; }
    }

    public class WebhookData
    {
        public long orderCode { get; set; }
        public int amount { get; set; }
        public string description { get; set; }
        public int status { get; set; }
        public long transactionId { get; set; }
        public long paymentLinkId { get; set; }
        public string checkoutUrl { get; set; }
        public string qrCode { get; set; }
    }
}