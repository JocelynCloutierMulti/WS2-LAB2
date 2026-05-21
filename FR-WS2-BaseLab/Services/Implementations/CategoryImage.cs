using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FR_WS2_BaseLab.Services.Implementations;

public class CategoryImageService(FrWs2BaselabContext context, ILogger<CategoryImageService> logger) : ICategoryImageService
{
    private readonly FrWs2BaselabContext _context = context;
    private readonly ILogger<CategoryImageService> _logger = logger;

    public async Task<ServiceResult<List<CategoryImage>>> GetImagesByCategoryAsync(int categoryId)
    {
        try{
            var images = await _context.CategoryImages.Where(t => t.CategoryId == categoryId).ToListAsync();
            return ServiceResult<List<CategoryImage>>.Success(images);
        }catch (Exception ex){
            _logger.LogError(ex, "Erreur lors du chargement des images de catégories.");
            return ServiceResult<List<CategoryImage>>.Failure("Les images de catégories n'ont pas pu être chargées.");
        }
    }

    public async Task<ServiceResult<CategoryImage>> CreateAsync(Category category, IFormFile imageFile)
    {
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot/images/img_categories");
        if (!Directory.Exists(uploadsFolder))Directory.CreateDirectory(uploadsFolder);
        string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
        string filePath = Path.Combine(uploadsFolder, fileName);
        await using var stream = new FileStream(filePath, FileMode.Create);
        await imageFile.CopyToAsync(stream);
        try{
            var anciennesImages = await _context.CategoryImages.Where(i => i.CategoryId == category.Id).ToListAsync();
            foreach (var img in anciennesImages) {img.AfficherImage = false;}
            CategoryImage catImg = new()
            {
                FileName = fileName,
                ContentType = imageFile.ContentType,
                OriginalFileName = imageFile.FileName,
                AltText = Path.GetFileNameWithoutExtension(imageFile.FileName),
                AfficherImage = true,
                CategoryId = category.Id,
                SizeInBytes = imageFile.Length,
                UploadedAtUtc = DateTime.UtcNow
            };
            _context.CategoryImages.Add(catImg);
            await _context.SaveChangesAsync();
            return ServiceResult<CategoryImage>.Success(catImg);
        }catch (DbUpdateException ex){
            _logger.LogError(ex, "Erreur BD lors de la création d'une image.");
            return ServiceResult<CategoryImage>.Failure("L'image n'a pas pu être créée.");}
    }

    public async Task<ServiceResult<CategoryImage>> SetMainImageAsync(int categoryId, int imageId)
    {
        var images = await _context.CategoryImages.Where(i => i.CategoryId == categoryId).ToListAsync();
        if (images.Count == 0) return ServiceResult<CategoryImage>.Failure("Aucune image.");
        var mainImage = images.FirstOrDefault(i => i.Id == imageId);
        if (mainImage is null) return ServiceResult<CategoryImage>.Failure("L'image principale est introuvable.");
        foreach (var img in images) {img.AfficherImage = false;}
        await _context.SaveChangesAsync();
        mainImage.AfficherImage = true;
        await _context.SaveChangesAsync();
        return ServiceResult<CategoryImage>.Success(mainImage);
    }

    public async Task<ServiceResult<CategoryImage>> DeleteAsync(int id)
    {
        var image = await _context.CategoryImages.FindAsync(id);
        if (image is null) return ServiceResult<CategoryImage>.Failure("L'image est introuvable.");
        try{
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/img_categories", image.FileName);
            _context.CategoryImages.Remove(image);
            await _context.SaveChangesAsync();
            if (File.Exists(filePath)) File.Delete(filePath);
            return ServiceResult<CategoryImage>.Success(image);
        }catch (Exception ex){
            _logger.LogError(ex, "Erreur lors de la suppression de l'image {CategoryImageId}.", id);
            return ServiceResult<CategoryImage>.Failure("L'image n'a pas pu être supprimée.");}
    }

    public async Task<ServiceResult<CategoryImage>> DeleteAllAsync(int categoryId)
    {
        var images = await _context.CategoryImages.Where(i => i.CategoryId == categoryId).ToListAsync();
        if (images.Count == 0) return ServiceResult<CategoryImage>.Success(null!);
        try{
            foreach (var image in images)
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/img_categories", image.FileName);
                if (File.Exists(filePath))File.Delete(filePath);
            }
            _context.CategoryImages.RemoveRange(images);
            await _context.SaveChangesAsync();
            return ServiceResult<CategoryImage>.Success(null!);
        } catch (Exception ex) {
            _logger.LogError(ex, "Erreur lors de la suppression des images de la catégorie {CategoryId}.",categoryId);
            return ServiceResult<CategoryImage>.Failure("Les images de cette catégorie n'ont pas pu être supprimées.");}
    }
}