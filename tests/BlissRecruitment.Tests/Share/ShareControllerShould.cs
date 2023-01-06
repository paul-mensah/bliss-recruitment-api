using System.Net;
using BlissRecruitment.Api.Controllers;
using BlissRecruitment.Core.Models.Requests;
using BlissRecruitment.Core.Models.Responses;
using BlissRecruitment.Core.Services.Email;
using BlissRecruitment.Tests.TestSetup;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BlissRecruitment.Tests.Share;

public class ShareControllerShould : IClassFixture<TestFixture>
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Faker _faker = new Faker();

    public ShareControllerShould()
    {
        _emailServiceMock = new Mock<IEmailService>();
    }

    private ShareController GetShareController()
    {
        return new ShareController(_emailServiceMock.Object);
    }
    
    [Theory]
    [InlineData("", "https://localhost:5000/questions/1234556")]
    [InlineData("paulmensah@gmail", "")]
    public async Task Return_BadRequest_When_All_Required_Are_Not_Provided(string destinationEmail, string contentUrl)
    {
        // Arrange
        var shareController = GetShareController();
        
        // Act
        var response = (ObjectResult) await shareController.Share(destinationEmail, contentUrl);

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        
        var parsedResponse = response.Value as StatusOnlyResponse;
        parsedResponse.Should().NotBeNull();
        parsedResponse?.Status.Should().NotBeNullOrEmpty();
        parsedResponse?.Status.Should().Be("Bad Request. Either destination_email not valid or empty content_url");
    }
    
    [Fact]
    public async Task Return_Ok_Success_Response_When_Sending_Email_Is_Successful()
    {
        // Arrange
        _emailServiceMock.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new BaseResponse<bool>
            {
                Code = (int) HttpStatusCode.OK,
                Status = "Email sent successfully",
                Data = true
            });
        
        var shareController = GetShareController();
        
        // Act
        var response = (ObjectResult) await shareController.Share(_faker.Person.Email, "test content");

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);

        var parsedResponse = response.Value as StatusOnlyResponse;
        parsedResponse.Should().NotBeNull();
        parsedResponse?.Status.Should().Be("OK");
    }
    
    [Fact]
    public async Task Return_FailedDependency_Response_When_Sending_Email_Is_Not_Successful()
    {
        // Arrange
        var mockResponseFromEmailService = new BaseResponse<bool>
        {
            Code = (int)HttpStatusCode.FailedDependency,
            Status = "An error occured, try again later",
            Data = false
        };
        
        _emailServiceMock.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(mockResponseFromEmailService);
        
        var shareController = GetShareController();
        
        // Act
        var response = (ObjectResult) await shareController.Share("destinationEmail", "contentUrl");

        // Assert
        response.StatusCode.Should().Be(mockResponseFromEmailService.Code);

        var parsedResponse = response.Value as StatusOnlyResponse;
        parsedResponse.Should().NotBeNull();
        parsedResponse?.Status.Should().Be(mockResponseFromEmailService.Status);
    }
}