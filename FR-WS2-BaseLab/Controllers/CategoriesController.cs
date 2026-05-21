using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Implementations;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FR_WS2_BaseLab.Controllers;

public class CategoriesController : Controller
{
    private readonly FrWs2BaselabContext _context;
    private readonly ICategoriesService _categoryService;

    public CategoriesController(ICategoriesService categoryService,FrWs2BaselabContext context)
    {
        _context = context;
        _categoryService = categoryService;
    }

    // GET: Categories
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Index(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }
        ViewData["CategoryId"] = id;
        var result = await _categoryService.GetByIdAsync(id.Value);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return View(new List<Category>());
        }
        return View(result.Value);

        //return View(await _context.Categories.ToListAsync());
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
        if (!result.Succeeded || result.Value is null)
        {
            return NotFound();
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
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Create([Bind("Id,Inactive,Name,Description,Image")] Category category)
    {
        //if (ModelState.IsValid)
        //{
        //    _context.Add(category);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index), "Home");
        //}
        //return View(category);
        if (!ModelState.IsValid)
        {
            ViewData["CategoryId"] = category.Id;
            return View(category);
        }
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _categoryService.CreateAsync(category);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            ViewData["CategoryId"] = category.Id;
            return View(category);
        }
        return RedirectToAction(nameof(Index), new { id = category.Id });
    }

    // GET: Categories/Edit/5
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    // POST: Categories/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Inactive,Name,Description,Image")] Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
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

        var category = await _context.Categories
            .FirstOrDefaultAsync(m => m.Id == id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMINISTRATOR")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.Id == id);
    }
}
