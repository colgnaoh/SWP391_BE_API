using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class Payment : BaseEnity
    {
        public string PaymentNo { get; set; }
        public PaymentStatus Status { get; set; }
        public Guid? UserId { get; set; }
        public User? User { get; set; }
        public Guid? OrderId { get; set; } 
        public Order? Order { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
        public string? ExternalTransactionId { get; set; }
        public string? StripeCheckoutUrl { get; set; }


    }
}
