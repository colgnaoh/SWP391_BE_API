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
        public string? PaymentNo { get; set; }
        public paymentStatus? Status { get; set; }
        //foreign key to user
        public Guid? UserId { get; set; }
        public User? User { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Amount { get; set; }
        public paymentMethod? PaymentMethod { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal? OrganizationShare { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ConsultantShare { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}
