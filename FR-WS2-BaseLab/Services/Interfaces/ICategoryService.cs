using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services;
 
namespace FR_WS2_BaseLab.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<ServiceResult<List<Category>>> GetAllAsync();
        Task<ServiceResult<Category>> GetByIdAsync(int id);
        Task<ServiceResult> CreateAsync(Category category);
        Task<ServiceResult> UpdateAsync(int id, Category category);
        Task<ServiceResult> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}