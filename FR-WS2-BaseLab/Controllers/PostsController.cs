using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FR_WS2_BaseLab.Controllers;

[Authorize] 
public class PostsController : Controller
{
	private readonly IPostService _postService;

	public PostsController(IPostService postService)
	{
		_postService = postService;
	}

	// GET: /Posts?id={topicId}
	public async Task<IActionResult> Index(int? id)
	{
		if (id is null) return NotFound();

		ViewData["TopicId"] = id;
		var result = await _postService.GetByTopicIdAsync(id.Value);
		if(!result.Succeeded)
		{
			TempData["ErrorMessage"] = result.ErrorMessage;
			return View(new List<Post>());
		}
		return View(result.Value);
	}

	// GET: /Posts/Details/5
	public async Task<IActionResult> Details(int? id)
	{
		if (id is null) return NotFound();

		var result = await _postService.GetDetailsAsync(id.Value);
		if (!result.Succeeded || result.Value is null) return NotFound();

		return View(result.Value);
	}

	// GET: /Posts/Create?id={topicId}
	public IActionResult Create(int? id)
	{
		if (id is null) return NotFound();

		ViewData["TopicId"] = id;
		return View(new Post { TopId = id.Value });
	}

	// POST: /Posts/Create
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([Bind("TopId,Texte")] Post post)
	{
		if (!ModelState.IsValid)
		{
			ViewData["TopicId"] = post.TopId;
			return View(post);
		}

		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		var result = await _postService.CreateAsync(post, userId);
		if (!result.Succeeded)
		{
			ModelState.AddModelError(string.Empty, result.ErrorMessage!);
			ViewData["TopicId"] = post.TopId;
			return View(post);
		}

		TempData["SuccessMessage"] = "Le message a été créé.";
		return RedirectToAction(nameof(Index), new { id = post.TopId });
	}

	// GET: /Posts/Edit/5
	public async Task<IActionResult> Edit(int? id)
	{
		if (id is null) return NotFound();

		var result = await _postService.GetForEditAsync(id.Value);
		if (!result.Succeeded || result.Value is null) return NotFound();

		return View(result.Value);
	}

	// POST: /Posts/Edit/5
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(int id, [Bind("Id,Texte,Inactive")] Post post)
	{
		if (id != post.Id) return NotFound();

		if (!ModelState.IsValid) return View(post);

		var result = await _postService.UpdateAsync(id, post);
		if (!result.Succeeded)
		{
			ModelState.AddModelError(string.Empty, result.ErrorMessage!);
			return View(post);
		}

		TempData["SuccessMessage"] = "Le message a été modifié.";
		return RedirectToAction(nameof(Details), new { id });
	}

	// GET: /Posts/Delete/5
	public async Task<IActionResult> Delete(int? id)
	{
		if (id is null) return NotFound();

		var result = await _postService.GetDetailsAsync(id.Value);
		if (!result.Succeeded || result.Value is null) return NotFound();

		return View(result.Value);
	}

	// POST: /Posts/Delete/5
	[HttpPost, ActionName("Delete")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> DeleteConfirmed(int id)
	{
		var post = await _postService.GetForEditAsync(id);
		var topId = post.Value?.TopId;
		var result = await _postService.DeleteAsync(id);
		if (!result.Succeeded)
		{
			TempData["ErrorMessage"] = result.ErrorMessage;
			return RedirectToAction(nameof(Details), new { id });
		}

		TempData["SuccessMessage"] = "Le message a été supprimé.";
		return RedirectToAction(nameof(Index), new { id = topId });
	}
}