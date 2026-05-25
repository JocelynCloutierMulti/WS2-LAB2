using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services;
using FR_WS2_BaseLab.Services.Email;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FR_WS2_BaseLab.Services.Implementations
{
    public class PostService : IPostService
    {
        private readonly FrWs2BaselabContext _context;
        private readonly IApplicationEmailSender _emailSender;
        private readonly ILogger<PostService> _logger;

        public PostService(
            FrWs2BaselabContext context,
            IApplicationEmailSender emailSender,
            ILogger<PostService> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        // Liste des messages d'un sujet.
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

                return ServiceResult<List<Post>>.Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erreur lors du chargement des messages du sujet {TopicId}.",
                    topicId);
                return ServiceResult<List<Post>>.Fail(
                    "Les messages n'ont pas pu être chargés.");
            }
        }

        public async Task<ServiceResult<Post>> GetDetailsAsync(int id)
        {
            try
            {
                var post = await _context.Posts
                    .AsNoTracking()
                    .Include(p => p.Top)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (post is null)
                {
                    return ServiceResult<Post>.Fail("Le message est introuvable.");
                }

                return ServiceResult<Post>.Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erreur lors du chargement du message {PostId}.", id);
                return ServiceResult<Post>.Fail(
                    "Le message n'a pas pu être chargé.");
            }
        }

        public async Task<ServiceResult<Post>> GetForEditAsync(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            return post is null
                ? ServiceResult<Post>.Fail("Le message est introuvable.")
                : ServiceResult<Post>.Ok(post);
        }

        public async Task<ServiceResult<Post>> CreateAsync(Post post, string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ServiceResult<Post>.Fail("Vous devez être connecté.");
            }

            try
            {
                // Valeurs définies côté serveur.
                post.UserId = userId;
                post.Date = DateOnly.FromDateTime(DateTime.Now);
                post.Inactive = false;

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                // Lab 3 : envoyer une notification à l'auteur du sujet.
                // L'échec d'envoi ne doit PAS annuler la création du message.
                await TryNotifyTopicAuthorAsync(post);

                return ServiceResult<Post>.Ok(post);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erreur BD lors de la création d'un message.");
                return ServiceResult<Post>.Fail("Le message n'a pas pu être créé.");
            }
        }

        public async Task<ServiceResult<Post>> UpdateAsync(int id, Post post)
        {
            if (id != post.Id)
            {
                return ServiceResult<Post>.Fail("L'identifiant reçu est invalide.");
            }

            var existing = await _context.Posts.FindAsync(id);
            if (existing is null)
            {
                return ServiceResult<Post>.Fail("Le message est introuvable.");
            }

            try
            {
                existing.Texte = post.Texte;
                existing.Inactive = post.Inactive;

                await _context.SaveChangesAsync();
                return ServiceResult<Post>.Ok(existing);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,
                    "Erreur BD lors de la modification du message {PostId}.", id);
                return ServiceResult<Post>.Fail("Le message n'a pas pu être modifié.");
            }
        }

        public async Task<ServiceResult<Post>> DeleteAsync(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post is null)
            {
                return ServiceResult<Post>.Fail("Le message est introuvable.");
            }

            try
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
                return ServiceResult<Post>.Ok(post);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,
                    "Erreur BD lors de la suppression du message {PostId}.", id);
                return ServiceResult<Post>.Fail(
                    "Le message ne peut pas être supprimé.");
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Posts.AnyAsync(p => p.Id == id);
        }

        // -----------------------------------------------------------------
        // Lab 3 : Notification email à l'auteur du sujet.
        // -----------------------------------------------------------------
        private async Task TryNotifyTopicAuthorAsync(Post newPost)
        {
            try
            {
                var topic = await _context.Topics
                    .AsNoTracking()
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == newPost.TopId);

                if (topic?.User is null)
                {
                    return;
                }

                // Ne pas notifier l'auteur s'il répond à son propre sujet.
                if (topic.UserId == newPost.UserId)
                {
                    return;
                }

                // Ne pas envoyer si l'adresse n'est pas confirmée ou vide.
                if (!topic.User.EmailConfirmed ||
                    string.IsNullOrWhiteSpace(topic.User.Email))
                {
                    return;
                }

                var subject = $"Nouveau message dans : {topic.Title}";
                var html = $@"<p>Un nouveau message a été ajouté dans votre sujet.</p>
                           <p><strong>{System.Net.WebUtility.HtmlEncode(topic.Title)}</strong></p>
                           <p>Connectez-vous à FR-WS2 BaseLab pour consulter la réponse.</p>";

                await _emailSender.SendAsync(topic.User.Email, subject, html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Échec de l'envoi de la notification pour le message {PostId} du sujet {TopicId}.",
                    newPost.Id, newPost.TopId);
            }
        }
    }
}