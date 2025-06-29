using DrugPreventionSystemBE.DrugPreventionSystem.Core;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class OrderLog : BaseEnity
    {
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public Guid? CartId { get; set; }
        public Cart? Cart { get; set; }
        public string? Action { get; set; } 
        public string? OldStatus { get; set; } 
        public string? NewStatus { get; set; } 
        public string? Note { get; set; } 

        public Guid? UserId { get; set; }
        public User? User { get; set; } // Assuming a User model exists 
    }
}