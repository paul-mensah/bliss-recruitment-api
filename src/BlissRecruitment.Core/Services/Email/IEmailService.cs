using BlissRecruitment.Core.Models.Responses;

namespace BlissRecruitment.Core.Services.Email;

public interface IEmailService
{
    Task<BaseResponse<bool>> SendEmail(string email, string content, string subject);
}