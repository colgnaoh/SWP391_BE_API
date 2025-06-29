using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using System.ComponentModel.DataAnnotations;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class Order
    {
        [Key]
        public Guid? Id { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? OrderDate { get; set; }
        public OrderStatus? Status { get; set; }
        public Guid? CartId { get; set; }
        public Cart? Cart { get; set; }
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<OrderLog> OrderLogs { get; set; } = new List<OrderLog>();

    }
}
