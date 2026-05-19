using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FR_WS2_BaseLab.Models;
using System.Security.Claims;
using FR_WS2_BaseLab.Services.Email;
using Microsoft.AspNetCore.Identity;

namespace FR_WS2_BaseLab.Controllers
{
    public class PostsController : Controller
    {
        private readonly FrWs2BaselabContext _context;
        private readonly IApplicationEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public PostsController(FrWs2BaselabContext context, 
        IApplicationEmailSender emailSender,
        UserManager<IdentityUser> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
        }
        // GET: Posts
        public async Task<IActionResult> Index(int? id)
        {
            @ViewData["TopicId"] = id;           
            var frWs2BaselabContext = _context.Posts
                .Include(p=>p.Top)
                .Include(p=>p.User)
                .Where(p=>p.TopId == id);
            return View(await frWs2BaselabContext.ToListAsync());
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Posts/Create
        public IActionResult Create(int? id)
        {
            ViewData["TopicId"] = id;
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TopId,UserId,Inactive,Texte,Date")] Post post)
        {
            if (ModelState.IsValid)
            {
                post.Date = DateTime.Now;
                post.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Add(post);
                await _context.SaveChangesAsync();

                var topic = await _context.Topics
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == post.TopId);

                if (topic?.User != null && 
                    topic.User.EmailConfirmed == true && !string.IsNullOrWhiteSpace(topic.User.Email) && topic.UserId != post.UserId)
                { 
                    await _emailSender.SendAsync(topic.User.Email,
                        $"Nouveau message dans: {topic.Title}",
                        $"<p>Un nouveau message a été ajouté dans votre sujet <strong>{topic.Title}</strong>.</p>"
                    );
                }
                    return RedirectToAction(nameof(Index), new { id = post.TopId });
            }
            return View(post);
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
