using AutoMapper;
using BlissRecruitment.Core.Domain;
using BlissRecruitment.Core.Models.Requests.Questions;
using BlissRecruitment.Core.Models.Responses.Questions;

namespace BlissRecruitment.Api.Mappings;

public class AutoMapper : Profile
{
    public AutoMapper()
    {
        CreateMap<CreateQuestionRequest, QuestionEntity>()
            .ForMember(des => des.Choices,
            option => option.MapFrom(src => src.Choices
                .Distinct()
                .Select(choice => new QuestionChoice
                {
                    Choice = choice,
                    Votes = 0
                })
                .ToArray()));

        CreateMap<QuestionEntity, QuestionResponse>().ReverseMap();
    }
}