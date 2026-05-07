using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;

namespace FR_WS2_BaseLab.Services.Implementations;
public class PostService : IPostService
{
    private readonly FrWs2BaselabContext _context;
    private readonly ILogger<PostService> _logger;

    public PostService(
        FrWs2BaselabContext context,
        ILogger<PostService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<ServiceResult<Post>> CreateAsync(Post post, string? userId)
    {
        throw new NotImplementedException();
    }

    public Task<Services.ServiceResult<Post>> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<List<Post>>> GetByTopicIdAsync(int topicId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<Post>> GetDetailsAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<Post>> GetForEditAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Services.ServiceResult<Post>> UpdateAsync(int id, Post post)
    {
        throw new NotImplementedException();
    }
}