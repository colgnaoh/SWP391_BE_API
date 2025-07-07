using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PaymentReq;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.VnPayPayment;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Services
{
    public interface IVnPayService
    {
        public string GetPaymentUrl(PaymentRequestVnPay request);

        public bool ProcessVnPayReturn(VnPayReturnResponse response);

        public VnPayIpnResult ProcessVnPayIpn(VnPayIpnResponse response);
    }
}
