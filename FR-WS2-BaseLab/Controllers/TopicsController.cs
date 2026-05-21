using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Data;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace FR_WS2_BaseLab.Controllers;

public class TopicsController(
    ITopicService topicService, 
    FrWs2BaselabContext context, 
    ApplicationDbContext appUser) : Controller
{
    private readonly ITopicService _topicService = topicService;
    private readonly FrWs2BaselabContext _context = context;
    private readonly ApplicationDbContext _appUser = appUser;

    // GET: Topics
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Categories");
    }

    // GET: Topics par catégorie
    [Authorize(Roles = "Admin")]
    [HttpGet("Topics/Index/{id}")]
    public async Task<IActionResult> Index(int? id)
    {
        if (id is null) return NotFound();
        ViewData["CategoryId"] = id;
        var result = await _topicService.GetByCategoryIdAsync(id.Value);
        if (!result.Succeeded){
            TempData["ErrorMessage"] = result.ErrorMessage;
            return View(new List<Topic>());}
        else if (result.Value is not null){
            var messages = result.Value.Select(t => new{
                topicId = t.Id,
                posts = t.Posts.OrderByDescending(p => p.Date).Take(3).Select(p => new {
                    id = p.Id,
                    texte = p.Texte,
                    date = p.Date.ToShortDateString(),
                    userName = p.User != null ? p.User.UserName : "Anonyme"
                }).ToList()}).ToDictionary(t => t.topicId, t => t.posts);
            ViewData["Messages"] = messages;}
        return View(result.Value);
    }

    //Afficher sujets pour tous les utilisateurs
    public async Task<IActionResult> AfficherSujets(int? id)
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
    public async Task<IActionResult> Create(int? id)
    {
        if (id is null) return NotFound();
        if (User.IsInRole("Admin"))
        {
            ViewBag.Categories = new SelectList(
                _context.Categories.Where(c => !c.Inactive),"Id","Name",id.Value);
            ViewBag.Users = new SelectList(_context.AspNetUsers,"Id","UserName");
        }
        return View(new Topic { CatId = id.Value });
    }

    // POST: Topics/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(
    [Bind("CatId,UserId,Title,Texte")] Topic topic)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Challenge();
        if (!User.IsInRole("Admin")) topic.UserId = userId;
        else if (string.IsNullOrWhiteSpace(topic.UserId)){
            topic.UserId = userId;
            ModelState.Remove("UserId");
        }
        if (!ModelState.IsValid)
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.Categories = new SelectList(
                    _context.Categories.Where(c => !c.Inactive),"Id","Name",topic.CatId);
                ViewBag.Users = new SelectList(_appUser.Users.ToList(), "Id","UserName",topic.UserId);
            }
            return View(topic);
        }
        var result = await _topicService.CreateAsync(topic, topic.UserId);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.ErrorMessage!);
            if (User.IsInRole("Admin"))
            {
                ViewBag.Categories = new SelectList(_context.Categories.Where(c => !c.Inactive), "Id", "Name", topic.CatId);
                ViewBag.Users = new SelectList(_appUser.Users.ToList(), "Id", "UserName", topic.UserId);
            }
            return View(topic);
        }
        return RedirectToAction(nameof(Details), new { id = topic.Id });
    }

    // GET: Topics/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Challenge();
        var result = await _topicService.GetForEditAsync(id.Value);
        if (!result.Succeeded || result.Value is null)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Details), new { id });
        } 
        var topic = result.Value;
        if (topic.UserId != userId && !User.IsInRole("Admin")) return Forbid();
        if (User.IsInRole("Admin"))
        {
            ViewData["Name"] = new SelectList(_context.Categories, "Id", "Name", topic.CatId);
            ViewData["UserName"] = new SelectList(_context.AspNetUsers, "Id", "UserName", topic.UserId);
        }
        return View(result.Value);
    }

    // POST: Topics/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CatId,UserId,Inactive,Title,Texte,Date,Views")] Topic topic)
    {
        if (id != topic.Id) return NotFound();
        var autorise = User.IsInRole("Admin");
        if (!ModelState.IsValid)
        { 
            if (autorise)
            {
                 ViewData["Name"] = new SelectList(_context.Categories, "Id", "Name", topic.CatId);
                 ViewData["UserName"] = new SelectList(_context.AspNetUsers, "Id", "UserName", topic.UserId);
            }
            return View(topic); 
        }
        var result = await _topicService.UpdateAsync(id, topic, autorise);
        if (!result.Succeeded || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            if (autorise)
            {
               ViewData["Name"] = new SelectList(_context.Categories, "Id", "Name", result.Value?.CatId);
                ViewData["UserName"] = new SelectList(_context.AspNetUsers, "Id", "UserName", topic.UserId); 
            }
            return View(topic);
        } 
        return RedirectToAction(nameof(Details), new { id = topic.Id });
    }

    // GET: Topics/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Challenge();
        var result = await _topicService.GetDetailsAsync(id.Value);
        if (!result.Succeeded || result.Value is null)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Details), new { id });
        }
        var topic = result.Value;
        if (topic.UserId != userId && !User.IsInRole("Admin")) return Forbid();
        return View(result.Value);
    }

    // POST: Topics/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _topicService.DeleteAsync(id);
            if (!result.Succeeded || result.Value is null)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage!);
                return RedirectToAction(nameof(Details), new { id });
            }
        var categId = result.Value.CatId;
        if (User.IsInRole("Admin"))
        {
           return RedirectToAction(nameof(Index), new { id = categId }); 
        }
        else
        {
            return RedirectToAction(nameof(AfficherSujets), new { id = categId });
        }
    }
}