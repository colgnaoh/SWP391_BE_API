using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class OrderDetail : BaseEnity
    {
        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }
        public ServiceType? ServiceType { get; set; }
        public decimal? Amount { get; set; }
        public Guid? TransactionId { get; set; }
        public Transaction? Transaction { get; set; }
        public Course Course { get; set; }
        public Guid? CourseId { get; set; }
    }
}
