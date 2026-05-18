using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Email;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FR_WS2_BaseLab.Services.Implementations
{
	public class PostService : IPostService
	{
		private readonly FrWs2BaselabContext _context;
		private readonly IApplicationEmailSender _emailSender ;
		private readonly ILogger<PostService> _logger;

		public PostService(
			FrWs2BaselabContext context,
			ILogger<PostService> logger , IApplicationEmailSender emailSender)
		{
			_context = context;
			_logger = logger;
			_emailSender =emailSender;
		}

		// Liste des messages d'un sujet (Index).
		public async Task<ServiceResult<List<Post>>> GetByTopicIdAsync(int topicId)
		{
			try
			{
				var posts = await _context.Posts
					.AsNoTracking()
					.Include(p => p.User)
					.Where(p => p.TopId == topicId)   // <-- filtre corrigé
					.OrderBy(p => p.Date)
					.ToListAsync();

				return ServiceResult<List<Post>>.Success(posts);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"Erreur lors du chargement des messages du sujet {TopicId}.", topicId);
				return ServiceResult<List<Post>>.Failure(
					"Les messages n'ont pas pu être chargés.");
			}
		}

		// Détails d'un message.
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

		// Chargement pour édition (formulaire Edit GET).
		public async Task<ServiceResult<Post>> GetForEditAsync(int id)
		{
			var post = await _context.Posts.FindAsync(id);
			return post is null
				? ServiceResult<Post>.Failure("Le message est introuvable.")
				: ServiceResult<Post>.Success(post);
		}

		// Création d'un message.
		public async Task<ServiceResult> CreateAsync(Post post, string? userId)
		{
			if (string.IsNullOrWhiteSpace(userId))
			{
				return ServiceResult.Failure("Vous devez être connecté.");
			}

			try
			{
				
				post.UserId = userId;
				post.Date = DateTime.Now;
				post.Inactive = false;

				_context.Posts.Add(post);
				await _context.SaveChangesAsync();
				var topic = await _context.Topics
					.Include(t => t.User)
					.FirstOrDefaultAsync(t => t.Id == post.TopId);

				var subject = $"Nouveau message dans :{topic.Title}";
				var html = $@"
							<p> Bonjour, message  a été ajouter dans ce sujet </p>";
				try
				{
					await _emailSender.SendAsync(topic.User.Email, subject, html);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Échec d'envoi");
				}


				return ServiceResult.Success();
			}
			catch (DbUpdateException ex)
			{
				_logger.LogError(ex, "Erreur BD lors de la création d'un message.");
				return ServiceResult.Failure("Le message n'a pas pu être créé.");
			}
		}

		// Modification d'un message.
		public async Task<ServiceResult> UpdateAsync(int id, Post post)
		{
			if (id != post.Id)
			{
				return ServiceResult.Failure("L'identifiant reçu est invalide.");
			}

			var existing = await _context.Posts.FindAsync(id);
			if (existing is null)
			{
				return ServiceResult.Failure("Le message est introuvable.");
			}

			try
			{
				// Anti-sur-publication : on ne copie que les champs autorisés.
				existing.Texte = post.Texte;
				existing.Inactive = post.Inactive;

				await _context.SaveChangesAsync();
				return ServiceResult.Success();
			}
			catch (DbUpdateException ex)
			{
				_logger.LogError(ex,
					"Erreur BD lors de la modification du message {PostId}.", id);
				return ServiceResult.Failure("Le message n'a pas pu être modifié.");
			}
		}

		// Suppression d'un message.
		public async Task<ServiceResult> DeleteAsync(int id)
		{
			var post = await _context.Posts.FindAsync(id);
			if (post is null)
			{
				return ServiceResult.Failure("Le message est introuvable.");
			}

			try
			{
				_context.Posts.Remove(post);
				await _context.SaveChangesAsync();
				return ServiceResult.Success();
			}
			catch (DbUpdateException ex)
			{
				_logger.LogError(ex,
					"Erreur BD lors de la suppression du message {PostId}.", id);
				return ServiceResult.Failure("Le message n'a pas pu être supprimé.");
			}
		}

		public async Task<bool> ExistsAsync(int id)
		{
			return await _context.Posts.AnyAsync(p => p.Id == id);
		}
	}
}