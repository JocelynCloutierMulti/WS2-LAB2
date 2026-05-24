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

    private const long MaxFileSize = 2 * 1024 * 1024;

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
            Url = $"/uploads/category-images/{image.FileName}",
            OrginalFileName = image.OriginalFileName,
            contentType = image.ContentType,
            SizeBytes = image.SizeInBytes,
            AltText = image.AltText
        };
    }
    public async Task<IReadOnlyList<CategoryImageDto>> GetForCategoryAsync(
        int categoryId)
    {
        var images = await _context.CategoryImages
            .Where(i => i.CategoryId == categoryId)
            .OrderByDescending(i => i.UploadedAtUtc)
            .Select(i => ToDto(i))
            .ToListAsync();


    }
}