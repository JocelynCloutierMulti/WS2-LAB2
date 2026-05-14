using FR_WS2_BaseLab.Models;

namespace FR_WS2_BaseLab.Services.Interfaces;

public interface ICategoryService
{
    Task<ServiceResult<List<Category>>> GetAllAsync(bool includeTopics = false);
    Task<ServiceResult<Category>> GetDetailsAsync(int id);
    Task<ServiceResult<Category>> GetForEditAsync(int id);
    Task<ServiceResult<Category>> CreateAsync(Category category);
    Task<ServiceResult<Category>> UpdateAsync(int id, Category category);
    Task<ServiceResult<Category>> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
