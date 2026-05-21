using FR_WS2_BaseLab.Models;

using FR_WS2_BaseLab.Services.Interfaces;

using Microsoft.EntityFrameworkCore;



namespace FR_WS2_BaseLab.Services.Implementations;



public class CategoriesService : ICategoriesService

{

    private readonly FrWs2BaselabContext _context;

    private readonly ILogger<CategoriesService> _logger;



    public CategoriesService(

        FrWs2BaselabContext context,

        ILogger<CategoriesService> logger)

    {

        _context = context;

        _logger = logger;

    }

   

   
    public  async Task<ServiceResult<Category>> CreateAsync(Category category)
    {
        //if (string.IsNullOrWhiteSpace(userId))
        //{
        //    return ServiceResult<Category>.Failure("Vous devez être connecté.");
        //}

        try
        {
            category.Inactive = false;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return ServiceResult<Category>.Success(category);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erreur BD lors de la création d'une categorie.");
            return ServiceResult<Category>.Failure("La categorie n'a pas pu être créé.");
        }

    }

    public  async Task<ServiceResult<Category>> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category is null)
        {
            return ServiceResult<Category>.Failure("La categorie est introuvable.");
        }

        try
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return ServiceResult<Category>.Success(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erreur BD lors de la suppression de la categorie {Id}.", id);

            return ServiceResult<Category>.Failure(
                "La categorie ne peut pas être supprimé. Il contient peut-être des sujets.");
        }
    }

    public Task<bool> ExistsAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<List<Category>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<Category>> GetByIdAsync(int id)
    {
        try
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category is null)
            {
                return ServiceResult<Category>.Failure("Le message est introuvable.");
            }

            return ServiceResult<Category>.Success(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erreur lors du chargement de la categorie {CtegoryyId}.", id);

            return ServiceResult<Category>.Failure(
                "La categorie n'a pas pu être chargé.");
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
            return ServiceResult<Category>.Failure("Le sujet est introuvable.");
        }

        try
        {
            existingCategory.Name =category.Name ;
            existingCategory.Description = category.Description;
            existingCategory.Inactive = category.Inactive;

            await _context.SaveChangesAsync();
            return ServiceResult<Category>.Success(category);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex,
                "Erreur BD lors de la modification de la categorie {CategoryId}.", id);
            return ServiceResult<Category>.Failure("La category n'a pas pu être modifié.");
        }
    }

    //Task<ServiceResult<Category>> ICategoriesService.CreateAsync(Category category)
    //{
    //    throw new NotImplementedException();
    //}
}



    

