namespace FR_WS2_BaseLab.Services.Email;

public interface IApplicationEmailSender
{
    Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}