using System.ComponentModel.DataAnnotations;

namespace FR_WS2_BaseLab.Models.ViewModels;

public class AdminEmailViewModel
{
    [Required]
    [EmailAddress]
    public string ToEmail { get; set; } = "";

    [Required]
    [StringLength(120)]
    public string Subject { get; set; } = "";

    [Required]
    [StringLength(2000)]
    public string Message { get; set; } = "";
}