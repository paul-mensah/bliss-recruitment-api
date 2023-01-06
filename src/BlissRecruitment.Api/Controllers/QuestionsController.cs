using System.Net.Mime;
using BlissRecruitment.Core.Models.Filters;
using BlissRecruitment.Core.Models.Requests;
using BlissRecruitment.Core.Models.Requests.Questions;
using BlissRecruitment.Core.Models.Responses.Questions;
using BlissRecruitment.Core.Services.Questions;
using Microsoft.AspNetCore.Mvc;

namespace BlissRecruitment.Api.Controllers;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusOnlyResponse))]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionsService _questionsService;

    public QuestionsController(IQuestionsService questionsService)
    {
        _questionsService = questionsService;
    }

    /// <summary>
    /// Create a new question
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(QuestionResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusOnlyResponse))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(StatusOnlyResponse))]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequest request)
    {
        var response = await _questionsService.CreateQuestion(request);
        return response.IsSuccessful() 
            ? CreatedAtAction(nameof(GetQuestionDetails), new { questionId = response.Data.Id }, response.Data) 
            : StatusCode(response.Code, new StatusOnlyResponse
            {
                Status = response.Status
            });
    }
    
    /// <summary>
    /// Retrieve a question
    /// </summary>
    /// <param name="questionId"></param>
    /// <returns></returns>
    [HttpGet("{questionId:required}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(QuestionResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusOnlyResponse))]
    public async Task<IActionResult> GetQuestionDetails([FromRoute] string questionId)
    {
        var response = await _questionsService.GetQuestionById(questionId);

        return response.IsSuccessful()
            ? Ok(response.Data)
            : StatusCode(response.Code, new StatusOnlyResponse
            {
                Status = response.Status
            });
    }

    /// <summary>
    /// List all questions
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<QuestionResponse>))]
    public async Task<IActionResult> GetAllQuestions([FromQuery] QuestionsFilter questionsFilter)
    {
        var response = await _questionsService.GetQuestions(questionsFilter);

        return response.IsSuccessful()
            ? Ok(response.Data)
            : StatusCode(response.Code, new StatusOnlyResponse
            {
                Status = response.Status
            });
    }

    /// <summary>
    /// Update a question
    /// </summary>
    /// <param name="questionId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{questionId:required}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(QuestionResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusOnlyResponse))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(StatusOnlyResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusOnlyResponse))]
    public async Task<IActionResult> UpdateQuestion([FromRoute] string questionId, [FromBody] UpdateQuestionRequest request)
    {
        var response = await _questionsService.UpdateQuestion(questionId, request);

        return response.IsSuccessful()
            ? CreatedAtAction(nameof(GetQuestionDetails), new { questionId }, response.Data)
            : StatusCode(response.Code, new StatusOnlyResponse
            {
                Status = response.Status
            });
    }
}