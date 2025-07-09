using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CommunityProgramsResModel
{
    public class EnrollmentHistoryItem
    {
        public Guid ProgramId { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Location { get; set; } = default!;
        public Programtype? Type { get; set; } = default!;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ProgramImgUrl { get; set; }
        public string? ProgramVidUrl { get; set; }
        public RiskLevel? RiskLevel { get; set; }
        public DateTime? JoinDate { get; set; }
        public int? UserCount { get; set; }
    }

}
