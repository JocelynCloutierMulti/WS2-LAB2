using FR_WS2_BaseLab.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FR_WS2_BaseLab.Services.Email;

public class MailKitEmailSender : IApplicationEmailSender
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

	public async Task SendAsync(
		string toEmail,
		string subject,
		string htmlBody,
		CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(toEmail))
		{
			throw new ArgumentException("Le destinataire est obligatoire.", nameof(toEmail));
		}

		var message = new MimeMessage();
		message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
		message.To.Add(MailboxAddress.Parse(toEmail));
		message.Subject = subject;

		var body = new BodyBuilder
		{
			HtmlBody = htmlBody,
			TextBody = "Ce message contient du contenu HTML."
		};

		message.Body = body.ToMessageBody();

		using var client = new SmtpClient();

		var secureSocketOptions = _options.UseStartTls
			? SecureSocketOptions.StartTls
			: SecureSocketOptions.None;

		await client.ConnectAsync(
			_options.Host,
			_options.Port,
			secureSocketOptions,
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

		_logger.LogInformation("Courriel envoyé à {Email}.", toEmail);
	}
}
