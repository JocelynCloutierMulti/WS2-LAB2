using FR_WS2_BaseLab.Models.ViewModels;
using FR_WS2_BaseLab.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace FR_WS2_BaseLab.Controllers;

[Authorize(Roles = "ADMINISTRATOR")]
public class AdminMessagesController : Controller
{
	private readonly IApplicationEmailSender _emailSender;
	private readonly ILogger<AdminMessagesController> _logger;

	public AdminMessagesController(
		IApplicationEmailSender emailSender,
		ILogger<AdminMessagesController> logger)
	{
		_emailSender = emailSender;
		_logger = logger;
	}

	// GET : afficher le formulaire vide
	[HttpGet]
	public IActionResult Create()
	{
		return View(new AdminEmailViewModel());
	}

	// POST : envoyer le courriel
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(AdminEmailViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View(model);
		}

		// Encoder le message pour éviter une injection HTML/XSS dans le courriel
		var encodedMessage = HtmlEncoder.Default
			.Encode(model.Message)
			.Replace("\n", "<br>");

		var html = $"<p>{encodedMessage}</p>";

		try
		{
			await _emailSender.SendAsync(model.ToEmail, model.Subject, html);

			TempData["Success"] = $"Le courriel a été envoyé à {model.ToEmail}.";
			return RedirectToAction(nameof(Create));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Échec d'envoi du courriel admin vers {Email}.", model.ToEmail);
			ModelState.AddModelError("", "Une erreur est survenue lors de l'envoi du courriel.");
			return View(model);
		}
	}
}