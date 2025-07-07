using DrugPreventionSystemBE.DrugPreventionSystem.Core; 
using DrugPreventionSystemBE.DrugPreventionSystem.Enum; 
using System; 
using System.Collections.Generic; 
using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity 
{
    public class Transaction : BaseEnity 
    {
        public Guid? ConsultantId { get; set; }
        public User? Consultant { get; set; }
        [Required] // Mỗi giao dịch cần có số tiền
        [Column(TypeName = "decimal(18,2)")] // Phạm vi rộng hơn cho số tiền
        public decimal Amount { get; set; } // Non-nullable

        [Required] // Mỗi giao dịch cần có trạng thái
        public TransactionStatus Status { get; set; } // Non-nullable

        [Required] // Mỗi giao dịch cần có loại dịch vụ
        public ServiceType ServiceType { get; set; } // Non-nullable

        // Foreign key to Payment
        public Guid? PaymentId { get; set; }
        public Payment? Payment { get; set; } // Navigation property

        // Foreign key to Course
        public Guid? CourseId { get; set; }
        public Course? Course { get; set; } 
        public Guid? ProgramId { get; set; }
        public CommunityProgram? Program { get; set; } 
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}