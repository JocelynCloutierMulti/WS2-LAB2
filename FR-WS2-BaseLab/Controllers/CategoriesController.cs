using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FR_WS2_BaseLab.Controllers;

public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IWebHostEnvironment _env;

    public CategoriesController(ICategoryService categoryService, IWebHostEnvironment env)
    {
        _categoryService = categoryService;
        _env = env;
    }

    // GET: Categories
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Index()
    {
        var result = await _categoryService.GetAllAsync();
        if (!result.Succeeded)        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return View(new List<Category>());
        }
        if (!result.Succeeded || result.Value is null)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction("Index", "Home");
        }
        return View(result.Value);
    }

    // GET: Categories/Details/5
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var result = await _categoryService.GetDetailsAsync(id.Value);
        if (!result.Succeeded || result.Value is null)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    [Authorize(Roles = "ADMINISTRATOR")]
    public IActionResult Create() => View();

    // POST: Categories/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Create([Bind("Inactive,Name,Description")] Category category, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(category);

        category.Image = await SaveImageAsync(imageFile);

        var result = await _categoryService.CreateAsync(category);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(category);
        }
        TempData["SuccessMessage"] = "La catégorie a été créée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Edit/5
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Edit(int? id)
{
    if (id is null)
        return NotFound();

    var result = await _categoryService.GetForEditAsync(id.Value);

    if (!result.Succeeded || result.Value is null)
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
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Inactive,Name,Description,Image")] Category category, IFormFile? imageFile)
    {
        if (id != category.Id) return NotFound();
        if (!ModelState.IsValid) return View(category);

        var newImage = await SaveImageAsync(imageFile);
        if (newImage is not null)
            category.Image = newImage;

        var result = await _categoryService.UpdateAsync(id, category);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(category);
        }
        TempData["SuccessMessage"] = "La catégorie a été modifiée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Delete/5
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var result = await _categoryService.GetDetailsAsync(id.Value);
        if (!result.Succeeded || result.Value is null)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Delete), new { id });
        }
        TempData["SuccessMessage"] = "La catégorie a été supprimée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var folder = Path.Combine(_env.WebRootPath, "images", "categories");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, fileName);

        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/images/categories/{fileName}";
    }
}