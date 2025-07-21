using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AddFavoriteReqModel
{
    public class AddFavoriteRequestModel
    {
        public Guid TargetId { get; set; } // ProgramId or CourseId
        public FavoriteType TargetType { get; set; }
    }
}
