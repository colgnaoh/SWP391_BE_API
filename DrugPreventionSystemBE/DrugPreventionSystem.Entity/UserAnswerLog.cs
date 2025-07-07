using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class UserAnswerLog
{
    [Key]
    public Guid? Id { get; set; }

    public Guid? SurveyResultId { get; set; }
    public SurveyResult? SurveyResult { get; set; }

    public Guid? QuestionId { get; set; }
    public Question? Question { get; set; }

    public Guid? AnswerOptionId { get; set; }
    public AnswerOption? AnswerOption { get; set; }

    public Guid? ProgramId { get; set; }
    public CommunityProgram? Program { get; set; }

    [Column(TypeName = "nvarchar(255)")]
    public string? AnswerValue { get; set; }

    public int? Score { get; set; }
}
