using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using System.ComponentModel.DataAnnotations.Schema;

public class Survey : BaseEnity
{
    [Column(TypeName = "nvarchar(100)")]
    public string? Name { get; set; }

    [Column(TypeName = "nvarchar(100)")]
    public string? Description { get; set; }

    public SurveyType? Type { get; set; }  // Enum

    public ICollection<Question> Questions { get; set; }
    public ICollection<SurveyResult> SurveyResults { get; set; }
}
