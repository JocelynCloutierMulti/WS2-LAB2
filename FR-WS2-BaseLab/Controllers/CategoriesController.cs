using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FR_WS2_BaseLab.Controllers;

[Authorize(Roles = "Admin")]

public class CategoriesController(ICategoryService categoryService) : Controller
{
    private readonly ICategoryService _categoryService = categoryService;

    // GET: Categories
    public async Task<IActionResult> Index()
    {
        var result = await _categoryService.GetAllAsync();
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index), "Home");
        }
        return View(result.Value);
    }

    // GET: Categories/Details/5
    public async Task<IActionResult> Details(int ? id)
    {
       if (id is null) return NotFound();
        var result = await _categoryService.GetByIdAsync(id.Value);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index), "Home");
        }
        return View(result.Value);
    }

    // GET: Categories/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Categories/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Inactive,Name,Description,Image")] Category category, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(category);
        var result = await _categoryService.CreateAsync(category, imageFile);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(category);
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var result = await _categoryService.GetByIdAsync(id.Value);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value); 
    }

    // POST: Categories/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, 
        [Bind("Id,Inactive,Name,Description")] Category category, IFormFile? imageFile)
    {
        if (id != category.Id) return NotFound();
        if (!ModelState.IsValid) return View(category);
        var result = await _categoryService.UpdateAsync(id, category, imageFile);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(category);
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var result = await _categoryService.GetByIdAsync(id.Value);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value); 
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
         var result = await _categoryService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Delete), new { id });
        }
       return RedirectToAction(nameof(Index));
    }
}