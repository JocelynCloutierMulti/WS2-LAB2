using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FR_WS2_BaseLab.Models;

public class AdminEmailViewModel
{
    [Required(ErrorMessage = "Vous devez choisir un utilisateur.")]
    [Display(Name = "Utilisateur")]
    public string RecipientId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le sujet est obligatoire.")]
    [StringLength(150, ErrorMessage = "Le sujet doit contenir au maximum 150 caractères.")]
    [Display(Name = "Sujet")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le message est obligatoire.")]
    [StringLength(4000, ErrorMessage = "Le message doit contenir au maximum 4000 caractères.")]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;

    public List<SelectListItem> Users { get; set; } = new();
}
