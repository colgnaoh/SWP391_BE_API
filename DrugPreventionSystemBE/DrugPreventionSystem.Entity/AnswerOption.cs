using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class AnswerOption
{
    [Key]
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }
    public Question Question { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string OptionContent { get; set; }

    public int Score { get; set; }

    public int PositionOrder { get; set; }
}
