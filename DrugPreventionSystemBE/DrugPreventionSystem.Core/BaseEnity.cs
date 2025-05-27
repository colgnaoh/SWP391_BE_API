using System.ComponentModel.DataAnnotations;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Core
{
    public class BaseEnity
    {
        //protected BaseEntity()
        //{
        //    Id = Guid.NewGuid().ToString("N");
        //    CreatedTime = LastUpdatedTime = CoreHelper.SystemTimeNow;
        //}

        [Key]
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
