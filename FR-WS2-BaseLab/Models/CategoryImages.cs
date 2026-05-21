using System;
 
namespace FR_WS2_BaseLab.Models;
 
public  class CategoryImage
{
    public int Id { get; set; }
 
    public int CategoryId { get; set; }
 
    public string FileName { get; set; } = null!;
 
    public string OriginalFileName { get; set; } = null!;
 
    public string ContentType { get; set; } = null!;
 
    public long SizeInBytes { get; set; }
 
    public string? AltText { get; set; }
 
    public DateTime UploadedAtUtc { get; set; }
 
    public virtual Category Category { get; set; } = null!;
}
 