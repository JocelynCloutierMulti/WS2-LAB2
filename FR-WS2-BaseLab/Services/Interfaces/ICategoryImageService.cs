using FR_WS2_BaseLab.Services.DTOs;
using Microsoft.AspNetCore.Http;
 
namespace FR_WS2_BaseLab.Services.Interfaces;
 
public interface ICategoryImageService
{
    Task<List<CategoryImageDto>> GetForCategoryAsync(int categoryId);
    Task<ServiceResult<CategoryImageDto>> UploadAsync(int categoryId, IFormFile file);
    Task<ServiceResult<bool>> DeleteAsync(int imageId);
}