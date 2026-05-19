using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using FR_WS2_BaseLab.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using IdentityEmailSender = Microsoft.AspNetCore.Identity.IEmailSender<Microsoft.AspNetCore.Identity.IdentityUser>;
using UiEmailSender = Microsoft.AspNetCore.Identity.UI.Services.IEmailSender;

namespace FR_WS2_BaseLab.Services.Implementations;

public class SmtpEmailSender : UiEmailSender, IdentityEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var fromEmail = GetFromEmail();

        if (string.IsNullOrWhiteSpace(_settings.Host) ||
            string.IsNullOrWhiteSpace(_settings.UserName) ||
            string.IsNullOrWhiteSpace(_settings.Password) ||
            string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new InvalidOperationException(
                "Le SMTP n'est pas configure. Verifiez EmailSettings:Host, UserName, Password et FromEmail.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, _settings.FromName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        message.To.Add(email);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password)
        };

        await client.SendMailAsync(message);
    }

    private string GetFromEmail()
    {
        return string.IsNullOrWhiteSpace(_settings.FromEmail) ? _settings.UserName : _settings.FromEmail;
    }

    public Task SendConfirmationLinkAsync(IdentityUser user, string email, string confirmationLink)
    {
        var encodedLink = HtmlEncoder.Default.Encode(confirmationLink);
        var body = $"""
            <p>Bonjour,</p>
            <p>Veuillez confirmer votre compte en cliquant sur le lien suivant :</p>
            <p><a href="{encodedLink}">Confirmer mon adresse courriel</a></p>
            """;

        return SendEmailAsync(email, "Confirmation de votre adresse courriel", body);
    }

    public Task SendPasswordResetLinkAsync(IdentityUser user, string email, string resetLink)
    {
        var encodedLink = HtmlEncoder.Default.Encode(resetLink);
        var body = $"""
            <p>Bonjour,</p>
            <p>Vous pouvez reinitialiser votre mot de passe en cliquant sur le lien suivant :</p>
            <p><a href="{encodedLink}">Reinitialiser mon mot de passe</a></p>
            """;

        return SendEmailAsync(email, "Reinitialisation de votre mot de passe", body);
    }

    public Task SendPasswordResetCodeAsync(IdentityUser user, string email, string resetCode)
    {
        var encodedCode = HtmlEncoder.Default.Encode(resetCode);
        var body = $"""
            <p>Bonjour,</p>
            <p>Voici votre code de reinitialisation de mot de passe :</p>
            <p><strong>{encodedCode}</strong></p>
            """;

        return SendEmailAsync(email, "Code de reinitialisation de votre mot de passe", body);
    }

}
