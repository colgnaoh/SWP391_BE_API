using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CommunityProgramReqModel
{
    public class CommunityProgramUpdateRequest
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Location { get; set; } = default!;
        public Programtype Type { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ProgramImgUrl { get; set; } = default!;
        public string? ProgramVidUrl { get; set; } = default;
        public RiskLevel RiskLevel { get; set; }
    }
}
