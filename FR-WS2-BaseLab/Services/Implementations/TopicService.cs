using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FR_WS2_BaseLab.Services.Implementations;

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

    public async Task<ServiceResult<Topic>> CreateAsync(Topic topic, string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return ServiceResult<Topic>.Failure("Vous devez être connecté.");
        }

        try
        {
            topic.UserId = userId;
            topic.Date = DateTime.Now;
            topic.Views = 0;
            topic.Inactive = false;

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();

            return ServiceResult<Topic>.Success(topic);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erreur BD lors de la création d'un sujet.");
            return ServiceResult<Topic>.Failure("Le sujet n'a pas pu être créé.");
        }
    }


    public async Task<ServiceResult<Topic>> DeleteAsync(int id)
    {
        var topic = await _context.Topics.FindAsync(id);

        if (topic is null)
        {
            return ServiceResult<Topic>.Failure("Le sujet est introuvable.");
        }

        try
        {
            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();
            return ServiceResult<Topic>.Success(topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erreur BD lors de la suppression du sujet {TopicId}.", id);

            return ServiceResult<Topic>.Failure(
                "Le sujet ne peut pas être supprimé. Il contient peut-être des messages.");
        }
    }


    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Topics.AnyAsync(e => e.Id == id);
    }

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

        return ServiceResult<List<Topic>>.Success(topics);
    } catch (Exception ex)
    {
        _logger.LogError(ex,
            "Erreur lors du chargement des sujets de la catégorie {CategoryId}.",
            categoryId);

        return ServiceResult<List<Topic>>.Failure(
            "Les sujets n'ont pas pu être chargés.");
    }
   
}


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
                return ServiceResult<Topic>.Failure("Le sujet est introuvable.");
            }

            return ServiceResult<Topic>.Success(topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erreur lors du chargement du sujet {TopicId}.", id);

            return ServiceResult<Topic>.Failure(
                "Le sujet n'a pas pu être chargé.");
        }
    }


    public async Task<ServiceResult<Topic>> GetForEditAsync(int id)
    {
        try
        {     
            var topic = await _context.Topics
                .FirstOrDefaultAsync(t => t.Id == id);

            if (topic is null)
            {
                return ServiceResult<Topic>.Failure("Le sujet à modifier est introuvable.");
            }

            return ServiceResult<Topic>.Success(topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du sujet {TopicId}.", id);
            return ServiceResult<Topic>.Failure("Une erreur est survenue lors du chargement du sujet.");
        }
    }

    public async Task<ServiceResult<Topic>> UpdateAsync(int id, Topic topic)
    {
        if (id != topic.Id)
        {
            return ServiceResult<Topic>.Failure("L'identifiant reçu est invalide.");
        }

        var existingTopic = await _context.Topics.FindAsync(id);

        if (existingTopic is null)
        {
            return ServiceResult<Topic>.Failure("Le sujet est introuvable.");
        }

        try
        {
            existingTopic.Title = topic.Title;
            existingTopic.Texte = topic.Texte;
            existingTopic.Inactive = topic.Inactive;

            await _context.SaveChangesAsync();
            return ServiceResult<Topic>.Success(topic);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex,
                "Erreur BD lors de la modification du sujet {TopicId}.", id);
            return ServiceResult<Topic>.Failure("Le sujet n'a pas pu être modifié.");
        }
    }

}

