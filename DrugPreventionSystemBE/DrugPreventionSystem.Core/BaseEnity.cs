using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Core
{
    public class BaseEnity
    {
        

        [Key]
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; }

    }
}
