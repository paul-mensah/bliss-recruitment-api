using AutoMapper;
using BlissRecruitment.Core.Domain;
using BlissRecruitment.Core.Models.Filters;
using BlissRecruitment.Core.Models.Requests.Questions;
using BlissRecruitment.Core.Models.Responses;
using BlissRecruitment.Core.Models.Responses.Questions;
using BlissRecruitment.Core.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BlissRecruitment.Core.Services.Questions;

public class QuestionsService : IQuestionsService
{
    private readonly IQuestionsRepository _questionsRepository;
    private readonly ILogger<QuestionsService> _logger;
    private readonly IMapper _mapper;

    public QuestionsService(IQuestionsRepository questionsRepository,
        ILogger<QuestionsService> logger,
        IMapper mapper)
    {
        _questionsRepository = questionsRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<BaseResponse<QuestionResponse>> CreateQuestion(CreateQuestionRequest request)
    {
        try
        {
            var validationResult = await new CreateQuestionRequestValidator().ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return CommonResponses.ErrorResponse
                    .BadRequestResponse<QuestionResponse>("Bad Request. All fields are mandatory.");
            }
            
            var newQuestion = _mapper.Map<QuestionEntity>(request);
            bool addedSuccessfully = await _questionsRepository.AddAsync(newQuestion);

            return !addedSuccessfully
                ? CommonResponses.ErrorResponse.FailedDependencyErrorResponse<QuestionResponse>()
                : CommonResponses.SuccessResponse.CreatedResponse(_mapper.Map<QuestionResponse>(newQuestion));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured creating a question\n{request}",
                JsonConvert.SerializeObject(request, Formatting.Indented));

            return CommonResponses.ErrorResponse.InternalServerErrorResponse<QuestionResponse>();
        }
    }

    public async Task<BaseResponse<QuestionResponse>> UpdateQuestion(string questionId, UpdateQuestionRequest request)
    {
        try
        {
            var validationResult = await new UpdateQuestionRequestValidator().ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return CommonResponses.ErrorResponse
                    .BadRequestResponse<QuestionResponse>("Bad Request. All fields are mandatory.");
            }
            
            // Ensure the question exists
            var question = await _questionsRepository.GetById(questionId);

            if (question is null)
            {
                return CommonResponses.ErrorResponse.NotFoundResponse<QuestionResponse>("Question not found");
            }

            // Update question
            question.Question = request.Question;
            question.ImageUrl = request.ImageUrl;
            question.ThumbUrl = request.ThumbUrl;
            
            // Update votes of only choices which exists in question
            question.Choices = question.Choices
                .Select(x =>
                {
                    var choiceResponse = request.Choices.FirstOrDefault(rc => rc.Choice.Equals(x.Choice));

                    if (choiceResponse is not null)
                    {
                        x.Votes = choiceResponse.Votes;
                    }

                    return x;
                }).ToArray();

            bool isUpdated = await _questionsRepository.UpdateAsync(question);

            return isUpdated
                ? CommonResponses.SuccessResponse.CreatedResponse(_mapper.Map<QuestionResponse>(question))
                : CommonResponses.ErrorResponse.FailedDependencyErrorResponse<QuestionResponse>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{questionId}: An error occured updating question\n{request}",
                questionId, JsonConvert.SerializeObject(request));

            return CommonResponses.ErrorResponse.InternalServerErrorResponse<QuestionResponse>();
        }
    }

    public async Task<BaseResponse<QuestionResponse>> GetQuestionById(string questionId)
    {
        try
        {
            var question = await _questionsRepository.GetById(questionId);

            return question is not null
                ? CommonResponses.SuccessResponse.OkResponse(_mapper.Map<QuestionResponse>(question))
                : CommonResponses.ErrorResponse.NotFoundResponse<QuestionResponse>("Question not found");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{questionId}: An error occured getting question", questionId);
            return CommonResponses.ErrorResponse.InternalServerErrorResponse<QuestionResponse>();
        }
    }

    public async Task<BaseResponse<IEnumerable<QuestionResponse>>> GetQuestions(QuestionsFilter filters)
    {
        try
        {
            var questionsEnumerable = await _questionsRepository.GetQuestionsByFilter(filters);

            if (!questionsEnumerable.Any())
            {
                return CommonResponses.SuccessResponse.OkResponse(Array.Empty<QuestionResponse>().AsEnumerable());
            }

            var questionResponsesEnumerable = _mapper.Map<IEnumerable<QuestionResponse>>(questionsEnumerable);
            return CommonResponses.SuccessResponse.OkResponse(questionResponsesEnumerable);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured getting questions with filter\n{filter}",
                JsonConvert.SerializeObject(filters, Formatting.Indented));

            return CommonResponses.ErrorResponse.InternalServerErrorResponse<IEnumerable<QuestionResponse>>();
        }
    }
}