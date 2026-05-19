namespace FR_WS2_BaseLab.Models;

public class EmailSettings
{
    public string Host { get; set; } = "smtp.mailersend.net";

    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = "FR_WS2_BaseLab";
}
