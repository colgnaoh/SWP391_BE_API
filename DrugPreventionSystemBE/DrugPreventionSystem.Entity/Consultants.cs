using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class Consultants
    {
    
        public Guid? Id { get; set; }

        public Guid? UserId { get; set; }
        public User? User { get; set; } 
        
        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Qualifications { get; set; }
        public string? JobTitle { get; set; }

        public DateTime? HireDate { get; set; }

        public decimal? Salary { get; set; }

        public ConsultantStatus? Status { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
