namespace FR_WS2_BaseLab.Services.Interfaces;

public interface IForumEmailService
{
    Task NotifyTopicAuthorOfNewPostAsync(int topicId, string? postAuthorId, string topicUrl);
}
