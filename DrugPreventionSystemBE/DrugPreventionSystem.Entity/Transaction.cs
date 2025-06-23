using DrugPreventionSystemBE;
using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using System.ComponentModel.DataAnnotations.Schema;

public class Transaction : BaseEnity
{
    // Foreign key to Consultant
    public Guid? ConsultantId { get; set; }
    public User? Consultant { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? Amount { get; set; }

    public TransactionStatus? Status { get; set; }

    public ServiceType? ServiceType { get; set; }

    // Foreign key to Payment
    public Guid? PaymentId { get; set; }
    public Payment? Payment { get; set; }

    // Foreign key to Course
    public Guid? CourseId { get; set; }
    public Course? Course { get; set; }

    // Foreign key to Program
    public Guid? ProgramId { get; set; }
    public CommunityProgram? Program { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

}
