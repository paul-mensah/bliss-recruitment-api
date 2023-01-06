using BlissRecruitment.Core.Options;
using BlissRecruitment.Core.Services.Email;
using BlissRecruitment.Tests.TestSetup;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Xunit;

namespace BlissRecruitment.Tests.Share;

public class EmailServiceShould : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly Faker _faker = new Faker();
    
    public EmailServiceShould(TestFixture fixture)
    {
        _fixture = fixture;
    }

    private EmailService GetEmailService()
    {
        var logger = _fixture.ServiceProvider.GetService<ILogger<EmailService>>();
        var emailConfiguration = _fixture.ServiceProvider.GetService<IOptions<EmailConfiguration>>();

        return new EmailService(emailConfiguration, logger);
    }

    [Theory]
    [InlineData("", "This is a test content", "Test Email")]
    [InlineData("paulmensah@gmail.com", "", "Test Email")]
    [InlineData("paulmensah@gmail.com", "This is a test content", "")]
    public async Task Return_BadRequestResponse_When_Any_Of_The_Method_Parameter_Is_Null_Or_Empty(string email, string content, string subject)
    {
        // Arrange
        var emailService = GetEmailService();
        
        // Act
        var response = await emailService.SendEmail(email, content, subject);

        // Asset
        response.Code.Should().Be(400);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Email, content and subject are required");
        response.Data.Should().BeFalse();
    }

    [Fact]
    public void Return_True_And_MimeMessage_When_All_EmailConfiguration_And_Parameters_Are_Not_Null()
    {
        // Arrange
        string email = _faker.Person.Email;
        string content = _faker.Random.String2(5);
        string subject = _faker.Random.String2(2);

        var emailService = GetEmailService();

        // Act
        (bool successful, var mail) = emailService.CreateMessage(email, content, subject);

        // Assert
        successful.Should().BeTrue();
        mail.Should().NotBeNull();
        mail.To.Count.Should().BeGreaterThan(0);
        mail.To.Mailboxes.Contains(new MailboxAddress("", email)).Should().BeTrue();
        mail.Subject.Should().Be(subject);
    }
    
    [Fact]
    public void Return_False_And_Null_MimeMessage_When_EmailConfiguration_From_Field_Is_Null()
    {
        // Arrange
        string email = _faker.Person.Email;
        string content = _faker.Random.String2(5);
        string subject = _faker.Random.String2(2);

        var logger = _fixture.ServiceProvider.GetService<ILogger<EmailService>>();
        var emailConfiguration = new OptionsWrapper<EmailConfiguration>(new EmailConfiguration());

        var emailService = new EmailService(emailConfiguration, logger);

        // Act
        (bool successful, var mail) = emailService.CreateMessage(email, content, subject);

        // Assert
        successful.Should().BeFalse();
        mail.Should().BeNull();
    }

    [Fact]
    public async Task Return_Failed_Dependency_Error_When_CreateMessage_Returns_False_And_NoMail()
    {
        // Arrange
        string email = _faker.Person.Email;
        string content = _faker.Random.String2(5);
        string subject = _faker.Random.String2(2);

        var logger = _fixture.ServiceProvider.GetService<ILogger<EmailService>>();
        var emailConfiguration = new OptionsWrapper<EmailConfiguration>(new EmailConfiguration());

        var emailService = new EmailService(emailConfiguration, logger);

        // Act
        var response = await emailService.SendEmail(email, content, subject);
        
        // Assert
        response.IsSuccessful().Should().BeFalse();
        response.Code.Should().Be(424);
        response.Data.Should().BeFalse();
    }
    
    [Fact]
    public async Task Return_False_When_EmailConfiguration_Is_Null_During_SendMail()
    {
        // Arrange
        var logger = _fixture.ServiceProvider.GetService<ILogger<EmailService>>();
        var emailConfiguration = new OptionsWrapper<EmailConfiguration>(new EmailConfiguration());

        var emailService = new EmailService(emailConfiguration, logger);

        // Act
        bool response = await emailService.SendMail(new MimeMessage());
        
        // Assert
        response.Should().BeFalse();
    }

    [Fact]
    public async Task Return_True_When_EmailConfiguration_And_Parameter_Is_Not_Null_During_SendMail()
    {
        // Arrange
        string email = _faker.Person.Email;
        string content = _faker.Random.String2(5);
        string subject = _faker.Random.String2(2);

        var emailService = GetEmailService();

        // Act
        (bool successful, var mail) = emailService.CreateMessage(email, content, subject);

        successful.Should().BeTrue();

        bool mailSent = await emailService.SendMail(mail); 
        
        // Assert
        mailSent.Should().BeTrue();
    }
}