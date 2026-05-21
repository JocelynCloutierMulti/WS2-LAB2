using FR_WS2_BaseLab.Models;

namespace FR_WS2_BaseLab.Services.Interfaces;

public interface ICategoryImageService
{
    Task<ServiceResult<List<CategoryImage>>> GetImagesByCategoryAsync(int categoryId);
    Task<ServiceResult<CategoryImage>> CreateAsync(Category category, IFormFile imageFile);
    Task<ServiceResult<CategoryImage>> SetMainImageAsync(int categoryId, int imageId);
    Task<ServiceResult<CategoryImage>> DeleteAsync(int id);
    Task<ServiceResult<CategoryImage>> DeleteAllAsync(int categoryId);
}