namespace FR_WS2_BaseLab.Services.Email;

public interface IapplicationEmailSender
{
	Task SendAsync(
		string email, 
		string subject, 
		string htmlBody,
		CancellationToken cancellationToken = default);
}
