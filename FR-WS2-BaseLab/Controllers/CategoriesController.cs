using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FR_WS2_BaseLab.Controllers;

public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: Categories
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var result = await _categoryService.GetDetailsAsync(id.Value);
        if (!result.Succeeded || result.Value is null)
        {
            return NotFound();
        }

        return View(result.Value);
    }

    // GET: Categories/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Categories/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([Bind("Id,Inactive,Name,Description,Image")] Category category)
    {
        if (ModelState.IsValid)
        {
            var result = await _categoryService.CreateAsync(category);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
        }

        return View(category);
    }

    // GET: Categories/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var result = await _categoryService.GetForEditAsync(id.Value);
        if (!result.Succeeded || result.Value is null)
        {
            return NotFound();
        }

        return View(result.Value);
    }

    // POST: Categories/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Inactive,Name,Description,Image")] Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var result = await _categoryService.UpdateAsync(id, category);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
        }
        return View(category);
    }

    // GET: Categories/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var result = await _categoryService.GetDetailsAsync(id.Value);
        if (!result.Succeeded || result.Value is null)
        {
            return NotFound();
        }

        return View(result.Value);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Index));
    }
}
