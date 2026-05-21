namespace FR_WS2_BaseLab.Services.DTOs;
 
public class CategoryImageDto
{
    public int Id { get; set; }
    public string Url { get; set; } = "";
    public string OriginalFileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long SizeBytes { get; set; }
    public string? AltText { get; set; }
}