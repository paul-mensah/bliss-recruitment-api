using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BlissRecruitment.Core.Domain;
using FluentValidation;

namespace BlissRecruitment.Core.Models.Requests.Questions;

public class UpdateQuestionRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Question { get; set; }
    [Required(AllowEmptyStrings = false), JsonPropertyName("image_url"), Url]
    public string ImageUrl { get; set; }
    [Required(AllowEmptyStrings = false), JsonPropertyName("thumb_url"), Url]
    public string ThumbUrl { get; set; }
    public QuestionChoice[] Choices { get; set; } = Array.Empty<QuestionChoice>();
}

public class UpdateQuestionRequestValidator : AbstractValidator<UpdateQuestionRequest>
{
    public UpdateQuestionRequestValidator()
    {
        RuleFor(x => x.Question).NotNull().NotEmpty();
        RuleFor(x => x.ImageUrl)
            .Must(x => !string.IsNullOrEmpty(x) && 
                       (x.Contains("http://") || x.Contains("https://")));
        RuleFor(x => x.ThumbUrl)
            .Must(x => !string.IsNullOrEmpty(x) && 
                       (x.Contains("http://") || x.Contains("https://")));
        RuleFor(x => x.Choices )
            .Must(x => x != null &&
                x.All(c => !string.IsNullOrEmpty(c.Choice) ));
    }
}