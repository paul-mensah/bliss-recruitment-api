using System.Text.Json.Serialization;
using BlissRecruitment.Core.Domain;

namespace BlissRecruitment.Core.Models.Responses.Questions;

public class QuestionResponse
{
    public string Id { get; set; }
    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; }
    [JsonPropertyName("thumb_url")]
    public string ThumbUrl { get; set; }
    public string Question { get; set; }
    public QuestionChoice[] Choices { get; set; } = Array.Empty<QuestionChoice>();
    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }
}