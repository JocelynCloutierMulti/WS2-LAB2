using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FR_WS2_BaseLab.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly FrWs2BaselabContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(FrWs2BaselabContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult<List<Category>>> GetAllAsync(bool includeTopics = false)
    {
        try
        {
            IQueryable<Category> query = _context.Categories.AsNoTracking();

            if (includeTopics)
            {
                query = query.Include(c => c.Topics);
            }

            var categories = await query.OrderBy(c => c.Name).ToListAsync();
            return ServiceResult<List<Category>>.Success(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement des catégories.");
            return ServiceResult<List<Category>>.Failure("Les catégories n'ont pas pu être chargées.");
        }
    }

    public async Task<ServiceResult<Category>> GetDetailsAsync(int id)
    {
        try
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category is null)
            {
                return ServiceResult<Category>.Failure("La catégorie est introuvable.");
            }

            return ServiceResult<Category>.Success(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement de la catégorie {CategoryId}.", id);
            return ServiceResult<Category>.Failure("La catégorie n'a pas pu être chargée.");
        }
    }

    public Task<ServiceResult<Category>> GetForEditAsync(int id)
    {
        return GetDetailsAsync(id);
    }

    public async Task<ServiceResult<Category>> CreateAsync(Category category)
    {
        try
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return ServiceResult<Category>.Success(category);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erreur BD lors de la création d'une catégorie.");
            return ServiceResult<Category>.Failure("La catégorie n'a pas pu être créée.");
        }
    }

    public async Task<ServiceResult<Category>> UpdateAsync(int id, Category category)
    {
        if (id != category.Id)
        {
            return ServiceResult<Category>.Failure("L'identifiant reçu est invalide.");
        }

        var existingCategory = await _context.Categories.FindAsync(id);

        if (existingCategory is null)
        {
            return ServiceResult<Category>.Failure("La catégorie est introuvable.");
        }

        try
        {
            existingCategory.Inactive = category.Inactive;
            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.Image = category.Image;

            await _context.SaveChangesAsync();
            return ServiceResult<Category>.Success(existingCategory);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erreur BD lors de la modification de la catégorie {CategoryId}.", id);
            return ServiceResult<Category>.Failure("La catégorie n'a pas pu être modifiée.");
        }
    }

    public async Task<ServiceResult<Category>> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category is null)
        {
            return ServiceResult<Category>.Failure("La catégorie est introuvable.");
        }

        try
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return ServiceResult<Category>.Success(category);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erreur BD lors de la suppression de la catégorie {CategoryId}.", id);
            return ServiceResult<Category>.Failure("La catégorie ne peut pas être supprimée. Elle est peut-être liée à des sujets ou des images.");
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id);
    }
}
