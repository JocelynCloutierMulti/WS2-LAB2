using FR_WS2_BaseLab.Models;
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

public class TopicsController : Controller
{

    private readonly ITopicService _topicService;
    public TopicsController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    // GET: Topics
    public async Task<IActionResult> Index(int? id)
    {
        if (id is null) return NotFound();
        ViewData["CategoryId"] = id;

        var result = await _topicService.GetByCategoryIdAsync(id.Value);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return View(new List<Topic>());
        }

        return View(result.Value);
    }

    // GET: Topics/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var result = await _topicService.GetDetailsAsync(id.Value);

        if (!result.Succeeded || result.Value is null) return NotFound();
        
        return View(result.Value);
    }

    // GET: Topics/Create
    [Authorize]
    public IActionResult Create(int? id)
    {
        if (id == null) return NotFound();
        ViewData["CategoryId"] = id;
        return View();
    }

    // POST: Topics/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create([Bind("CatId,Title,Texte")] Topic topic)
    {
        if (!ModelState.IsValid)
        {
            ViewData["CategoryId"] = topic.CatId;
            return View(topic);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _topicService.CreateAsync(topic, userId);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            ViewData["CategoryId"] = topic.CatId;
            return View(topic);
        }

        return RedirectToAction(nameof(Index), new { id = topic.CatId });

    }

    // GET: Topics/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var result = await _topicService.GetForEditAsync(id.Value);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index)); 
        }

        return View(result.Value);
    }

    // POST: Topics/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Texte,Inactive,CatId")] Topic topic)
    {
        if (id != topic.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var result = await _topicService.UpdateAsync(id, topic);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index), new { id = topic.CatId });
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
        }

        return View(topic);
    }

    // GET: Topics/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var result = await _topicService.GetDetailsAsync(id.Value);

        if (!result.Succeeded) return NotFound();

        return View(result.Value);
    }

    // POST: Topics/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var topicResult = await _topicService.GetDetailsAsync(id);
        int? categoryId = topicResult.Value?.CatId;

        var result = await _topicService.DeleteAsync(id);

        if (result.Succeeded && categoryId.HasValue)
        {
            return RedirectToAction(nameof(Index), new { id = categoryId.Value });
        }

        return RedirectToAction("Index");
    }

    private async Task<bool> TopicExists(int id)
    {
        return await _topicService.ExistsAsync(id);
    }
}
