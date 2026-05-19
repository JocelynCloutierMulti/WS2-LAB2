using FR_WS2_BaseLab.Models.ViewModels;
using FR_WS2_BaseLab.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace FR_WS2_BaseLab.Controllers;

[Authorize(Roles = "Admin")]
public class AdminMessagesController(IApplicationEmailSender emailSender) : Controller
{
    private readonly IApplicationEmailSender _emailSender = emailSender;

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

        var encodedMessage = HtmlEncoder.Default
            .Encode(model.Message)
            .Replace("\n", "<br>");

        var html = $"<p>{encodedMessage}</p>";

        await _emailSender.SendAsync(model.ToEmail, model.Subject, html);

        TempData["Success"] = "Le courriel a été envoyé.";
        return RedirectToAction(nameof(Create));
    }
}