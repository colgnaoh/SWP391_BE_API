using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using System.ComponentModel.DataAnnotations;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PaymentReq
{
    public class CreatePaymentRequest
    {
        [Required(ErrorMessage = "ID đơn hàng là bắt buộc.")]
        public Guid OrderId { get; set; } 

        [Required(ErrorMessage = "ID người dùng là bắt buộc.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Số tiền là bắt buộc.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc.")]
        public PaymentMethod PaymentMethod { get; set; } 

    }
}
