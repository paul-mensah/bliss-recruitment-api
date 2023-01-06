using BlissRecruitment.Core.Models.Filters;
using BlissRecruitment.Core.Models.Requests.Questions;
using BlissRecruitment.Core.Models.Responses;
using BlissRecruitment.Core.Models.Responses.Questions;

namespace BlissRecruitment.Core.Services.Questions;

public interface IQuestionsService
{
    Task<BaseResponse<QuestionResponse>> CreateQuestion(CreateQuestionRequest request);
    Task<BaseResponse<QuestionResponse>> UpdateQuestion(string questionId, UpdateQuestionRequest request);
    Task<BaseResponse<QuestionResponse>> GetQuestionById(string questionId);
    Task<BaseResponse<IEnumerable<QuestionResponse>>> GetQuestions(QuestionsFilter filters);
}