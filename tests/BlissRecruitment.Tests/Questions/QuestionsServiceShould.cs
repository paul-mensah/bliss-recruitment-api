using System.Net;
using AutoMapper;
using BlissRecruitment.Core.Domain;
using BlissRecruitment.Core.Models.Requests.Questions;
using BlissRecruitment.Core.Models.Responses.Questions;
using BlissRecruitment.Core.Repositories;
using BlissRecruitment.Core.Services.Questions;
using BlissRecruitment.Tests.TestSetup;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BlissRecruitment.Tests.Questions;

public class QuestionsServiceShould : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly Mock<IQuestionsRepository> _questionRepositoryMock;
    private readonly Faker _faker = new Faker();

    public QuestionsServiceShould(TestFixture fixture)
    {
        _fixture = fixture;
        _questionRepositoryMock = new Mock<IQuestionsRepository>();
    }

    private QuestionsService GetQuestionsService()
    {
        var logger = _fixture.ServiceProvider.GetService<ILogger<QuestionsService>>();
        var mapper = _fixture.ServiceProvider.GetService<IMapper>();

        return new QuestionsService(_questionRepositoryMock.Object, logger, mapper);
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
    public async Task Return_BadRequestResponse_When_Invalid_Values_Are_Passed_To_CreateQuestion_Method()
    {
        // Arrange
        var questionsService = GetQuestionsService();

        // Act
        var response = await questionsService.CreateQuestion(new CreateQuestionRequest());

        // Assert
        response.Code.Should().Be((int)HttpStatusCode.BadRequest);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Bad Request. All fields are mandatory.");
    }

    [Fact]
    public async Task Return_CreatedResponse_When_Adding_New_Question_To_Database_Is_Successful()
    {
        // Arrange
        var questionResponse = GetMockQuestionResponse();
        
        _questionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<QuestionEntity>()))
            .ReturnsAsync(true);

        var questionsService = GetQuestionsService();
        
        // Act
        var response = await questionsService.CreateQuestion(new CreateQuestionRequest
        {
            Choices = questionResponse.Choices.Select(x=> x.Choice).ToArray(),
            Question = questionResponse.Question,
            ImageUrl = questionResponse.ImageUrl,
            ThumbUrl = questionResponse.ThumbUrl
        });

        // Assert
        response.Code.Should().Be((int)HttpStatusCode.Created);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Created successfully");
        response.Data.Id.Should().NotBeNullOrEmpty();
        response.Data.ThumbUrl.Should().Be(questionResponse.ThumbUrl);
        response.Data.ImageUrl.Should().Be(questionResponse.ImageUrl);
        response.Data.Question.Should().Be(questionResponse.Question);
        foreach (var questionChoice in response.Data.Choices)
        {
            questionChoice.Choice.Should().NotBeNullOrEmpty();
            questionChoice.Votes.Should().Be(0);
        }
    }

    [Fact]
    public async Task Return_FailedDependencyResponse_When_Adding_New_Question_To_Database_Fails()
    {
        // Arrange
        var questionResponse = GetMockQuestionResponse();
        
        _questionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<QuestionEntity>()))
            .ReturnsAsync(false);

        var questionsService = GetQuestionsService();
        
        // Act
        var response = await questionsService.CreateQuestion(new CreateQuestionRequest
        {
            Choices = questionResponse.Choices.Select(x=> x.Choice).ToArray(),
            Question = questionResponse.Question,
            ImageUrl = questionResponse.ImageUrl,
            ThumbUrl = questionResponse.ThumbUrl
        });

        // Assert
        response.Code.Should().Be((int)HttpStatusCode.FailedDependency);
        response.Status.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Return_BadRequestResponse_When_Invalid_Values_Are_Passed_To_UpdateQuestion_Method()
    {
        // Arrange
        var questionsService = GetQuestionsService();

        // Act
        var response = await questionsService.UpdateQuestion(Guid.NewGuid().ToString("N"), new UpdateQuestionRequest());

        // Assert
        response.Code.Should().Be((int)HttpStatusCode.BadRequest);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Bad Request. All fields are mandatory.");
    }
    
    [Fact]
    public async Task Return_NotFoundResponse_When_Updating_Record_Which_Does_Not_Exist_In_Database()
    {
        // Arrange
        var questionResponse = GetMockQuestionResponse();
        
        _questionRepositoryMock.Setup(x => x.GetById(It.IsAny<string>()))
            .ReturnsAsync(() => null);

        var questionsService = GetQuestionsService();
        
        // Act
        var response = await questionsService.UpdateQuestion(questionResponse.Id, new UpdateQuestionRequest
        {
            Choices = questionResponse.Choices,
            Question = questionResponse.Question,
            ImageUrl = questionResponse.ImageUrl,
            ThumbUrl = questionResponse.ThumbUrl
        });

        // Assert
        response.Code.Should().Be((int)HttpStatusCode.NotFound);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Question not found");
    }
    
    [Fact]
    public async Task Return_CreatedResponse_When_Updating_An_Existing_Question_Is_Successful()
    {
        // Arrange
        var questionResponse = GetMockQuestionResponse();
        
        _questionRepositoryMock.Setup(x => x.GetById(It.IsAny<string>()))
            .ReturnsAsync(new QuestionEntity
            {
                Choices = questionResponse.Choices,
                Question = questionResponse.Question,
                ImageUrl = questionResponse.ImageUrl,
                ThumbUrl = questionResponse.ThumbUrl, 
                PublishedAt = questionResponse.PublishedAt,
                Id = questionResponse.Id
            });

        _questionRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<QuestionEntity>()))
            .ReturnsAsync(() => true);

        var questionsService = GetQuestionsService();
        
        // Act
        var response = await questionsService.UpdateQuestion(questionResponse.Id, new UpdateQuestionRequest
        {
            Choices = questionResponse.Choices,
            Question = questionResponse.Question,
            ImageUrl = questionResponse.ImageUrl,
            ThumbUrl = questionResponse.ThumbUrl
        });

        // Assert
        response.Code.Should().Be((int)HttpStatusCode.Created);
        response.Status.Should().NotBeNullOrEmpty();
        response.Data.ImageUrl.Should().Be(questionResponse.ImageUrl);
        response.Data.ThumbUrl.Should().Be(questionResponse.ThumbUrl);
        response.Data.Question.Should().Be(questionResponse.Question);
    }
    
    [Fact]
    public async Task Return_FailedDependencyResponse_When_Updating_An_Existing_Question_Fails()
    {
        // Arrange
        var questionResponse = GetMockQuestionResponse();
        
        _questionRepositoryMock.Setup(x => x.GetById(It.IsAny<string>()))
            .ReturnsAsync(new QuestionEntity
            {
                Choices = questionResponse.Choices,
                Question = questionResponse.Question,
                ImageUrl = questionResponse.ImageUrl,
                ThumbUrl = questionResponse.ThumbUrl, 
                PublishedAt = questionResponse.PublishedAt,
                Id = questionResponse.Id
            });

        _questionRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<QuestionEntity>()))
            .ReturnsAsync(() => false);

        var questionsService = GetQuestionsService();
        
        // Act
        var response = await questionsService.UpdateQuestion(questionResponse.Id, new UpdateQuestionRequest
        {
            Choices = questionResponse.Choices,
            Question = questionResponse.Question,
            ImageUrl = questionResponse.ImageUrl,
            ThumbUrl = questionResponse.ThumbUrl
        });

        // Assert
        response.Code.Should().Be((int)HttpStatusCode.FailedDependency);
        response.Status.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Return_OkResponse_When_GetQuestionById_Returns_An_Existing_Question()
    {
        // Arrange
        var questionResponse = GetMockQuestionResponse();
        
        _questionRepositoryMock.Setup(x => x.GetById(It.IsAny<string>()))
            .ReturnsAsync(new QuestionEntity
            {
                Choices = questionResponse.Choices,
                Question = questionResponse.Question,
                ImageUrl = questionResponse.ImageUrl,
                ThumbUrl = questionResponse.ThumbUrl, 
                PublishedAt = questionResponse.PublishedAt,
                Id = questionResponse.Id
            });

        var questionsService = GetQuestionsService();
        
        // Act
        var response = await questionsService.GetQuestionById(questionResponse.Id);

        // Assert
        response.Code.Should().Be((int)HttpStatusCode.OK);
        response.Status.Should().NotBeNullOrEmpty();
        response.Data.Id.Should().Be(questionResponse.Id);
        response.Data.Question.Should().Be(questionResponse.Question);
        response.Data.ImageUrl.Should().Be(questionResponse.ImageUrl);
        response.Data.ThumbUrl.Should().Be(questionResponse.ThumbUrl);
    }
    
    [Fact]
    public async Task Return_NotFoundResponse_When_GetQuestionById_Does_Not_Return_A_Question_That_Matches_QuestionId()
    {
        // Arrange
        _questionRepositoryMock.Setup(x => x.GetById(It.IsAny<string>()))
            .ReturnsAsync(() => null);

        var questionsService = GetQuestionsService();
        
        // Act
        var response = await questionsService.GetQuestionById(Guid.NewGuid().ToString("N"));

        // Assert
        response.Code.Should().Be((int)HttpStatusCode.NotFound);
        response.Status.Should().NotBeNullOrEmpty();
    }
}