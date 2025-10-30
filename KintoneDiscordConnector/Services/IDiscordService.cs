using KintoneDiscordConnector.Models;

namespace KintoneDiscordConnector.Services;

public interface IDiscordService
{
    Task SendNotificationAsync(KintoneWebhookPayload payload);
    Task<bool> TestWebhookAsync();
}
