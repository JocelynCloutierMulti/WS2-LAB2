using System.Text.Encodings.Web;
using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace FR_WS2_BaseLab.Services.Implementations;

public class ForumEmailService : IForumEmailService
{
    private readonly FrWs2BaselabContext _context;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ForumEmailService> _logger;

    public ForumEmailService(
        FrWs2BaselabContext context,
        IEmailSender emailSender,
        ILogger<ForumEmailService> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task NotifyTopicAuthorOfNewPostAsync(int topicId, string? postAuthorId, string topicUrl)
    {
        try
        {
            var topic = await _context.Topics
                .AsNoTracking()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == topicId);

            if (topic?.User is null ||
                string.IsNullOrWhiteSpace(topic.User.Email) ||
                topic.UserId == postAuthorId)
            {
                return;
            }

            var encodedTitle = HtmlEncoder.Default.Encode(topic.Title);
            var encodedUrl = HtmlEncoder.Default.Encode(topicUrl);
            var body = $"""
                <p>Bonjour,</p>
                <p>Un nouveau message a ete ajoute a votre sujet <strong>{encodedTitle}</strong>.</p>
                <p><a href="{encodedUrl}">Voir le sujet</a></p>
                """;

            await _emailSender.SendEmailAsync(
                topic.User.Email,
                $"Nouveau message sur votre sujet : {topic.Title}",
                body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Le courriel de notification du sujet {TopicId} n'a pas pu etre envoye.", topicId);
        }
    }
}
