using DrugPreventionSystemBE.DrugPreventionSystem.Core;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class Blog : BaseEnity
    {
        public Guid UserId { get; set; }    
        public string? Content { get; set; }
        public string? BlogImgUrl { get; set; }
    }
}
