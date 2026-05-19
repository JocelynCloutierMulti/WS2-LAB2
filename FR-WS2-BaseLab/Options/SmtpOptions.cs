namespace FR_WS2_BaseLab.Options;

public class SmtpOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 25;

    public bool UseStartTls { get; set; }
    public bool UseAuthentication { get; set; }

    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";

    public string FromEmail { get; set; } = "";
    public string FromName { get; set; } = "";

}

