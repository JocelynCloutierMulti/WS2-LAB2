using FR_WS2_BaseLab.Models;

using FR_WS2_BaseLab.Models.ViewModels;

using FR_WS2_BaseLab.Services.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace FR_WS2_BaseLab.Services.Implementations;

public class CategoryImageService : ICategoryImageService

{

    private readonly FrWs2BaselabContext _context;

    private readonly IWebHostEnvironment _environment;

    private readonly ILogger<CategoryImageService> _logger;

    private const long MaxFileSize = 2 * 1024 * 1024; // 2 Mo

    private static readonly string[] AllowedExtensions =

        [".jpg", ".jpeg", ".png", ".webp"];

    private static readonly string[] AllowedContentTypes =

        ["image/jpeg", "image/png", "image/webp"];

    public CategoryImageService(

        FrWs2BaselabContext context,

        IWebHostEnvironment environment,

        ILogger<CategoryImageService> logger)

    {

        _context = context;

        _environment = environment;

        _logger = logger;

    }

    private static CategoryImageDto ToDto(CategoryImage image)

    {

        return new CategoryImageDto

        {

            Id = image.Id,

            CategoryId = image.CategoryId,

            Url = $"/uploads/categories/{image.CategoryId}/{image.FileName}",

            OriginalFileName = image.OriginalFileName,

            ContentType = image.ContentType,

            SizeBytes = image.SizeBytes,

            AltText = image.AltText

        };

    }

    public async Task<IReadOnlyList<CategoryImageDto>> GetForCategoryAsync(int categoryId)

    {

        var images = await _context.CategoryImages

            .Where(i => i.CategoryId == categoryId)

            .OrderByDescending(i => i.UploadedAtUtc)

            .Select(i => ToDto(i))

            .ToListAsync();

        return images;

    }

    public async Task<ServiceResult<CategoryImageDto>> UploadAsync(

        int categoryId,

        IFormFile file,

        string? altText)

    {

        if (file is null || file.Length == 0)

            return ServiceResult<CategoryImageDto>.Fail("Aucun fichier reçu.");

        if (file.Length > MaxFileSize)

            return ServiceResult<CategoryImageDto>.Fail("Le fichier dépasse 2 Mo.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))

            return ServiceResult<CategoryImageDto>.Fail("Extension non autorisée.");

        if (!AllowedContentTypes.Contains(file.ContentType))

            return ServiceResult<CategoryImageDto>.Fail("Type de fichier non autorisé.");

        var categoryExists = await _context.Categories

            .AnyAsync(c => c.Id == categoryId);

        if (!categoryExists)

            return ServiceResult<CategoryImageDto>.Fail("Catégorie introuvable.");

        var serverFileName = $"{Guid.NewGuid():N}{extension}";

        var uploadRoot = Path.Combine(

            _environment.WebRootPath,

            "uploads",

            "categories",

            categoryId.ToString());

        Directory.CreateDirectory(uploadRoot);

        var physicalPath = Path.Combine(uploadRoot, serverFileName);

        try

        {

            await using (var stream = File.Create(physicalPath))

            {

                await file.CopyToAsync(stream);

            }

            var image = new CategoryImage

            {

                CategoryId = categoryId,

                FileName = serverFileName,

                OriginalFileName = Path.GetFileName(file.FileName),

                ContentType = file.ContentType,

                SizeBytes = file.Length,

                AltText = altText,

                UploadedAtUtc = DateTime.UtcNow

            };

            _context.CategoryImages.Add(image);

            await _context.SaveChangesAsync();

            return ServiceResult<CategoryImageDto>.Ok(ToDto(image));

        }

        catch (Exception ex)

        {

            _logger.LogError(ex,

                "Erreur lors du téléversement de l'image pour la catégorie {CategoryId}.",

                categoryId);

            // Nettoyer le fichier orphelin si la sauvegarde BD a échoué

            if (File.Exists(physicalPath))

            {

                File.Delete(physicalPath);

            }

            return ServiceResult<CategoryImageDto>.Fail(

                "Erreur lors du téléversement de l'image.");

        }

    }

    public async Task<ServiceResult<bool>> DeleteAsync(int imageId)

    {

        var image = await _context.CategoryImages

            .FirstOrDefaultAsync(i => i.Id == imageId);

        if (image is null)

            return ServiceResult<bool>.Fail("Image introuvable.");

        var physicalPath = Path.Combine(

            _environment.WebRootPath,

            "uploads",

            "categories",

            image.CategoryId.ToString(),

            image.FileName);

        try

        {

            _context.CategoryImages.Remove(image);

            await _context.SaveChangesAsync();

            if (File.Exists(physicalPath))

            {

                File.Delete(physicalPath);

            }

            return ServiceResult<bool>.Ok(true);

        }

        catch (Exception ex)

        {

            _logger.LogError(ex,

                "Erreur lors de la suppression de l'image {ImageId}.",

                imageId);

            return ServiceResult<bool>.Fail(

                "Erreur lors de la suppression de l'image.");

        }

    }

}
