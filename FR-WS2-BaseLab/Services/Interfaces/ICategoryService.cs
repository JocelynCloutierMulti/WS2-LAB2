using FR_WS2_BaseLab.Models;

namespace FR_WS2_BaseLab.Services.Interfaces;

public interface ICategoryService

{

    Task<ServiceResult<List<Category>>> GetAllAsync();
    Task<ServiceResult<Category>> GetByIdAsync(int id);
    Task<ServiceResult<Category>> CreateAsync(Category category);
    Task<ServiceResult<Category>> UpdateAsync(int id, Category category);
    Task<ServiceResult<Category>> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);

}

