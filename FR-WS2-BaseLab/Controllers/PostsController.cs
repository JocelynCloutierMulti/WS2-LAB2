using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FR_WS2_BaseLab.Controllers
{
    public class PostsController : Controller
    {
        private readonly IPostService _postService;
        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        // GET: Posts
        public async Task<IActionResult> Index(int? id)
        {
            if (id is null) return NotFound();
            @ViewData["TopicId"] = id;    
            
            var result = await _postService.GetByTopicIdAsync(id.Value);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return View(new List<Post>());
            }

            return View(result.Value);
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _postService.GetDetailsAsync(id.Value);

            if (!result.Succeeded || result.Value is null) return NotFound();

            return View(result.Value);
        }

        // GET: Posts/Create
        [Authorize]
        public IActionResult Create(int? id)
        {
            if (id == null) return NotFound();
            ViewData["TopicId"] = id;
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("TopId,Texte")] Post post)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _postService.CreateAsync(post, userId);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index), new { id = post.TopId });
                }

                ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            }
            ViewData["TopicId"] = post.TopId;
            return View(post);
        }

        // GET: Posts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _postService.GetDetailsAsync(id.Value);
            if (!result.Succeeded) return NotFound();

            return View(result.Value);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TopId,Texte")] Post post)
        {
            if (id != post.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _postService.UpdateAsync(id, post);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index), new { id = post.TopId });
                }

                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Erreur de modification.");
            }

            return View(post);
        }

        // GET: Posts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _postService.GetDetailsAsync(id.Value);
            if (!result.Succeeded) return NotFound();

            return View(result.Value);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var postDetails = await _postService.GetDetailsAsync(id);
            int? topicId = postDetails.Value?.TopId;

            var result = await _postService.DeleteAsync(id);

            if (result.Succeeded && topicId.HasValue)
            {
                return RedirectToAction(nameof(Index), new { id = topicId.Value });
            }
            return RedirectToAction("Index");
        }
        

        private async Task<bool> PostExists(int id)
        {
            return await _postService.ExistsAsync(id);
        }
    }
}
