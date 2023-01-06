using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BlissRecruitment.Core.Attributes;
using FluentValidation;

namespace BlissRecruitment.Core.Models.Requests.Questions;

public sealed class CreateQuestionRequest
{
    [Required(AllowEmptyStrings = false)] public string Question { get; set; }

    [Required(AllowEmptyStrings = false), JsonPropertyName("image_url"), Url]
    public string ImageUrl { get; set; }

    [Required(AllowEmptyStrings = false), JsonPropertyName("thumb_url"), Url]
    public string ThumbUrl { get; set; }
    
    [AllowDuplicatesStrings(false, 2)] // Prevent choices and must provide at least 2 choices
    public string[] Choices { get; set; } = Array.Empty<string>();
}

public class CreateQuestionRequestValidator : AbstractValidator<CreateQuestionRequest>
{
    public CreateQuestionRequestValidator()
    {
        RuleFor(x => x.Question).NotNull().NotEmpty();
        RuleFor(x => x.ImageUrl)
            .Must(x => !string.IsNullOrEmpty(x) && 
                       (x.Contains("http://") || x.Contains("https://")));
        RuleFor(x => x.ThumbUrl)
            .Must(x => !string.IsNullOrEmpty(x) && 
                       (x.Contains("http://") || x.Contains("https://")));
        RuleFor(x => x.Choices)
            .Must(x => x is { Length: >= 2 });
    }
}