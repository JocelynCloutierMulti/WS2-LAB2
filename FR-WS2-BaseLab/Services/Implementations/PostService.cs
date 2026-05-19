using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using FR_WS2_BaseLab.Services.Email;

namespace FR_WS2_BaseLab.Services.Implementations;

public class PostService(FrWs2BaselabContext context, ILogger<PostService> logger, IApplicationEmailSender emailSender) : IPostService
{
    private readonly IApplicationEmailSender _emailSender = emailSender;
    private readonly FrWs2BaselabContext _context = context;
    private readonly ILogger<PostService> _logger = logger;

    /// <summary>
    /// Récupère tous les messages d'un sujet donné par son identifiant. Les messages sont triés par date de création, du plus récent au plus ancien. Les informations de l'utilisateur qui a créé chaque message et du sujet auquel chaque message appartient sont également incluses.
    /// </summary>
    /// <param name="sujetId">L'identifiant du sujet.</param>
    /// <returns>Le résultat du service contenant la liste des messages du sujet.</returns>
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

    /// <summary>
    /// Récupère les détails d'un message par son identifiant. Les détails incluent les informations de l'utilisateur qui a créé le message et du sujet auquel le message appartient.
    /// </summary>
    /// <param name="id">L'identifiant du message.</param>
    /// <returns>Le résultat du service contenant les détails du message.</returns>
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

    /// <summary>
    /// Crée un nouveau message. L'utilisateur doit être connecté pour créer un message. Le champ UserId du message est automatiquement assigné à l'identifiant de l'utilisateur connecté, et le champ Date est assigné à la date et l'heure actuelles. Après la création du message, un courriel est envoyé à l'utilisateur du sujet pour l'aviser qu'un nouveau message a été ajouté.
    /// </summary>
    /// <param name="Post">Le message à créer.</param>
    /// <param name="userId">L'identifiant de l'utilisateur connecté.</param>
    /// <returns>Le résultat du service contenant le message créé.</returns>
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
            await AviserUsagerSujet(Post.Id);
            return ServiceResult<Post>.Success(Post);
        }catch (DbUpdateException ex) {
            _logger.LogError(ex, "Erreur BD lors de la création d'un message.");
            return ServiceResult<Post>.Failure("Le message n'a pas pu être créé.");
        }
    }

    /// <summary>
    /// Récupère un message pour l'édition. Seuls les champs Texte et Inactive peuvent être modifiés par un utilisateur régulier. Un administrateur peut également modifier les champs UserId, Date et TopId.
    /// </summary>
    /// <param name="id">L'identifiant du message.</param>
    /// <returns>Le résultat du service contenant le message à éditer.</returns>
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

    /// <summary>
    /// Met à jour un message existant. Si adminTrue est vrai, les champs UserId, Date et TopId peuvent également être modifiés.    
    /// </summary>
    /// <param name="id">L'identifiant du message à mettre à jour.</param>
    /// <param name="Post">Le message à mettre à jour.</param>
    /// <param name="adminTrue">Indique si l'utilisateur est un administrateur.</param>
    /// <returns>Le résultat du service contenant le message mis à jour.</returns>
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

    /// <summary>
    /// Supprime un message par son identifiant.
    /// </summary>
    /// <param name="id">L'identifiant du message.</param>
    /// <returns>Le résultat du service contenant le message supprimé.</returns>
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

    /// <summary>
    /// Vérifie si un message existe.
    /// </summary>
    /// <param name="id">L'identifiant du message.</param>
    /// <returns>True si le message existe, sinon false.</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Posts.AnyAsync(c => c.Id == id);
    }

    /// <summary>
    /// Envoie un courriel à l'utilisateur du sujet pour l'aviser qu'un nouveau message a été ajouté.
    /// </summary>
    /// <param name="postId">L'identifiant du message.</param>
    /// <returns>Le résultat du service contenant le message.</returns>
    public async Task<ServiceResult<Post>> AviserUsagerSujet(int postId)
    {
        var post = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == postId);
        if (post is null) return ServiceResult<Post>.Failure("Le message est introuvable.");
        var topic = await _context.Topics
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == post.TopId);
        if (topic is null)
        {
            _logger.LogWarning("Sujet introuvable pour le message {PostId}", postId);
            return ServiceResult<Post>.Success(post);
        }
        if (topic?.User?.EmailConfirmed == true &&
            !string.IsNullOrWhiteSpace(topic.User.Email) &&
            topic.UserId != post.UserId)
        {
            var subject = $"Nouveau message dans: {topic.Title}";
            var html = $"""
                <p>Un nouveau message a été ajouté dans votre sujet.</p>
                <p><strong>{topic.Title}</strong></p>
                <p>Visitez <a href="http://localhost:5063/Posts/Details/{post.Id}">À vos Services</a> pour consulter la réponse.</p>
                """;
            try {
                await _emailSender.SendAsync(topic.User.Email, subject, html);
            } catch (Exception ex){
                _logger.LogError(ex, "Erreur lors de l'envoi du courriel.");
            }
        }
        return ServiceResult<Post>.Success(post);
    }

}