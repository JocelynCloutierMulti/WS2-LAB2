namespace FR_WS2_BaseLab.Models
{
    public class CategoryImage
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string? AltText { get; set; }
        public DateTime UploadedAtUtc { get; set; }
        public Category? Category { get; set; }
    }
}
