using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ConsultantModel
{
    public class ConsultantResponseModel
    {
        public Guid? Id { get; set; }
        public Guid? UserId { get; set; }
        public string? FullName { get; set; }
        public List<string> Qualifications { get; set; }
        public string JobTitle { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public ConsultantStatus Status { get; set; }
        public string? Email { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? ProfilePicUrl { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
