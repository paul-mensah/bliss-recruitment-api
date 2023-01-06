using System.ComponentModel.DataAnnotations;
using BlissRecruitment.Core.Models.Requests;
using BlissRecruitment.Core.Models.Responses;
using BlissRecruitment.Core.Services.Email;
using Microsoft.AspNetCore.Mvc;

namespace BlissRecruitment.Api.Controllers;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusOnlyResponse))]
public class ShareController : ControllerBase
{
    private readonly IEmailService _emailService;

    public ShareController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>
    /// Share
    /// </summary>
    /// <param name="destinationEmail"></param>
    /// <param name="contentUrl"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusOnlyResponse))]
    public async Task<IActionResult> Share([FromQuery] [DataType(DataType.EmailAddress)] string destinationEmail, [FromQuery] string contentUrl)
    {
        if (string.IsNullOrEmpty(destinationEmail) || string.IsNullOrEmpty(contentUrl))
        {
            return BadRequest(new StatusOnlyResponse
            {
                Status = "Bad Request. Either destination_email not valid or empty content_url" 
            });
        }
        
        var response = await _emailService.SendEmail(destinationEmail, contentUrl, "Check this out!");

        return response.IsSuccessful()
            ? Ok(new StatusOnlyResponse { Status = "OK" })
            : StatusCode(response.Code, new StatusOnlyResponse { Status = response.Status });
    }
}