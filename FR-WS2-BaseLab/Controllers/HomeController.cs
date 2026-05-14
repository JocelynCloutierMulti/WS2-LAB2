using System.Diagnostics;
using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FR_WS2_BaseLab.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICategoryService _categoryService;

        public HomeController(ILogger<HomeController> logger, ICategoryService categoryService)
        {
            _logger = logger;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _categoryService.GetAllAsync(includeTopics: true);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Affichage de l'accueil sans catégories: {ErrorMessage}", result.ErrorMessage);
                TempData["ErrorMessage"] = result.ErrorMessage;
                return View(new List<Category>());
            }

            return View(result.Value);
        }
    }
}
