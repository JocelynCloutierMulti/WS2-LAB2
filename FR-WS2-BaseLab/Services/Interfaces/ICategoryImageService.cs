using FR_WS2_BaseLab.Models.ViewModels;

namespace FR_WS2_BaseLab.Services.Interfaces
{
    public interface ICategoryImageService
    {
        Task<IReadOnlyList<CategoryImageDto>> GetForCategoryAsync(int categoryId);
        Task<ServiceResult<CategoryImageDto>> UploadAsync(int categoryId, IFormFile file, string? altText);
        Task<ServiceResult<bool>> DeleteAsync(int imageId, int categoryId);
    }
}
