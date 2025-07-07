using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.VnPayPayment
{
    public class PaymentRequestVnPay
    {
        public long PaymentId { get; set; } // Your unique order ID
        public double Money { get; set; } // Amount to pay
        public string Description { get; set; } // Order description
        public string IpAddress { get; set; }
        public BankCode BankCode { get; set; } = BankCode.ANY;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public Currency Currency { get; set; } = Currency.VND;
        public DisplayLanguage Language { get; set; } = DisplayLanguage.Vietnamese;
    }
}
