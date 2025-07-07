using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ConsultantModel
{
    public class ConsultantUpdateRequest
    {
        public List<string> Qualifications { get; set; } = new List<string>();

        public string? JobTitle { get; set; }

        public DateTime? HireDate { get; set; }


        public decimal? Salary { get; set; }

        public ConsultantStatus? Status { get; set; }
    }
}
