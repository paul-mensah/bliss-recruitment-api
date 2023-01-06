using BlissRecruitment.Core.Models.Responses;
using BlissRecruitment.Core.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace BlissRecruitment.Core.Services.Email;

public class EmailService : IEmailService
{
    private readonly EmailConfiguration _emailConfiguration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailConfiguration> emailConfiguration,
        ILogger<EmailService> logger)
    {
        _emailConfiguration = emailConfiguration.Value;
        _logger = logger;
    }

    public async Task<BaseResponse<bool>> SendEmail(string email, string content, string subject)
    {
        try
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(content) || string.IsNullOrEmpty(subject))
            {
                return CommonResponses.ErrorResponse
                    .BadRequestResponse<bool>("Email, content and subject are required");
            }
            
            (bool successful, var mail) = CreateMessage(email, content, subject);

            if (!successful)
            {
                return CommonResponses.ErrorResponse.FailedDependencyErrorResponse<bool>();
            }

            bool isMailSent = await SendMail(mail);

            return isMailSent
                ? CommonResponses.SuccessResponse.OkResponse(true, "Email sent successfully")
                : CommonResponses.ErrorResponse.FailedDependencyErrorResponse<bool>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured sending email to {email} with content, {content}",
                email, content);
            
            return CommonResponses.ErrorResponse.InternalServerErrorResponse<bool>();
        }
    }

    public (bool successful, MimeMessage mail) CreateMessage(string email, string content, string subject)
    {
        try
        {
            var mail = new MimeMessage();
            mail.Subject = subject;
            mail.Body = new TextPart(TextFormat.Text)
            {
                Text = content
            };
            mail.To.Add(new MailboxAddress("", email));
            mail.From.Add(new MailboxAddress("Bliss Application Test - Paul Mensah", _emailConfiguration.From));

            return (true, mail);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured creating mail");

            return (false, null);
        }
    }

    public async Task<bool> SendMail(MimeMessage message)
    {
        bool isSuccessful;
        
        using (var smtpClient = new SmtpClient())
        {
            try
            {
                await smtpClient.ConnectAsync(_emailConfiguration.Host, _emailConfiguration.Port, true);
                smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
                await smtpClient.AuthenticateAsync(_emailConfiguration.From, _emailConfiguration.Password);

                string response = await smtpClient.SendAsync(message);
                    
                _logger.LogDebug("Response from sending email\n{response}", response);
                isSuccessful = true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured sending email");
                isSuccessful = false;
            }
            finally
            {
                await smtpClient.DisconnectAsync(true);
                smtpClient.Dispose();
            }
        }

        return isSuccessful;
    }
}