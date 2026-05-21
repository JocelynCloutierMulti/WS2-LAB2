using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Implementations;
using FR_WS2_BaseLab.Services.Interfaces;
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
        private readonly IPosts _postService;
        private readonly FrWs2BaselabContext _context;

        public PostsController(IPosts postService, FrWs2BaselabContext context)
        {
            _postService = postService;
            _context = context;
        }

        // GET: Posts
        public async Task<IActionResult> Index(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }
            ViewData["TopicId"] = id;
            var result = await _postService.GetByTopicIdAsync(id.Value);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return View(new List<Post>());
            }
            return View(result.Value);

            //@ViewData["TopicId"] = id;           
            //var frWs2BaselabContext = _context.Posts.Where(p=>p.Id == id);
            //return View(await frWs2BaselabContext.ToListAsync());
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var result = await _postService.GetDetailsAsync(id.Value);
            if (!result.Succeeded || result.Value is null)
            {
                return NotFound();
            }
            return View(result.Value);

        }

        // GET: Posts/Create
        public IActionResult Create(int? id)
        {
            ViewData["PostId"] = id;
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TopId,UserId,Inactive,Texte,Date")] Post post)
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
            return RedirectToAction(nameof(Index), new { id = post.TopId });
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["TopId"] = new SelectList(_context.Topics, "Id", "Id", post.TopId);
            ViewData["UserId"] = new SelectList(_context.AspNetUsers, "Id", "Id", post.UserId);
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TopId,UserId,Inactive,Texte,Date")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
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
            ViewData["TopId"] = new SelectList(_context.Topics, "Id", "Id", post.TopId);
            ViewData["UserId"] = new SelectList(_context.AspNetUsers, "Id", "Id", post.UserId);
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Top)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
