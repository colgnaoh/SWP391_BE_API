using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Enity;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class Blog : BaseEnity
    {
        public Guid? UserId { get; set; }    
        public User? User { get; set; }
        public string? Content { get; set; }
        public string? BlogImgUrl { get; set; }
    }
}
