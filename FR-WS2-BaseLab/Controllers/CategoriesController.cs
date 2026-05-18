using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FR_WS2_BaseLab.Controllers
{
	[Authorize(Roles = "ADMINISTRATOR")]
	public class CategoriesController : Controller
	{
		private readonly ICategoryService _categoryService;

		public CategoriesController(ICategoryService categoryService)
		{
			_categoryService = categoryService;
		}

		// GET: /Categories
		// Liste publique : tout le monde peut voir les catégories.
		[AllowAnonymous]
		public async Task<IActionResult> Index()
		{
			var result = await _categoryService.GetAllAsync();
			if (!result.Succeeded)
			{
				TempData["ErrorMessage"] = result.ErrorMessage;
				return View(new List<Category>());
			}

			return View(result.Value);
		}

		// GET: /Categories/Details/5
		// Détails publics.
		[AllowAnonymous]
		public async Task<IActionResult> Details(int? id)
		{
			if (id is null)
			{
				return NotFound();
			}

			var result = await _categoryService.GetByIdAsync(id.Value);
			if (!result.Succeeded || result.Value is null)
			{
				return NotFound();
			}

			return View(result.Value);
		}

		// GET: /Categories/Create
		// Création réservée aux administrateurs (Authorize au niveau de la classe).
		public IActionResult Create()
		{
			return View();
		}

		// POST: /Categories/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Name,Description,Image")] Category category)
		{
			if (!ModelState.IsValid)
			{
				return View(category);
			}

			var result = await _categoryService.CreateAsync(category);
			if (!result.Succeeded)
			{
				ModelState.AddModelError(string.Empty, result.ErrorMessage!);
				return View(category);
			}

			TempData["SuccessMessage"] = "La catégorie a été créée.";
			return RedirectToAction(nameof(Index));
		}

		// GET: /Categories/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id is null)
			{
				return NotFound();
			}

			var result = await _categoryService.GetByIdAsync(id.Value);
			if (!result.Succeeded || result.Value is null)
			{
				return NotFound();
			}

			return View(result.Value);
		}

		// POST: /Categories/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id,
			[Bind("Id,Name,Description,Image,Inactive")] Category category)
		{
			if (id != category.Id)
			{
				return NotFound();
			}

			if (!ModelState.IsValid)
			{
				return View(category);
			}

			var result = await _categoryService.UpdateAsync(id, category);
			if (!result.Succeeded)
			{
				ModelState.AddModelError(string.Empty, result.ErrorMessage!);
				return View(category);
			}

			TempData["SuccessMessage"] = "La catégorie a été modifiée.";
			return RedirectToAction(nameof(Index));
		}

		// GET: /Categories/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id is null)
			{
				return NotFound();
			}

			var result = await _categoryService.GetByIdAsync(id.Value);
			if (!result.Succeeded || result.Value is null)
			{
				return NotFound();
			}

			return View(result.Value);
		}

		// POST: /Categories/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var result = await _categoryService.DeleteAsync(id);
			if (!result.Succeeded)
			{
				TempData["ErrorMessage"] = result.ErrorMessage;
				return RedirectToAction(nameof(Details), new { id });
			}

			TempData["SuccessMessage"] = "La catégorie a été supprimée.";
			return RedirectToAction(nameof(Index));
		}
	}
}