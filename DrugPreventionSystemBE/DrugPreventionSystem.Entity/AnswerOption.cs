using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class AnswerOption : BaseEnity
{
    public Guid? QuestionId { get; set; }
    public Question? Question { get; set; }

    public string? OptionContent { get; set; }

    public int? Score { get; set; }

    public int? PositionOrder { get; set; }

    public ICollection<UserAnswerLog> UserAnswerLogs { get; set; } = new List<UserAnswerLog>();

}
