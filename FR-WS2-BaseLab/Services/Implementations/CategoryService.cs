using System.Drawing;
using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FR_WS2_BaseLab.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly FrWs2BaselabContext _context;
    private readonly ILogger<CategoryService> _logger;
    private readonly ICategoryImageService _categoryImageService;

    public CategoryService(FrWs2BaselabContext context, ILogger<CategoryService> logger, ICategoryImageService categoryImageService)
    {
        _context = context;
        _logger = logger;
        _categoryImageService = categoryImageService;
    }

     public async Task<ServiceResult<List<Category>>> GetAllAsync()
    {
        try{
            var cats = await _context.Categories
                .Include(c => c.CategoryImages)
                .ToListAsync();
            return ServiceResult<List<Category>>.Success(cats);
        }catch (Exception ex){
            _logger.LogError(ex, "Erreur lors du chargement des catégories.");
            return ServiceResult<List<Category>>.Failure("Les catégories n'ont pas pu être chargées.");
        }
    }
    
    public async Task<ServiceResult<Category>> GetByIdAsync(int id)
    {
        try{
            var cat = await _context.Categories
                .Include(c => c.CategoryImages)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (cat is null) return ServiceResult<Category>.Failure("La catégorie est introuvable.");
            return ServiceResult<Category>.Success(cat);
        }catch (Exception ex){
            _logger.LogError(ex, "Erreur lors du chargement de la catégorie {CategoryId}.", id);
            return ServiceResult<Category>.Failure("La catégorie n'a pas pu être chargée.");
        }
    }

    public async Task<ServiceResult<Category>> CreateAsync(Category category)
    {
        if (category.Description.Length > 250)
            return ServiceResult<Category>.Failure("La description ne doit pas dépasser 250 caractères.");
        try{
            category.Inactive = false;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return ServiceResult<Category>.Success(category);
        }catch (DbUpdateException ex){
            _logger.LogError(ex, "Erreur BD lors de la création d'une catégorie.");
            return ServiceResult<Category>.Failure("La catégorie n'a pas pu être créée.");
        }
    }

    public async Task<ServiceResult<Category>> UpdateAsync(int id, Category categorie)
    {
        if (id != categorie.Id) return ServiceResult<Category>.Failure("L'identifiant reçu est invalide.");
        var existingCategory = await _context.Categories
            .Include(c => c.CategoryImages)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (existingCategory is null) return ServiceResult<Category>.Failure("La catégorie est introuvable.");
        if (categorie.Description.Length > 250)
            return ServiceResult<Category>.Failure("La description ne doit pas dépasser 250 caractères.");
        try{
            existingCategory.Description = categorie.Description;
            existingCategory.Name = categorie.Name;
            existingCategory.Inactive = categorie.Inactive;
            await _context.SaveChangesAsync();
            return ServiceResult<Category>.Success(existingCategory);
        }catch (DbUpdateException ex){
                _logger.LogError(ex, "Erreur BD lors de la modification de la catégorie {CategoryId}.", id);
                return ServiceResult<Category>.Failure("La catégorie n'a pas pu être modifiée.");}
    }

    public async Task<ServiceResult<Category>> DeleteAsync(int id)
    {
        var cat = await _context.Categories
            .Include(c => c.CategoryImages)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (cat is null) return ServiceResult<Category>.Failure("La catégorie est introuvable.");
        try{
            _context.Categories.Remove(cat);
            await _context.SaveChangesAsync();
            return ServiceResult<Category>.Success(cat);
        }catch (Exception ex){
            _logger.LogError(ex, "Erreur BD lors de la suppression de la catégorie {CategoryId}.", id);
            return ServiceResult<Category>.Failure( "La catégorie n'a pas pu être supprimée. Elle contient peut-être des sujets.");
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id);
    }

}