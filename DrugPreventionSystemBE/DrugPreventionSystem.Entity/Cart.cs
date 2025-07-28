using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class Cart : BaseEnity
    {
        public Guid? UserId { get; set; }
        public User User { get; set; } 
        public Guid? CourseId { get; set; }
        public Course Course { get; set; } 
        public String CartNo { get; set; }
        public CartStatus Status { get; set; }
        public Guid? OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public ICollection<OrderLog> OrderLogs { get; set; } = new List<OrderLog>();
    }
}
