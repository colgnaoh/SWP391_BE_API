using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class Favorite : BaseEnity
    {
        public Guid UserId { get; set; }
        public Guid TargetId { get; set; } // Can be a CourseId or ProgramId
        public FavoriteType TargetType { get; set; }

        public User User { get; set; } = null!;
    }

}
