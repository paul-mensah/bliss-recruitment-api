using System.Net;
using BlissRecruitment.Api.Controllers;
using BlissRecruitment.Core.Domain;
using BlissRecruitment.Core.Models.Filters;
using BlissRecruitment.Core.Models.Requests;
using BlissRecruitment.Core.Models.Requests.Questions;
using BlissRecruitment.Core.Models.Responses;
using BlissRecruitment.Core.Models.Responses.Questions;
using BlissRecruitment.Core.Services.Questions;
using BlissRecruitment.Tests.TestSetup;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BlissRecruitment.Tests.Questions;

public class QuestionsControllerShould : IClassFixture<TestFixture>
{
    private readonly Mock<IQuestionsService> _questionsServiceMock;
    private readonly Faker _faker = new Faker();

    public QuestionsControllerShould()
    {
        _questionsServiceMock = new Mock<IQuestionsService>();
    }

    private QuestionsController GetQuestionsController()
    {
        return new QuestionsController(_questionsServiceMock.Object);
    }

    private QuestionResponse GetMockQuestionResponse()
    {
        return new QuestionResponse
        {
            Id = Guid.NewGuid().ToString("N"),
            Question = _faker.Random.Words(4),
            ImageUrl = _faker.Image.PlaceholderUrl(720, 720),
            ThumbUrl = _faker.Image.PlaceholderUrl(256, 256),
            Choices = new[]
            {
                new QuestionChoice { Choice = _faker.Music.Genre(), Votes = _faker.Random.Number(50)},
                new QuestionChoice { Choice = _faker.Music.Genre(), Votes = _faker.Random.Number(50)},
                new QuestionChoice { Choice = _faker.Music.Genre(), Votes = _faker.Random.Number(50)},
                new QuestionChoice { Choice = _faker.Music.Genre(), Votes = _faker.Random.Number(50)}
            },
            PublishedAt = _faker.Date.Recent()
        };
    }
    
    [Fact]
    public async Task Return_CreatedResponse_When_All_Requirements_Are_Met()
    {
        // Arrange
        var mockQuestionResponse = GetMockQuestionResponse();

        _questionsServiceMock.Setup(x => x.CreateQuestion(It.IsAny<CreateQuestionRequest>()))
            .ReturnsAsync(new BaseResponse<QuestionResponse>
            {
                Data = mockQuestionResponse,
                Code = (int) HttpStatusCode.Created,
                Status = "Created successfully"
            });

        var createQuestionRequest = new CreateQuestionRequest
        {
            Choices = mockQuestionResponse.Choices.Select(x => x.Choice).ToArray(),
            Question = mockQuestionResponse.Question,
            ImageUrl = mockQuestionResponse.ImageUrl,
            ThumbUrl = mockQuestionResponse.ThumbUrl
        };
        
        var questionsController = GetQuestionsController();

        // Act
        var response = (ObjectResult) await questionsController.CreateQuestion(createQuestionRequest);

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.Created);

        var parsedResponse = response.Value as QuestionResponse;
        parsedResponse.Should().NotBeNull();
        parsedResponse?.Id.Should().NotBeNullOrEmpty();
        parsedResponse?.Question.Should().NotBeNullOrEmpty();
        parsedResponse?.ImageUrl.Should().NotBeNullOrEmpty();
        parsedResponse?.ThumbUrl.Should().NotBeNullOrEmpty();
        parsedResponse?.Choices.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Return_BadRequest_When_Invalid_Or_Empty_Parameters_Are_Passed()
    {
        // Arrange
        _questionsServiceMock.Setup(x => x.CreateQuestion(It.IsAny<CreateQuestionRequest>()))
            .ReturnsAsync(new BaseResponse<QuestionResponse>
            {
                Code = (int) HttpStatusCode.BadRequest,
                Status = "Bad Request. All fields are mandatory."
            });
        
        var questionsController = GetQuestionsController();

        // Act
        var response = (ObjectResult) await questionsController.CreateQuestion(new CreateQuestionRequest());

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        
        var parsedResponse = response.Value as StatusOnlyResponse;
        parsedResponse?.Status.Should().NotBeNullOrEmpty();
        parsedResponse?.Status.Should().Be("Bad Request. All fields are mandatory.");
    }

    [Fact]
    public async Task Return_NotFoundResponse_When_There_No_Record_With_Matching_Id()
    {
        // Arrange
        _questionsServiceMock.Setup(x => x.GetQuestionById(It.IsAny<string>()))
            .ReturnsAsync(new BaseResponse<QuestionResponse>
            {
                Code = (int) HttpStatusCode.NotFound
            });

        var questionController = GetQuestionsController();

        // Act
        var response = (ObjectResult) await questionController.GetQuestionDetails(Guid.NewGuid().ToString("N"));

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Return_QuestionResponse_When_There_Is_A_Record_With_Matching_Id()
    {
        // Arrange
        var questionResponse = GetMockQuestionResponse();
        
        _questionsServiceMock.Setup(x => x.GetQuestionById(It.IsAny<string>()))
            .ReturnsAsync(new BaseResponse<QuestionResponse>
            {
                Code = (int) HttpStatusCode.OK,
                Data = questionResponse
            });

        var questionController = GetQuestionsController();

        // Act
        var response = (ObjectResult) await questionController.GetQuestionDetails(questionResponse.Id);

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);

        var parsedResponse = response.Value as QuestionResponse;
        parsedResponse.Should().NotBeNull();
        parsedResponse?.Id.Should().Be(questionResponse.Id);
        parsedResponse?.Choices.Length.Should().Be(questionResponse.Choices.Length);
        parsedResponse?.ThumbUrl.Should().Be(parsedResponse.ThumbUrl);
        parsedResponse?.ImageUrl.Should().Be(parsedResponse.ImageUrl);
    }

    [Fact]
    public async Task Return_List_Of_Questions()
    {
        // Arrange
        var questionResponse = GetMockQuestionResponse();

        _questionsServiceMock.Setup(x => x.GetQuestions(It.IsAny<QuestionsFilter>()))
            .ReturnsAsync(new BaseResponse<IEnumerable<QuestionResponse>>
            {
                Code = (int) HttpStatusCode.OK,
                Data = new List<QuestionResponse>{ questionResponse }
            });

        var questionsController = GetQuestionsController();
        
        // Act
        var response = (ObjectResult) await questionsController.GetAllQuestions(new QuestionsFilter());

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task Return_NotFoundResponse_When_No_Record_Exists_Matching_Id()
    {
        // Arrange
        _questionsServiceMock.Setup(x => x.UpdateQuestion(It.IsAny<string>(), It.IsAny<UpdateQuestionRequest>()))
            .ReturnsAsync(new BaseResponse<QuestionResponse>
            {
                Code = (int)HttpStatusCode.NotFound,
                Status = "Question not found"
            });

        var questionsController = GetQuestionsController();

        // Act
        var response = (ObjectResult) await questionsController.UpdateQuestion(Guid.NewGuid().ToString("N"), new UpdateQuestionRequest());

        // Arrange
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Return_CreatedResponse_When_Record_Exists_And_All_Parameters_Valid()
    {
        // Arrange
        var questionResponse = GetMockQuestionResponse();

        _questionsServiceMock.Setup(x => x.UpdateQuestion(It.IsAny<string>(), It.IsAny<UpdateQuestionRequest>()))
            .ReturnsAsync(new BaseResponse<QuestionResponse>
            {
                Code = (int) HttpStatusCode.Created,
                Data = questionResponse
            });

        var updateRequest = new UpdateQuestionRequest
        {
            ThumbUrl = questionResponse.ThumbUrl,
            ImageUrl = questionResponse.ImageUrl,
            Question = questionResponse.Question,
            Choices = questionResponse.Choices
        };
        
        var questionController = GetQuestionsController();

        // Act
        var response = (ObjectResult) await questionController.UpdateQuestion(questionResponse.Id, updateRequest);
        
        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.Created);

        var parsedResponse = response.Value as QuestionResponse;
        parsedResponse?.Id.Should().Be(questionResponse.Id);
        parsedResponse?.ThumbUrl.Should().Be(questionResponse.ThumbUrl);
        parsedResponse?.ImageUrl.Should().Be(questionResponse.ImageUrl);
        parsedResponse?.Question.Should().Be(questionResponse.Question);
    }
}