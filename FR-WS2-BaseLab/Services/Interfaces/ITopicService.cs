using FR_WS2_BaseLab.Models;

namespace FR_WS2_BaseLab.Services.Interfaces;

public interface ITopicService
{
    Task<ServiceResult<List<Topic>>> GetByCategoryIdAsync(int categoryId);
    Task<ServiceResult<Topic>> GetDetailsAsync(int id);
    Task<ServiceResult<Topic>> GetForEditAsync(int id);
    Task<ServiceResult<Topic>> CreateAsync(Topic topic, string? userId);
    Task<ServiceResult<Topic>> UpdateAsync(int id, Topic topic, string? userId, bool isAdmin);
    Task<ServiceResult<Topic>> DeleteAsync(int id, string? userId, bool isAdmin);
    Task<bool> ExistsAsync(int id);
}
