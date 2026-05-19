using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using UiEmailSender = Microsoft.AspNetCore.Identity.UI.Services.IEmailSender;

namespace FR_WS2_BaseLab.Areas.Identity.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly UiEmailSender _emailSender;
    private readonly ILogger<ForgotPasswordModel> _logger;

    public ForgotPasswordModel(
        UserManager<IdentityUser> userManager,
        UiEmailSender emailSender,
        ILogger<ForgotPasswordModel> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "L'adresse courriel est obligatoire.")]
        [EmailAddress(ErrorMessage = "L'adresse courriel est invalide.")]
        [Display(Name = "Adresse courriel")]
        public string Email { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user is null)
        {
            _logger.LogInformation("Demande de reinitialisation ignoree: aucun utilisateur pour {Email}.", Input.Email);
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        var email = await _userManager.GetEmailAsync(user);
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Demande de reinitialisation ignoree: l'utilisateur {UserId} n'a pas d'adresse courriel.", user.Id);
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { area = "Identity", code, email },
            protocol: Request.Scheme);

        if (string.IsNullOrWhiteSpace(callbackUrl))
        {
            _logger.LogError("Le lien de reinitialisation du mot de passe n'a pas pu etre genere pour {Email}.", email);
            ModelState.AddModelError(string.Empty, "Le lien de reinitialisation n'a pas pu etre genere.");
            return Page();
        }

        var encodedCallbackUrl = HtmlEncoder.Default.Encode(callbackUrl);
        var message = $"""
            <p>Bonjour,</p>
            <p>Vous pouvez reinitialiser votre mot de passe en cliquant sur le lien suivant :</p>
            <p><a href="{encodedCallbackUrl}">Reinitialiser mon mot de passe</a></p>
            """;

        try
        {
            _logger.LogInformation("Envoi du courriel de reinitialisation a {Email}.", email);
            await _emailSender.SendEmailAsync(email, "Reinitialisation de votre mot de passe", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Le courriel de reinitialisation n'a pas pu etre envoye a {Email}.", email);
            ModelState.AddModelError(string.Empty, "Le courriel n'a pas pu etre envoye. Verifiez la configuration SMTP.");
            return Page();
        }

        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}
