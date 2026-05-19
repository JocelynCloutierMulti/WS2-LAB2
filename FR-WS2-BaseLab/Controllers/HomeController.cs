using System.Diagnostics;
using AspNetCoreGeneratedDocument;
using FR_WS2_BaseLab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FR_WS2_BaseLab.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FrWs2BaselabContext _frWs2Context;

        public HomeController(ILogger<HomeController> logger, FrWs2BaselabContext frWs2Context)
        {
            _logger = logger;
            _frWs2Context = frWs2Context;
        }

        public IActionResult Index()
        {
            var categories = _frWs2Context.Categories
            .AsNoTracking()
            .Where(c => !c.Inactive)
            .Include(t=>t.Topics);
            return View(categories);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
