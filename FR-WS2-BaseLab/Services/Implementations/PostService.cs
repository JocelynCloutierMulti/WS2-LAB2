using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Email;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FR_WS2_BaseLab.Services.Implementations;
public class PostService : IPostService
{
    private readonly FrWs2BaselabContext _context;
    private readonly ILogger<PostService> _logger;
    private readonly IApplicationEmailSender _emailSender;

    public PostService(
        FrWs2BaselabContext context,
        ILogger<PostService> logger,
        IApplicationEmailSender emailSender)
    {
        _context = context;
        _logger = logger;
        _emailSender = emailSender;
    }

    public async Task<ServiceResult<Post>> CreateAsync(Post post, string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return ServiceResult<Post>.Failure("Vous devez être connecté.");
        }

        try
        {
            post.Date = DateTime.Now;
            post.UserId = userId;
            _context.Add(post);
            await _context.SaveChangesAsync();

            var topic = await _context.Topics
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == post.TopId);

            if (topic?.User?.EmailConfirmed == true &&
                !string.IsNullOrWhiteSpace(topic.User.Email) &&
                topic.UserId != post.UserId)
            {
                var subject = $"Nouveau message dans: {topic.Title}";
                var html = $""" 
                            <p>Un nouveau message a été ajouté dans votre sujet.</p> 
                            <p><strong>{topic.Title}</strong></p> 
                            <p>Connectez-vous à BaseLab pour consulter la réponse.</p> 
                            """;

                try
                {
                    await _emailSender.SendAsync(topic.User.Email, subject, html);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'envoi de la notification.");
                }
            }

            return ServiceResult<Post>.Success(post);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex,
                "Erreur BD lors de la création d'un message.");
            return ServiceResult<Post>.Failure(
                "Le message n'a pas pu être créé.");
        }
    }

    public async Task<ServiceResult<Post>> DeleteAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post is null)
        {
            return ServiceResult<Post>.Failure("Le message est introuvable.");
        }

        try
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return ServiceResult<Post>.Success(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erreur BD lors de la suppression du message {PostId}.", id);

            return ServiceResult<Post>.Failure(
                "Le message n'a pas pu être supprimé.");
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Posts.AnyAsync(post => post.Id == id);
    }

    public async Task<ServiceResult<List<Post>>> GetByTopicIdAsync(int topicId)
    {
        try
        {
            var posts = await _context.Posts
                .AsNoTracking()
                .Include(p => p.User)
                .Where(p => p.TopId == topicId)
                .OrderBy(p => p.Date)
                .ToListAsync();

            var sujet = await _context.Topics.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == topicId);
            sujet.Views++;
            await _context.SaveChangesAsync();
            return ServiceResult<List<Post>>.Success(posts);

        }

        catch (Exception ex)

        {
            _logger.LogError(ex,
                "Erreur lors du chargement des messages du sujet {TopicId}.",
                topicId);

            return ServiceResult<List<Post>>.Failure(
                "Les messages n'ont pas pu être chargés.");
        }
    }

    public async Task<ServiceResult<Post>> GetDetailsAsync(int id)
    {
        try
        {
            var post = await _context.Posts
                 .Include(p => p.Top)
                 .Include(p => p.User)
                 .FirstOrDefaultAsync(m => m.Id == id);

            if (post is null)
            {
                return ServiceResult<Post>.Failure("Le message est introuvable.");
            }
            return ServiceResult<Post>.Success(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erreur lors du chargement du message {PostId}.", id);

            return ServiceResult<Post>.Failure(
                "Le message n'a pas pu être chargé.");
        }
    }

    public async Task<ServiceResult<Post>> GetForEditAsync(int id)
    {
        try
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(t => t.Id == id);

            if (post is null)
            {
                return ServiceResult<Post>.Failure("Le message à modifier est introuvable.");
            }

            return ServiceResult<Post>.Success(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du message {TopicId}.", id);
            return ServiceResult<Post>.Failure("Une erreur est survenue lors du chargement du message.");
        }
    }

    public async Task<ServiceResult<Post>> UpdateAsync(int id, Post post)
    {
        if(id != post.Id)
        {
            return ServiceResult<Post>.Failure("L'identifiant reçu est invalide.");
        }

        var existingPost = await _context.Posts.FindAsync(id);

        if (existingPost is null)
        {
            return ServiceResult<Post>.Failure("Le message est introuvable.");
        }

        try
        {
            existingPost.Texte = post.Texte;
            existingPost.Inactive = post.Inactive;

            await _context.SaveChangesAsync();
            return ServiceResult<Post>.Success(post);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex,
                "Erreur BD lors de la modification du message {TopicId}.", id);
            return ServiceResult<Post>.Failure("Le message n'a pas pu être modifié.");
        }
    }
}