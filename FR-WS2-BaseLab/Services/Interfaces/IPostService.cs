using FR_WS2_BaseLab.Models;

namespace FR_WS2_BaseLab.Services.Interfaces;

public interface IPostService
{
    Task<ServiceResult<List<Post>>> GetByTopicIdAsync(int topicId);
    Task<ServiceResult<Post>> GetDetailsAsync(int id);
    Task<ServiceResult<Post>> GetForEditAsync(int id);
    Task<ServiceResult<Post>> CreateAsync(Post post, string? userId);
    Task<ServiceResult<Post>> UpdateAsync(int id, Post post, string? userId, bool isAdmin);
    Task<ServiceResult<Post>> DeleteAsync(int id, string? userId, bool isAdmin);
    Task<bool> ExistsAsync(int id);
}
