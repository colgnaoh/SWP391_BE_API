using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;

public class ProgramFavorite : BaseEnity
{
    public Guid UserId { get; set; }
    public Guid ProgramId { get; set; }

    public User User { get; set; }
    public CommunityProgram Program { get; set; }
}
