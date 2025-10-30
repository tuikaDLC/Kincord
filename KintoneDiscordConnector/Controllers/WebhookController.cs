using Microsoft.AspNetCore.Mvc;
using KintoneDiscordConnector.Models;
using KintoneDiscordConnector.Services;

namespace KintoneDiscordConnector.Controllers;

[ApiController]
[Route("webhook")]
public class WebhookController : ControllerBase
{
    private readonly IDiscordService _discordService;
    private readonly ILogger<WebhookController> _logger;
    private readonly AppSettings _settings;
    private readonly Action<string>? _onNotificationSent;

    public WebhookController(
        IDiscordService discordService,
        ILogger<WebhookController> logger,
        AppSettings settings)
    {
        _discordService = discordService;
        _logger = logger;
        _settings = settings;
    }

    public void SetNotificationCallback(Action<string> callback)
    {
        _onNotificationSent = callback;
    }

    [HttpPost("kintone")]
    public async Task<IActionResult> ReceiveKintoneWebhook(
        [FromBody] KintoneWebhookPayload payload,
        [FromHeader(Name = "X-Cybozu-Webhook-Token")] string? token)
    {
        try
        {
            _logger.LogInformation("kintone Webhookを受信しました。Type: {Type}, App: {AppId}",
                payload.Type, payload.App.Id);

            // トークン検証
            if (!ValidateToken(token))
            {
                _logger.LogWarning("無効なWebhookトークンです。");
                return Unauthorized(new { error = "Invalid webhook token" });
            }

            // Discord通知送信
            await _discordService.SendNotificationAsync(payload);

            // トレイアイコンにバルーン通知を送る
            var message = $"レコード{GetEventTypeJapanese(payload.Type)}の通知を送信しました";
            _onNotificationSent?.Invoke(message);

            _logger.LogInformation("Webhook処理が完了しました。");

            return Ok(new { success = true, message = "Notification sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook処理中にエラーが発生しました。");
            return StatusCode(500, new { error = "Internal server error", detail = ex.Message });
        }
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    private bool ValidateToken(string? token)
    {
        if (string.IsNullOrEmpty(_settings.Kintone.WebhookToken))
        {
            _logger.LogWarning("Webhookトークンが設定されていません。検証をスキップします。");
            return true; // トークンが設定されていない場合は検証しない
        }

        return token == _settings.Kintone.WebhookToken;
    }

    private string GetEventTypeJapanese(string eventType)
    {
        return eventType switch
        {
            "ADD_RECORD" => "作成",
            "UPDATE_RECORD" => "更新",
            "DELETE_RECORD" => "削除",
            _ => "変更"
        };
    }
}
