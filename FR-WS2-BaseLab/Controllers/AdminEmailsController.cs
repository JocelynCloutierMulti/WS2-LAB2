using FR_WS2_BaseLab.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FR_WS2_BaseLab.Controllers;

[Authorize(Roles = "Admin")]
public class AdminEmailsController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AdminEmailsController> _logger;

    public AdminEmailsController(
        UserManager<IdentityUser> userManager,
        IEmailSender emailSender,
        ILogger<AdminEmailsController> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(BuildViewModel(new AdminEmailViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AdminEmailViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(BuildViewModel(model));
        }

        var user = await _userManager.FindByIdAsync(model.RecipientId);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            ModelState.AddModelError(nameof(model.RecipientId), "Cet utilisateur est introuvable ou n'a pas d'adresse courriel.");
            return View(BuildViewModel(model));
        }

        try
        {
            var htmlMessage = $"<p>{System.Net.WebUtility.HtmlEncode(model.Message).Replace("\n", "<br />")}</p>";
            await _emailSender.SendEmailAsync(user.Email, model.Subject, htmlMessage);
            TempData["SuccessMessage"] = "Le courriel a ete envoyé.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Le courriel admin vers l'utilisateur {UserId} n'a pas pu etre envoyé.", model.RecipientId);
            ModelState.AddModelError(string.Empty, "Le courriel n'a pas pu etre envoye. Verifiez la configuration SMTP.");
            return View(BuildViewModel(model));
        }
    }

    private AdminEmailViewModel BuildViewModel(AdminEmailViewModel model)
    {
        model.Users = _userManager.Users
            .Where(u => u.Email != null)
            .OrderBy(u => u.Email)
            .Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.Email!
            })
            .ToList();

        return model;
    }
}
