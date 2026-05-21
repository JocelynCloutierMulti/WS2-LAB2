using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.DTOs;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
 
namespace FR_WS2_BaseLab.Services.Implementations;
 
public class CategoryImageService : ICategoryImageService
{
    private readonly FrWs2BaselabContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<CategoryImageService> _logger;
 
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/webp"];
 
    private static readonly string[] AllowedExtensions =
        [".jpg", ".jpeg", ".png", ".webp"];
 
    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 Mo
 
    public CategoryImageService(
        FrWs2BaselabContext context,
        IWebHostEnvironment env,
        ILogger<CategoryImageService> logger)
    {
        _context = context;
        _env = env;
        _logger = logger;
    }
 
    public async Task<List<CategoryImageDto>> GetForCategoryAsync(int categoryId)
    {
        return await _context.CategoryImages
            .Where(ci => ci.CategoryId == categoryId)
            .Select(ci => new CategoryImageDto
            {
                Id = ci.Id,
                Url = $"/uploads/categories/{categoryId}/{ci.FileName}",
                OriginalFileName = ci.OriginalFileName,
                ContentType = ci.ContentType,
                SizeBytes = ci.SizeInBytes,
                AltText = ci.AltText
            })
            .ToListAsync();
    }
 
    public async Task<ServiceResult<CategoryImageDto>> UploadAsync(int categoryId, IFormFile file)
    {
        // Vérifier que la catégorie existe
        var category = await _context.Categories.FindAsync(categoryId);
        if (category == null)
            return ServiceResult<CategoryImageDto>.Fail("Catégorie introuvable.");
 
        // Validation taille 
        if (file == null || file.Length == 0)
            return ServiceResult<CategoryImageDto>.Fail("Aucun fichier reçu.");
 

        if (file.Length > MaxFileSizeBytes)
            return ServiceResult<CategoryImageDto>.Fail("Le fichier dépasse 2 Mo.");
 
        
        if (!AllowedContentTypes.Contains(file.ContentType.ToLower()))
            return ServiceResult<CategoryImageDto>.Fail("Type de fichier non accepté. Utilisez JPG, PNG ou WEBP.");
 
        
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExtensions.Contains(extension))
            return ServiceResult<CategoryImageDto>.Fail("Extension non acceptée.");
 
        // Créer le dossier par catégorie
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "categories", categoryId.ToString());
        Directory.CreateDirectory(uploadsFolder);
 
        //  GUID 
        var newFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, newFileName);
 
        
        try
        {
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la sauvegarde du fichier pour la catégorie {CategoryId}", categoryId);
            return ServiceResult<CategoryImageDto>.Fail("Erreur lors de la sauvegarde du fichier.");
        }
 
        
        var categoryImage = new CategoryImage
        {
            CategoryId = categoryId,
            FileName = newFileName,
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            SizeInBytes = file.Length,
            UploadedAtUtc = DateTime.UtcNow
        };
 
        _context.CategoryImages.Add(categoryImage);
        await _context.SaveChangesAsync();
 
        var dto = new CategoryImageDto
        {
            Id = categoryImage.Id,
            Url = $"/uploads/categories/{categoryId}/{newFileName}",
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            SizeBytes = file.Length
        };
 
        return ServiceResult<CategoryImageDto>.Ok(dto);
    }
 
    public async Task<ServiceResult<bool>> DeleteAsync(int imageId)
    {
        var image = await _context.CategoryImages.FindAsync(imageId);
        if (image == null)
            return ServiceResult<bool>.Fail("Image introuvable.");
 
        // Supprimer le fichier physique
        var filePath = Path.Combine(
            _env.WebRootPath, "uploads", "categories",
            image.CategoryId.ToString(), image.FileName);
 
        if (System.IO.File.Exists(filePath))
        {
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du fichier {FileName}", image.FileName);
            }
        }
 
        // Supprimer l'enregistrement en BD
        _context.CategoryImages.Remove(image);
        await _context.SaveChangesAsync();
 
        return ServiceResult<bool>.Ok(true);
    }
}