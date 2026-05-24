using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Exchange.WebServices.Data;

namespace FR_WS2_BaseLab.Services.Implementations
{
    public class TopicService : ITopicService
    {
        private readonly FrWs2BaselabContext _context;
        private readonly ILogger<TopicService> _logger;
 
        public TopicService(
            FrWs2BaselabContext context,
            ILogger<TopicService> logger)
        {
            _context = context;
            _logger = logger;
        }
 
        // Liste des sujets d'une catégorie (Index).
        public async Task<ServiceResult<List<Topic>>> GetByCategoryIdAsync(int categoryId)
        {
            try
            {
                var topics = await _context.Topics
                    .AsNoTracking()
                    .Include(t => t.User)
                    .Where(t => t.CatId == categoryId)   
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();
 
                return ServiceResult<List<Topic>>.Ok(topics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erreur lors du chargement des sujets de la catégorie {CategoryId}.",
                    categoryId);
                return ServiceResult<List<Topic>>.Fail(
                    "Les sujets n'ont pas pu être chargés.");
            }
        }
 
        // Détails d'un sujet.
        public async Task<ServiceResult<Topic>> GetDetailsAsync(int id)
        {
            try
            {
                var topic = await _context.Topics
                    .AsNoTracking()
                    .Include(t => t.Cat)
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == id);
 
                if (topic is null)
                {
                    return ServiceResult<Topic>.Fail("Le sujet est introuvable.");
                }
 
                return ServiceResult<Topic>.Ok(topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erreur lors du chargement du sujet {TopicId}.", id);
				return ServiceResult<Topic>.Fail(
					"Le sujet n'a pas pu être chargé.");
            }
        }
 
        // Chargement pour édition (formulaire Edit GET).
        public async Task<ServiceResult<Topic>> GetForEditAsync(int id)
        {
            var topic = await _context.Topics.FindAsync(id);
            return topic is null
                ? ServiceResult<Topic>.Fail("Le sujet est introuvable.")
                : ServiceResult<Topic>.Ok(topic);
        }
 
        // Création d'un sujet.
        public async Task<ServiceResult<Topic>> CreateAsync(Topic topic, string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ServiceResult<Topic>.Fail("Vous devez être connecté.");
            }
 
            try
            {
                // Valeurs définies côté serveur (anti-manipulation).
                topic.UserId = userId;
                topic.Date = DateTime.Now;
                topic.Views = 0;
                topic.Inactive = false;
 
                _context.Topics.Add(topic);
                await _context.SaveChangesAsync();
 
                return ServiceResult<Topic>.Ok(topic);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erreur BD lors de la création d'un sujet.");
                return ServiceResult<Topic>.Fail("Le sujet n'a pas pu être créé.");
            }
        }
 
        // Modification d'un sujet.
        public async Task<ServiceResult<Topic>> UpdateAsync(int id, Topic topic)
        {
            if (id != topic.Id)
            {
                return ServiceResult<Topic>.Fail("L'identifiant reçu est invalide.");
            }
 
            var existing = await _context.Topics.FindAsync(id);
            if (existing is null)
            {
                return ServiceResult<Topic>.Fail("Le sujet est introuvable.");
            }
 
            try
            {
                // Anti-sur-publication : on ne copie que les champs autorisés.
                existing.Title = topic.Title;
                existing.Texte = topic.Texte;
                existing.Inactive = topic.Inactive;
 
                await _context.SaveChangesAsync();
                return ServiceResult<Topic>.Ok(existing);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,
                    "Erreur BD lors de la modification du sujet {TopicId}.", id);
                return ServiceResult<Topic>.Fail("Le sujet n'a pas pu être modifié.");
            }
        }
 
        // Suppression d'un sujet.
        public async Task<ServiceResult<Topic>>DeleteAsync(int id)
        {
            var topic = await _context.Topics.FindAsync(id);
            if (topic is null)
            {
                return ServiceResult<Topic>.Fail("Le sujet est introuvable.");
            }
 
            try
            {
                _context.Topics.Remove(topic);
                await _context.SaveChangesAsync();
                return ServiceResult<Topic>.Ok(topic);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,
                    "Erreur BD lors de la suppression du sujet {TopicId}.", id);
                return ServiceResult<Topic>.Fail(
                    "Le sujet ne peut pas être supprimé. Il contient peut-être des messages.");
            }
        }
 
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Topics.AnyAsync(t => t.Id == id);
        }
    }
}