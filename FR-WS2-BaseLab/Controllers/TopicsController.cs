using System.Security.Claims;
using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

 

namespace FR_WS2_BaseLab.Controllers;

 

public class TopicsController : Controller
{
    private readonly ITopicService _topicService;

 

    public TopicsController(ITopicService topicService)
    {
        _topicService = topicService;
    }

 

    // GET: Topics/Index/5  (5 = category id)
    public async Task<IActionResult> Index(int? id)
    {
        ViewData["CategoryId"] = id;

 

        if (id is null)
        {
            return View(new List<Topic>());
        }

 

        var result = await _topicService.GetByCategoryIdAsync(id.Value);
        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(new List<Topic>());
        }

 

        return View(result.Value);
    }

 

    // GET: Topics/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

 

        var result = await _topicService.GetDetailsAsync(id.Value);
        if (!result.Success)
        {
            return NotFound();
        }

 

        return View(result.Value);
    }

 

    // GET: Topics/Create/5  (5 = category id)
    [Authorize]
    public IActionResult Create(int? id)
    {
        ViewData["CategoryId"] = id;
        return View();
    }

 

    // POST: Topics/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(
        [Bind("CatId,Inactive,Title,Texte")] Topic topic)
    {
        // Fields not bound from the form — set in the service.
        ModelState.Remove(nameof(Topic.UserId));
        ModelState.Remove(nameof(Topic.Date));
        ModelState.Remove(nameof(Topic.User));
        ModelState.Remove(nameof(Topic.Cat));

 

        if (!ModelState.IsValid)
        {
            ViewData["CategoryId"] = topic.CatId;
            return View(topic);
        }

 

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _topicService.CreateAsync(topic, userId);

 

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Erreur lors de la création.");
            ViewData["CategoryId"] = topic.CatId;
            return View(topic);
        }

 

        return RedirectToAction(nameof(Index), new { id = topic.CatId });
    }

 

    // GET: Topics/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

 

        var result = await _topicService.GetForEditAsync(id.Value);
        if (!result.Success)
        {
            return NotFound();
        }

 

        ViewData["CategoryId"] = result.Value!.CatId;
        return View(result.Value);
    }

 

    // POST: Topics/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,CatId,Inactive,Title,Texte")] Topic topic)
    {
        if (id != topic.Id)
        {
            return NotFound();
        }

 

        ModelState.Remove(nameof(Topic.UserId));
        ModelState.Remove(nameof(Topic.Date));
        ModelState.Remove(nameof(Topic.User));
        ModelState.Remove(nameof(Topic.Cat));

 

        if (!ModelState.IsValid)
        {
            ViewData["CategoryId"] = topic.CatId;
            return View(topic);
        }

 

        var result = await _topicService.UpdateAsync(id, topic);

 

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Erreur lors de la modification.");
            ViewData["CategoryId"] = topic.CatId;
            return View(topic);
        }

 

        return RedirectToAction(nameof(Index), new { id = topic.CatId });
    }

 

    // GET: Topics/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

 

        var result = await _topicService.GetDetailsAsync(id.Value);
        if (!result.Success)
        {
            return NotFound();
        }

 

        return View(result.Value);
    }

 

    // POST: Topics/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _topicService.DeleteAsync(id);

 

        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction(nameof(Delete), new { id });
        }

 

        // result.Value is the deleted topic — use its CatId for redirect.
        return RedirectToAction(nameof(Index), new { id = result.Value!.CatId });
    }
}