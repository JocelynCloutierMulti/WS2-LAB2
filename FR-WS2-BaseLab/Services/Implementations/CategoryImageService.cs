using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Models.ViewModels;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FR_WS2_BaseLab.Services.Implementations
{
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

        public async Task<ServiceResult<bool>> DeleteAsync(int imageId, int categoryId)
        {
            var image = await _context.CategoryImages
         .FirstOrDefaultAsync(i => i.Id == imageId && i.CategoryId == categoryId);

            if (image is null)
                return ServiceResult<bool>.Failure("Image introuvable.");

            var physicalPath = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "categories",
                image.CategoryId.ToString(),
                image.FileName);

            _context.CategoryImages.Remove(image);
            await _context.SaveChangesAsync();

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
            else
            {
                _logger.LogWarning(
                    "Fichier physique introuvable lors de la suppression : {PhysicalPath}",
                    physicalPath);
            }

            return ServiceResult<bool>.Success(true);
        }

        public async Task<IReadOnlyList<CategoryImageDto>> GetForCategoryAsync(int categoryId)
        {
            return await _context.CategoryImages
            .Where(i => i.CategoryId == categoryId)
            .OrderByDescending(i => i.UploadedAtUtc)
            .Select(i => ToDto(i))
            .ToListAsync();

        }

        public async Task<ServiceResult<CategoryImageDto>> UploadAsync(int categoryId, IFormFile file, string? altText)
        {
            if (file is null || file.Length == 0)
                return ServiceResult<CategoryImageDto>.Failure("Aucun fichier reçu.");

            if (file.Length > MaxFileSize)
                return ServiceResult<CategoryImageDto>.Failure("Le fichier dépasse 2 Mo.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                return ServiceResult<CategoryImageDto>.Failure("Extension non autorisée.");

            if (!AllowedContentTypes.Contains(file.ContentType))
                return ServiceResult<CategoryImageDto>.Failure("Type de fichier non autorisé.");

            if (!await HasValidSignatureAsync(file, extension))
                return ServiceResult<CategoryImageDto>.Failure("Contenu du fichier invalide.");

            var categoryExists = await _context.Categories
                                .AnyAsync(c => c.Id == categoryId);

            if (!categoryExists)
                return ServiceResult<CategoryImageDto>.Failure("Catégorie introuvable.");

            var serverFileName = $"{Guid.NewGuid():N}{extension}";

            var uploadRoot = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "categories",
                categoryId.ToString());

            Directory.CreateDirectory(uploadRoot);

            var physicalPath = Path.Combine(uploadRoot, serverFileName);

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

            try
            {
                _context.CategoryImages.Add(image);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Échec DB. Suppression du fichier {Path}.", physicalPath);
                if (File.Exists(physicalPath))
                    File.Delete(physicalPath);
                return ServiceResult<CategoryImageDto>.Failure("Erreur lors de l'enregistrement.");
            }

            _logger.LogInformation(
            "Image {ServerFileName} téléversée pour la catégorie {CategoryId}.",
            serverFileName, categoryId);

            return ServiceResult<CategoryImageDto>.Success(ToDto(image));

        }
        private static CategoryImageDto ToDto(CategoryImage image) => new()
        {
            Id = image.Id,
            CategoryId = image.CategoryId,
            Url = $"/uploads/categories/{image.CategoryId}/{image.FileName}",
            OriginalFileName = image.OriginalFileName,
            ContentType = image.ContentType,
            SizeBytes = image.SizeBytes,
            AltText = image.AltText,
            UploadedAtUtc = image.UploadedAtUtc
        };

        private static readonly Dictionary<string, byte[]> Bytes = new()
        {
            { ".jpg",  new byte[] { 0xFF, 0xD8, 0xFF } },
            { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } }, 
            { ".png",  new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
            { ".webp", new byte[] { 0x52, 0x49, 0x46, 0x46 } }
        };

        private static async Task<bool> HasValidSignatureAsync(IFormFile file, string extension)
        {
            if (!Bytes.TryGetValue(extension, out var signature)) return false;
            var buffer = new byte[signature.Length];
            await using var stream = file.OpenReadStream();
            await stream.ReadExactlyAsync(buffer);
            return buffer.SequenceEqual(signature);
        }

       
    }
}
