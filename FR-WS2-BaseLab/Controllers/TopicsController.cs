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
    private readonly FrWs2BaselabContext _context;
    public TopicsController(ITopicService topicService, FrWs2BaselabContext context)
    {
        _topicService = topicService;
        _context = context;
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
        ViewData["CategoryId"] = id;
        return View();
    }

    // POST: Topics/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create([Bind("CatId,UserId,Inactive,Title,Texte,Date,Views")] Topic topic)
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
        if (id == null)
        {
            return NotFound();
        }

        var topic = await _context.Topics.FindAsync(id);
        if (topic == null)
        {
            return NotFound();
        }
        ViewData["CatId"] = new SelectList(_context.Categories, "Id", "Id", topic.CatId);
        ViewData["UserId"] = new SelectList(_context.AspNetUsers, "Id", "Id", topic.UserId);
        return View(topic);
    }

    // POST: Topics/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CatId,UserId,Inactive,Title,Texte,Date,Views")] Topic topic)
    {
        if (id != topic.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(topic);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TopicExists(topic.Id))
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
        ViewData["CatId"] = new SelectList(_context.Categories, "Id", "Id", topic.CatId);
        ViewData["UserId"] = new SelectList(_context.AspNetUsers, "Id", "Id", topic.UserId);
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

        var topic = await _context.Topics
            .Include(t => t.Cat)
            .Include(t => t.User)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (topic == null)
        {
            return NotFound();
        }

        return View(topic);
    }

    // POST: Topics/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var topic = await _context.Topics.FindAsync(id);
        if (topic != null)
        {
            _context.Topics.Remove(topic);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TopicExists(int id)
    {
        return _context.Topics.Any(e => e.Id == id);
    }
}
