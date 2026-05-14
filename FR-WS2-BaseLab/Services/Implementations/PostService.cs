using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using FR_WS2_BaseLab.Data;

namespace FR_WS2_BaseLab.Services.Implementations;

public class PostService(FrWs2BaselabContext context, ILogger<PostService> logger) : IPostService
{
    private readonly FrWs2BaselabContext _context = context;
    private readonly ILogger<PostService> _logger = logger;

    public async Task<ServiceResult<List<Post>>> GetByTopicIdAsync(int sujetId)
    {
        
        try{
            var Posts = await _context.Posts
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Top)
                    .ThenInclude(t => t!.Cat)
                .Where(t => t.TopId == sujetId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
            return ServiceResult<List<Post>>.Success(Posts);
        }catch (Exception ex){
            _logger.LogError(ex,"Erreur lors du chargement des messages du sujet {SujetId}.", sujetId);
            return ServiceResult<List<Post>>.Failure("Les messages n'ont pas pu être chargés.");
        }
    }

    public async Task<ServiceResult<Post>> GetDetailsAsync(int id)
    {
        try{
            var Post = await _context.Posts
                .AsNoTracking()
                .Include(t => t.Top)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (Post is null) return ServiceResult<Post>.Failure("Le message est introuvable.");
            return ServiceResult<Post>.Success(Post);
        }catch (Exception ex){
            _logger.LogError(ex, "Erreur lors du chargement du message {PostId}.", id);
            return ServiceResult<Post>.Failure("Le message n'a pas pu être chargé.");
        }
    }

    public async Task<ServiceResult<Post>> CreateAsync(Post Post, string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) 
            return ServiceResult<Post>.Failure("Vous devez être connecté.");
        try {
            Post.UserId = userId;
            Post.Date = DateTime.Now;
            Post.Inactive = false;
            _context.Posts.Add(Post);
            await _context.SaveChangesAsync();
            return ServiceResult<Post>.Success(Post);
        }catch (DbUpdateException ex) {
            _logger.LogError(ex, "Erreur BD lors de la création d'un message.");
            return ServiceResult<Post>.Failure("Le message n'a pas pu être créé.");
        }
    }

    public async Task<ServiceResult<Post>> GetForEditAsync(int id)
    {
        try{
            var post = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (post is null) return ServiceResult<Post>.Failure("Le message est introuvable.");
                return ServiceResult<Post>.Success(post);
        } catch (Exception ex){
            _logger.LogError(ex, "Erreur lors du chargement du message {PostId}.", id);
            return ServiceResult<Post>.Failure("Le message n'a pas pu être chargé.");
        }
    }
    public async Task<ServiceResult<Post>> UpdateAsync(int id, Post Post, bool adminTrue)
    {
        if (id != Post.Id) return ServiceResult<Post>.Failure("L'identifiant reçu est invalide.");
        var existingPost = await _context.Posts.FindAsync(id);
        if (existingPost is null) return ServiceResult<Post>.Failure("Le message est introuvable.");
        try {
            existingPost.Texte = Post.Texte;
            existingPost.Inactive = Post.Inactive;
            if (adminTrue)
            {
                existingPost.UserId = Post.UserId;
                existingPost.Date = Post.Date;
                existingPost.TopId = Post.TopId; 
            }
            await _context.SaveChangesAsync();
            return ServiceResult<Post>.Success(existingPost);
        }catch (DbUpdateException ex){
            _logger.LogError(ex, "Erreur BD lors de la modification du message {PostId}.", id);
            return ServiceResult<Post>.Failure("Le message n'a pas pu être modifié.");
        }
    }

    public async Task<ServiceResult<Post>> DeleteAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post is null) return ServiceResult<Post>.Failure("Le message est introuvable.");
        try {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return ServiceResult<Post>.Success(post);
        }catch (Exception ex) {
            _logger.LogError(ex,"Erreur BD lors de la suppression du message {PostId}.", id);
            return ServiceResult<Post>.Failure("Le message ne peut pas être supprimé.");
        }
    }

     public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Posts.AnyAsync(c => c.Id == id);
    }
}

