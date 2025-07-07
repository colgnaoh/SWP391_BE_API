using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using System.ComponentModel.DataAnnotations;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class Order : BaseEnity
    {
        public decimal TotalAmount { get; set; }
        public DateTime? OrderDate { get; set; }
        public OrderStatus? Status { get; set; }
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public Payment? Payment { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<OrderLog> OrderLogs { get; set; } = new List<OrderLog>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
    }
}
