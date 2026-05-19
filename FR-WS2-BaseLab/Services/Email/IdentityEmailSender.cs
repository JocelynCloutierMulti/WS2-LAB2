using Microsoft.AspNetCore.Identity.UI.Services;

namespace FR_WS2_BaseLab.Services.Email;

public class IdentityEmailSender : IEmailSender
{
    private readonly IApplicationEmailSender _emailSender;

    public IdentityEmailSender(IApplicationEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return _emailSender.SendAsync(email, subject, htmlMessage);

    }
}

