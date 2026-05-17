using System.ComponentModel.DataAnnotations;

namespace FR_WS2_BaseLab.Models.ViewModels;

public class AdminEmailViewModel

{

	[Required(ErrorMessage = "Le courriel du destinataire est obligatoire.")]

	[EmailAddress(ErrorMessage = "Le format du courriel est invalide.")]

	[Display(Name = "Destinataire")]

	public string ToEmail { get; set; } = "";

	[Required(ErrorMessage = "Le sujet est obligatoire.")]

	[StringLength(120, ErrorMessage = "Le sujet ne peut pas dépasser 120 caractères.")]

	[Display(Name = "Sujet")]

	public string Subject { get; set; } = "";

	[Required(ErrorMessage = "Le message est obligatoire.")]

	[StringLength(2000, ErrorMessage = "Le message ne peut pas dépasser 2000 caractères.")]

	[DataType(DataType.MultilineText)]

	[Display(Name = "Message")]

	public string Message { get; set; } = "";

}
