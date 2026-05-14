using FR_WS2_BaseLab.Models;

namespace FR_WS2_BaseLab.Services.Interfaces;

public interface ITopicService
{
    Task<ServiceResult<List<Topic>>> GetByCategoryIdAsync(int categoryId);
    Task<ServiceResult<Topic>> GetDetailsAsync(int id);
    Task<ServiceResult<Topic>> GetForEditAsync(int id);
    Task<ServiceResult<Topic>> CreateAsync(Topic topic, string? userId);
    Task<ServiceResult<Topic>> UpdateAsync(int id, Topic topic, bool autorise);
    Task<ServiceResult<Topic>> DeleteAsync(int id);
    Task IncrementViewsAsync(int topicId);
    Task<bool> ExistsAsync(int id);
}

