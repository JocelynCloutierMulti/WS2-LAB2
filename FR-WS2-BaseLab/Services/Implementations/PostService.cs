using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Exchange.WebServices.Data;

namespace FR_WS2_BaseLab.Services.Implementations
{
	public class PostService : IPostService
	{
		private readonly FrWs2BaselabContext _context;
		private readonly ILogger<PostService> _logger;

		public PostService(
			FrWs2BaselabContext context,
			ILogger<PostService> logger)
		{
			_context = context;
			_logger = logger;
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

				return ServiceResult<List<Post>>.Ok(posts);
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

		// Chargement pour édition (formulaire Edit GET).
		public async Task<ServiceResult<Post>> GetForEditAsync(int id)
		{
			var post = await _context.Posts.FindAsync(id);
			return post is null
				? ServiceResult<Post>.Fail("Le message est introuvable.")
				: ServiceResult<Post>.Ok(post);
		}

		// Création d'un message.
		public async Task<ServiceResult<Post>> CreateAsync(Post post, string? userId)
		{
			if (string.IsNullOrWhiteSpace(userId))
			{
				return ServiceResult<Post>.Fail("Vous devez être connecté.");
			}

			try
			{
				// Valeurs définies côté serveur (anti-manipulation).
				post.UserId = userId;
				post.Date = DateOnly.FromDateTime(DateTime.Now);
				post.Inactive = false;

				_context.Posts.Add(post);
				await _context.SaveChangesAsync();

				return ServiceResult<Post>.Ok(post);
			}
			catch (DbUpdateException ex)
			{
				_logger.LogError(ex, "Erreur BD lors de la création d'un message.");
				return ServiceResult<Post>.Fail("Le message n'a pas pu être créé.");
			}
		}

		// Modification d'un message.
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
				// Anti-sur-publication : on ne copie que les champs autorisés.
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

		// Suppression d'un message.
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
				return ServiceResult<Post>.Fail("Le message n'a pas pu être supprimé.");
			}
		}

		public async Task<bool> ExistsAsync(int id)
		{
			return await _context.Posts.AnyAsync(p => p.Id == id);
		}
	}
}
