using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FR_WS2_BaseLab.Models;

public partial class CategoryImage
{

    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Category))]
    public int CategoryId { get; set;}

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string OriginalFileName { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } = null!;

    [Required] 
    public long SizeInBytes { get; set; }

    [StringLength(255)]
    public string? AltText { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
	public DateTime UploadedAtUtc { get; set; }

    public virtual Category Category { get; set; } = null!;
}
