using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FR_WS2_BaseLab.Controllers;

public class CategoriesController : Controller
{
	private readonly ICategoryService _categoryService;

	public CategoriesController(ICategoryService categoryService)
	{
		_categoryService = categoryService;
	}

	// GET: Categories
	[Authorize(Roles = "ADMINISTRATOR")]
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

	// GET: Categories/Details/5
	[Authorize(Roles = "ADMINISTRATOR")]
	public async Task<IActionResult> Details(int? id)
	{
		if (id == null)
		{
			return NotFound();
		}

		var result = await _categoryService.GetByIdAsync(id.Value);
		if (!result.Succeeded)
		{
			TempData["ErrorMessage"] = result.ErrorMessage;
			return View();                          // FIX 1: ne pas passer une List<Category> à une vue Details
		}

		return View(result.Value);
	}

	// GET: Categories/Create
	[Authorize(Roles = "ADMINISTRATOR")]
	public IActionResult Create()
	{
		return View();
	}

	// POST: Categories/Create
	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Roles = "ADMINISTRATOR")]
	public async Task<IActionResult> Create(
		[Bind("Id,Inactive,Name,Description,Image")] Category category)
	{
		if (ModelState.IsValid)
		{
			var result = await _categoryService.CreateAsync(category);
			if (!result.Succeeded)
			{
				TempData["ErrorMessage"] = result.ErrorMessage;
				return View(category);
			}
			return RedirectToAction(nameof(Index), "Home");
		}
		return View(category);
	}

	// GET: Categories/Edit/5
	[Authorize(Roles = "ADMINISTRATOR")]
	public async Task<IActionResult> Edit(int? id)
	{
		if (id == null)
		{
			return NotFound();
		}

		// FIX 2: parenthèse fermante manquante + corps de méthode incomplet
		var result = await _categoryService.GetByIdAsync(id.Value);
		if (!result.Succeeded)
		{
			TempData["ErrorMessage"] = result.ErrorMessage;
			return View();
		}

		return View(result.Value);
	}

	// POST: Categories/Edit/5
	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Roles = "ADMINISTRATOR")]
	public async Task<IActionResult> Edit(
		int id,
		[Bind("Id,Inactive,Name,Description,Image")] Category category)
	{
		if (id != category.Id)
		{
			return NotFound();
		}

		if (ModelState.IsValid)
		{
			try
			{
				// FIX 3: remplacer _context.Update / SaveChangesAsync par le service
				var result = await _categoryService.UpdateAsync(id, category);
				if (!result.Succeeded)
				{
					TempData["ErrorMessage"] = result.ErrorMessage;
					return View(category);
				}
			}
			catch (DbUpdateConcurrencyException)
			{
				// FIX 4: remplacer CategoryExists(_context) par le service
				if (!await _categoryService.ExistsAsync(category.Id))
				{
					return NotFound();
				}
				throw;
			}

			return RedirectToAction(nameof(Index));
		}

		return View(category);
	}

	// GET: Categories/Delete/5
	[Authorize(Roles = "ADMINISTRATOR")]
	public async Task<IActionResult> Delete(int? id)
	{
		if (id == null)
		{
			return NotFound();
		}

		// FIX 5: remplacer _context.Categories.FirstOrDefaultAsync par le service
		var result = await _categoryService.GetByIdAsync(id.Value);
		if (!result.Succeeded)
		{
			TempData["ErrorMessage"] = result.ErrorMessage;
			return View();
		}

		return View(result.Value);
	}

	// POST: Categories/Delete/5
	[HttpPost, ActionName("Delete")]
	[ValidateAntiForgeryToken]
	[Authorize(Roles = "ADMINISTRATOR")]
	public async Task<IActionResult> DeleteConfirmed(int id)
	{
		// FIX 6: remplacer _context.Categories.Remove / SaveChangesAsync par le service
		var result = await _categoryService.DeleteAsync(id);
		if (!result.Succeeded)
		{
			TempData["ErrorMessage"] = result.ErrorMessage;
			return View();
		}

		return RedirectToAction(nameof(Index));
	}
}