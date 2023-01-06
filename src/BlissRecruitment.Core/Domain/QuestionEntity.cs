using System.ComponentModel.DataAnnotations.Schema;

namespace BlissRecruitment.Core.Domain;

public sealed class QuestionEntity : EntityBase
{
    public string Question { get; set; }
    public string ImageUrl { get; set; }
    public string ThumbUrl { get; set; }
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    [Column(TypeName = "jsonb")] 
    public QuestionChoice[] Choices { get; set; } = Array.Empty<QuestionChoice>();
}