using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FR_WS2_BaseLab.Controllers;

public class TopicsController : Controller
{
	private readonly ITopicService _topicService;
	private readonly ICategoryService _categoryService;

	public TopicsController(ITopicService topicService, ICategoryService categoryService)
	{
		_topicService = topicService;
		_categoryService = categoryService;
	}

	// GET: /Topics?id={categoryId}
	public async Task<IActionResult> Index(int? id)
	{
		if (id is null)
		{
			return NotFound();
		}

		ViewData["CategoryId"] = id;

		var result = await _topicService.GetByCategoryIdAsync(id.Value);
		if (!result.Succeeded)
		{
			TempData["ErrorMessage"] = result.ErrorMessage;
			return View(new List<Topic>());
		}

		return View(result.Value);
	}

	// GET: /Topics/Details/5
	public async Task<IActionResult> Details(int? id)
	{
		if (id is null)
		{
			return NotFound();
		}

		var result = await _topicService.GetDetailsAsync(id.Value);
		if (!result.Succeeded || result.Value is null)
		{
			return NotFound();
		}

		return View(result.Value);
	}

	// GET: /Topics/Create?id={categoryId}
	[Authorize]
	public async Task<IActionResult> Create(int? id)
	{
		if (id is null)
		{
			return NotFound();
		}

		ViewData["CategoryId"] = id;
		return View(new Topic { CatId = id.Value });
	}

	// POST: /Topics/Create
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

		TempData["SuccessMessage"] = "Le sujet a été créé.";
		return RedirectToAction(nameof(Index), new { id = topic.CatId });
	}

	// GET: /Topics/Edit/5
	[Authorize]
	public async Task<IActionResult> Edit(int? id)
	{
		if (id is null)
		{
			return NotFound();
		}

		var result = await _topicService.GetForEditAsync(id.Value);
		if (!result.Succeeded || result.Value is null)
		{
			return NotFound();
		}

		return View(result.Value);
	}

	// POST: /Topics/Edit/5
	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize]
	public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Texte,Inactive")] Topic topic)
	{
		if (id != topic.Id)
		{
			return NotFound();
		}

		if (!ModelState.IsValid)
		{
			return View(topic);
		}

		var result = await _topicService.UpdateAsync(id, topic);
		if (!result.Succeeded)
		{
			ModelState.AddModelError(string.Empty, result.ErrorMessage!);
			return View(topic);
		}

		TempData["SuccessMessage"] = "Le sujet a été modifié.";
		return RedirectToAction(nameof(Details), new { id });
	}

	// GET: /Topics/Delete/5
	[Authorize]
	public async Task<IActionResult> Delete(int? id)
	{
		if (id is null)
		{
			return NotFound();
		}

		var result = await _topicService.GetDetailsAsync(id.Value);
		if (!result.Succeeded || result.Value is null)
		{
			return NotFound();
		}

		return View(result.Value);
	}

	// POST: /Topics/Delete/5
	[HttpPost, ActionName("Delete")]
	[ValidateAntiForgeryToken]
	[Authorize]
	public async Task<IActionResult> DeleteConfirmed(int id)
	{
		var topic = await _topicService.GetForEditAsync(id);
		var catId = topic.Value?.CatId;

		var result = await _topicService.DeleteAsync(id);
		if (!result.Succeeded)
		{
			TempData["ErrorMessage"] = result.ErrorMessage;
			return RedirectToAction(nameof(Details), new { id });
		}

		TempData["SuccessMessage"] = "Le sujet a été supprimé.";
		return RedirectToAction(nameof(Index), new { id = catId });
	}
}