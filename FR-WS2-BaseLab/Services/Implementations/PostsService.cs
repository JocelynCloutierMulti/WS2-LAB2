using FR_WS2_BaseLab.Models;

using FR_WS2_BaseLab.Services.Interfaces;

using Microsoft.EntityFrameworkCore;



namespace FR_WS2_BaseLab.Services.Implementations;



public class PostsService : IPosts

{

    private readonly FrWs2BaselabContext _context;

    private readonly ILogger<PostsService> _logger;



    public PostsService(

        FrWs2BaselabContext context,

        ILogger<PostsService> logger)

    {

        _context = context;

        _logger = logger;

    }

    public async Task<ServiceResult<Post>> CreateAsync(Post post, string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))

        {

            return ServiceResult<Post>.Failure("Vous devez être connecté.");

        }



        try

        {

            post.UserId = userId;

            post.Date = DateTime.Now;

            post.Inactive = false;

             _context.Posts.Add(post);

            await _context.SaveChangesAsync();



            return ServiceResult<Post>.Success(post);

        }

        catch (DbUpdateException ex)

        {

            _logger.LogError(ex, "Erreur BD lors de la création d'un message.");

            return ServiceResult<Post>.Failure("Le message n'a pas pu être créé.");

        }
    }

    public  async Task<ServiceResult<Post>> DeleteAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post is null)
        {
            return ServiceResult<Post>.Failure("Le sujet est introuvable.");
        }
        try
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return ServiceResult<Post>.Success(post);
        }

        catch (DbUpdateException ex)
        {
            _logger.LogError(ex,
                "Erreur BD lors de la suppression du message {PostId}.", id);
             return ServiceResult<Post>.Failure(
             "Le message ne peut pas être supprimé.");

        }
    }

    public Task<bool> ExistsAsync(int id)
    {
        throw new NotImplementedException();
    }

    public  async Task<ServiceResult<List<Post>>> GetByTopicIdAsync(int topicId)
    {
        try 
        {
            var posts = await _context.Posts
                .AsNoTracking()
                .Include(p => p.User)
                .Where(p => p.TopId == topicId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
                return ServiceResult<List<Post>>.Success(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erreur lors du chargement des messages du sujet {TopicId}.",
                topicId);
                return ServiceResult<List<Post>>.Failure(

                "Les messages n'ont pas pu être chargés.");
        }
    }

    public  async Task<ServiceResult<Post>> GetDetailsAsync(int id)
    {
        try
        {
            var post = await _context.Posts
                .AsNoTracking()
                .Include(p => p.Top)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post is null)
            {
                return ServiceResult<Post>.Failure("Le message est introuvable.");
            }

            return ServiceResult<Post>.Success(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erreur lors du chargement du message {PostId}.", id);

            return ServiceResult<Post>.Failure(
                "Le message n'a pas pu être chargé.");
        }
    }

    public Task<ServiceResult<Post>> GetForEditAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<Post>> UpdateAsync(int id, Post post)
    {
        if (id != post.Id)
        {
            return ServiceResult<Post>.Failure("L'identifiant reçu est invalide.");
        }

        var existingPost = await _context.Posts.FindAsync(id);

        if (existingPost is null)
        {
            return ServiceResult<Post>.Failure("Le sujet est introuvable.");
        }

        try
        {
            existingPost.Texte = post.Texte;
            existingPost.Inactive = post.Inactive;

            await _context.SaveChangesAsync();
            return ServiceResult<Post>.Success(post);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex,
                "Erreur BD lors de la modification du message {PostId}.", id);
            return ServiceResult<Post>.Failure("Le message n'a pas pu être modifié.");
        }
    }



    

}
