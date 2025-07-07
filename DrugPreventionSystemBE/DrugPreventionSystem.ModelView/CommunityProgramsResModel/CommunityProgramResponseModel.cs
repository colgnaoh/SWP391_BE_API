using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CommunityProgramResModel
{
    public class CommunityProgramResponseModel
    {
        //public bool Success { get; set; }
        public string? Message { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public Programtype? Type { get; set; }
        public string? ProgramImgUrl { get; set; }
        public string? ProgramVidUrl { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public string? Location { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
