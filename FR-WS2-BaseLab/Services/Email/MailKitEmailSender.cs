using FR_WS2_BaseLab.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
namespace FR_WS2_BaseLab.Services.Email;

public class MailKitEmailSender : IapplicationEmailSender
{
	private readonly SmtpOptions _options;
	private readonly ILogger<MailKitEmailSender> _logger;

	public MailKitEmailSender(
		IOptions<SmtpOptions> options,
		ILogger<MailKitEmailSender> logger)
	{
		_options = options.Value;
		_logger = logger;
	}

	public async Task SendEmailAsync(
		string toEMail,
		string subject,
		string htmlBody,
		CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(toEMail))
		{
			throw new ArgumentException("L'adresse e-mail du destinataire est requise.", nameof(toEMail));
		}

		var message = new MimeMessage();
		message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
		message.To.Add(MailboxAddress.Parse(toEMail));
		message.Subject = subject;

		var body = new BodyBuilder
		{
			HtmlBody = htmlBody,
			TextBody = "Ce message contient du contenu HTML."
		};
		message.Body = body.ToMessageBody();

		using var client = new SmtpClient();
		var secureSocketOption = _options.UseStartTls
			? SecureSocketOptions.StartTls
			: SecureSocketOptions.SslOnConnect;

		await client.ConnectAsync(
			_options.Host,
			_options.Port,
			secureSocketOption,
			cancellationToken);

		if (_options.UseAuthentication)
		{
			await client.AuthenticateAsync(
				_options.UserName,
				_options.Password,
				cancellationToken);
		}

		await client.SendAsync(message, cancellationToken);
		await client.DisconnectAsync(true, cancellationToken);

		_logger.LogInformation("Couriel envoyé à {Email}.", toEMail);
	}
}