using FR_WS2_BaseLab.Models.ViewModels;
using FR_WS2_BaseLab.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FR_WS2_BaseLab.Controllers;

[Authorize(Roles = "ADMINISTRATOR")]
public class AdminMessagesController : Controller
{
    private readonly IApplicationEmailSender _emailSender;
    private readonly UserManager<IdentityUser> _userManager;

     public AdminMessagesController(UserManager<IdentityUser> userManager,
        IApplicationEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    //get
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        ViewData["Users"] = users;
        return View(new AdminEmailViewModel());
    }

    // Post
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AdminEmailViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Users"] = _userManager.Users.ToList();
            return View(model);
        }

        await _emailSender.SendAsync(model.ToEmail, model.Subject,
            $"<p>{model.Body}</p>");

        TempData["Success"] = $"Courriel envoyé à {model.ToEmail} !";
        return RedirectToAction(nameof(Index));
    }
}