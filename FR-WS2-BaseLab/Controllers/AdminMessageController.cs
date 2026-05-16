using FR_WS2_BaseLab.Models.ViewModels;
using FR_WS2_BaseLab.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace FR_WS2_BaseLab.Controllers;

[Authorize(Roles = "Admin")]
public class AdminMessageController : Controller
{
	private readonly IapplicationEmailSender _emailSender;

	public AdminMessageController(IapplicationEmailSender emailSender)
	{
		_emailSender = emailSender;
	}

	[HttpGet]
	public IActionResult Create()
	{
		return View(new AdminEmailViewModel());
	}
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(AdminEmailViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View(model);
		}

		var encodeMessage = HtmlEncoder.Default
			.Encode(model.Message)
			.Replace("\n", "<br />");

		var html = $"<p>{encodeMessage}</p>";

		await _emailSender.SendAsync(model.ToEmail, model.Subject, html);

		TempData["Success"] = "Le courriel a été envoyé";
		return RedirectToAction(nameof(Create));
	}
}
