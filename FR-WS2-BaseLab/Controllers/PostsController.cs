using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FR_WS2_BaseLab.Controllers
{
    public class PostsController(
        IPostService postService, 
        ITopicService topicService, 
        FrWs2BaselabContext context) : Controller
    {
        private readonly FrWs2BaselabContext _context = context;
        private readonly IPostService _postService = postService;
        private readonly ITopicService _topicService = topicService;

        // GET: Topics
        [Authorize(Roles = "ADMINISTRATOR")]
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Topics");
        }

        // GET: Posts par sujets
        [Authorize(Roles = "ADMINISTRATOR")]
        [HttpGet("Posts/Index/{id}")]
        // GET: Posts
        public async Task<IActionResult> Index(int? id)
        {
            if (id is null) return NotFound();
            await _topicService.IncrementViewsAsync(id.Value);
            ViewData["TopicId"] = id;
            var result = await _postService.GetByTopicIdAsync(id.Value);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return View(new List<Post>());
            }
            var topic = await _context.Topics.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id.Value);
            ViewData["CategoryId"] = topic?.CatId;
            return View(result.Value);
        }

        public async Task<IActionResult> AfficherMessages(int? id)
        {
            if (id is null) return NotFound();
            ViewData["TopicId"] = id;
            var result = await _postService.GetByTopicIdAsync(id.Value);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return View(new List<Post>());
            }
            return View(result.Value);
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id, int? topicId)
        {
            if (id is null) return NotFound();
            ViewData["TopicId"] = topicId;
            var result = await _postService.GetDetailsAsync(id.Value);
            if (!result.Succeeded || result.Value is null)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                if (topicId.HasValue) return RedirectToAction(nameof(Index), new { id = topicId.Value });
                return NotFound();
            }
            return View(result.Value);
        }

        // GET: Posts/Create
        [Authorize]
        public IActionResult Create(int? id)
        {
            if (id is null) return NotFound();
            ViewData["TopicId"] = id;
            return View(new Post { TopId = id.Value });
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("TopId,UserId,Inactive,Texte,Date")] Post post)
        {
            if (!ModelState.IsValid) return View(post);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Challenge();
            var result = await _postService.CreateAsync(post, userId);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage!);
                return View(post);
            }
            return RedirectToAction(nameof(Details), 
                new { id = result.Value!.Id, topicId = result.Value.TopId });
        }

        // GET: Posts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id, int? topicId)
        {
            if (id is null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Challenge();
            ViewData["TopicId"] = topicId;
            var result = await _postService.GetForEditAsync(id.Value);
            if (!result.Succeeded || result.Value is null)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Details), new { id });
            }
            var post = result.Value;
            if (post.UserId != userId && !User.IsInRole("ADMINISTRATOR")) return Forbid();
            if (User.IsInRole("ADMINISTRATOR"))
            {
                ViewData["TopId"] = new SelectList(_context.Topics, "Id", "Title", result.Value?.TopId);
                ViewData["UserName"] = new SelectList(_context.AspNetUsers, "Id", "UserName", post.UserId);
            }
            return View(result.Value);  
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TopId,UserId,Inactive,Texte,Date")] Post post)
        {
            ViewData["TopicId"] = post.TopId;
            if (!ModelState.IsValid) return View(post);
            var autorise = false;
            if (User.IsInRole("ADMINISTRATOR")) autorise = true;
            var result = await _postService.UpdateAsync(id, post, autorise);
            if (!result.Succeeded || result.Value is null)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage!);
                return View(post);
            }
            return RedirectToAction(nameof(Details), new { id, topicId = post.TopId });
        }

        // GET: Posts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id, int? topicId)
        {
            if (id is null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Challenge();
            var result = await _postService.GetDetailsAsync(id.Value);
            if (!result.Succeeded || result.Value is null)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Details), new { id });
            }
            var post = result.Value;
            if (post.UserId != userId && !User.IsInRole("ADMINISTRATOR")) return Forbid();
            ViewData["TopicId"] = topicId;
            return View(result.Value); 
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _postService.DeleteAsync(id);
            if (!result.Succeeded || result.Value is null)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage!);
                return RedirectToAction(nameof(Details), new { id });
            }
            var sujetId = result.Value.TopId;
            if (User.IsInRole("ADMINISTRATOR"))
                return RedirectToAction(nameof(Index), new { id = sujetId });
            return RedirectToAction(nameof(AfficherMessages), new { id = sujetId});
        }
    }
}