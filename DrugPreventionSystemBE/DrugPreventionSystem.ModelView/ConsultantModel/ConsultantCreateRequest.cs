using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ConsultantModel
{
    public class ConsultantCreateRequest
    {
        public Guid? UserId { get; set; }

        public List<string> Qualifications { get; set; } = new List<string>();

        public string? JobTitle { get; set; }

        public DateTime? HireDate { get; set; }


        public decimal? Salary { get; set; }

        public ConsultantStatus? Status { get; set; }
    }
}
