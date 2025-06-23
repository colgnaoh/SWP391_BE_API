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
        public User? User { get; set; } // Assuming a User model exists

        [Required]
        
        public string? FullName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        // Array is typically modeled as a List or string (e.g., JSON-encoded) in EF
        public List<string> Qualifications { get; set; } = new List<string>();

        
        public string? JobTitle { get; set; }

        public DateTime? HireDate { get; set; }

        
        public decimal? Salary { get; set; }

        public ConsultantStatus? Status { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
