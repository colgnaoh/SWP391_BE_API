using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using System.ComponentModel.DataAnnotations.Schema;

public class Question : BaseEnity
{
    public Guid? SurveyId { get; set; }
    public Survey? Survey { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? QuestionContent { get; set; }

    public QuestionType? QuestionType { get; set; }

    public int? PositionOrder { get; set; }

    public ICollection<AnswerOption> AnswerOptions { get; set; }
    public ICollection<UserAnswerLog> UserAnswerLogs { get; set; }
}
