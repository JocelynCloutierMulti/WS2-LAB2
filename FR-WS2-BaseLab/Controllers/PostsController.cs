using System.Security.Claims;
using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FR_WS2_BaseLab.Controllers
{
    public class PostsController : Controller
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        // GET: Posts/Index/5  (5 = topic id)
        public async Task<IActionResult> Index(int? id)
        {
            ViewData["TopicId"] = id;

            if (id is null)
            {
                return View(new List<Post>());
            }

            var result = await _postService.GetByTopicIdAsync(id.Value);
            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
                return View(new List<Post>());
            }

            return View(result.Value);
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var result = await _postService.GetDetailsAsync(id.Value);
            if (!result.Success)
            {
                return NotFound();
            }

            return View(result.Value);
        }

        // GET: Posts/Create/5  (5 = topic id)
        [Authorize]
        public IActionResult Create(int? id)
        {
            ViewData["TopicId"] = id;
            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(
            [Bind("TopId,Inactive,Texte")] Post post)
        {
            ModelState.Remove(nameof(Post.UserId));
            ModelState.Remove(nameof(Post.Date));
            ModelState.Remove(nameof(Post.User));
            ModelState.Remove(nameof(Post.Top));

            if (!ModelState.IsValid)
            {
                ViewData["TopicId"] = post.TopId;
                return View(post);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _postService.CreateAsync(post, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty,
                    result.ErrorMessage ?? "Erreur lors de la création.");
                ViewData["TopicId"] = post.TopId;
                return View(post);
            }

            return RedirectToAction(nameof(Index), new { id = post.TopId });
        }

        // GET: Posts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var result = await _postService.GetForEditAsync(id.Value);
            if (!result.Success)
            {
                return NotFound();
            }

            ViewData["TopicId"] = result.Value!.TopId;
            return View(result.Value);
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,TopId,Inactive,Texte")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Post.UserId));
            ModelState.Remove(nameof(Post.Date));
            ModelState.Remove(nameof(Post.User));
            ModelState.Remove(nameof(Post.Top));

            if (!ModelState.IsValid)
            {
                ViewData["TopicId"] = post.TopId;
                return View(post);
            }

            var result = await _postService.UpdateAsync(id, post);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty,
                    result.ErrorMessage ?? "Erreur lors de la modification.");
                ViewData["TopicId"] = post.TopId;
                return View(post);
            }

            return RedirectToAction(nameof(Index), new { id = post.TopId });
        }

        // GET: Posts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var result = await _postService.GetDetailsAsync(id.Value);
            if (!result.Success)
            {
                return NotFound();
            }

            return View(result.Value);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _postService.DeleteAsync(id);

            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index), new { id = result.Value!.TopId });
        }
    }
}