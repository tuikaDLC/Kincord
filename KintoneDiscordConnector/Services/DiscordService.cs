using System.Text;
using System.Text.Json;
using KintoneDiscordConnector.Models;
using Polly;
using Polly.Retry;

namespace KintoneDiscordConnector.Services;

public class DiscordService : IDiscordService
{
    private readonly HttpClient _httpClient;
    private readonly AppSettings _settings;
    private readonly ILogger<DiscordService> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public DiscordService(HttpClient httpClient, AppSettings settings, ILogger<DiscordService> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;

        // リトライポリシーの設定
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Discord送信リトライ {RetryCount} 回目（{Delay}秒後）。ステータス: {StatusCode}",
                        retryCount, timespan.TotalSeconds, outcome.Result?.StatusCode);
                });
    }

    public async Task SendNotificationAsync(KintoneWebhookPayload payload)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.Discord.WebhookUrl))
            {
                throw new InvalidOperationException("Discord Webhook URLが設定されていません。");
            }

            var embed = CreateEmbed(payload);
            var discordPayload = new
            {
                username = _settings.Discord.Username,
                avatar_url = string.IsNullOrEmpty(_settings.Discord.AvatarUrl) ? null : _settings.Discord.AvatarUrl,
                embeds = new[] { embed }
            };

            var json = JsonSerializer.Serialize(discordPayload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Discord通知を送信します。Type: {Type}, App: {AppName}, Record: {RecordId}",
                payload.Type, payload.App.Name, payload.Record.Id);

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.PostAsync(_settings.Discord.WebhookUrl, content));

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Discord送信失敗: {response.StatusCode}, Body: {errorBody}");
            }

            _logger.LogInformation("Discord通知の送信に成功しました。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Discord通知の送信中にエラーが発生しました。");
            throw;
        }
    }

    public async Task<bool> TestWebhookAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.Discord.WebhookUrl))
            {
                return false;
            }

            var testPayload = new
            {
                username = _settings.Discord.Username,
                content = "接続テストメッセージ - kintone-Discord連携アプリケーションが正常に動作しています。"
            };

            var json = JsonSerializer.Serialize(testPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_settings.Discord.WebhookUrl, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Discord Webhookテスト中にエラーが発生しました。");
            return false;
        }
    }

    private object CreateEmbed(KintoneWebhookPayload payload)
    {
        var color = GetEventColor(payload.Type);
        var title = GetEventTitle(payload.Type);

        var fields = new List<object>
        {
            new { name = "レコード番号", value = payload.Record.Id, inline = true }
        };

        // 更新者情報
        if (!string.IsNullOrEmpty(payload.Record.Modifier.Name))
        {
            fields.Add(new { name = "更新者", value = payload.Record.Modifier.Name, inline = true });
        }

        // 作成者情報
        if (!string.IsNullOrEmpty(payload.Record.Creator.Name) && payload.Type == "ADD_RECORD")
        {
            fields.Add(new { name = "作成者", value = payload.Record.Creator.Name, inline = true });
        }

        return new
        {
            title = title,
            description = $"アプリ「{payload.App.Name}」で更新がありました",
            url = payload.Url,
            color = color,
            fields = fields.ToArray(),
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            footer = new
            {
                text = "kintone-Discord連携"
            }
        };
    }

    private int GetEventColor(string eventType)
    {
        return eventType switch
        {
            "ADD_RECORD" => 0x00FF00,      // 緑: 新規作成
            "UPDATE_RECORD" => 0xFFA500,   // オレンジ: 更新
            "DELETE_RECORD" => 0xFF0000,   // 赤: 削除
            _ => 0x0099FF                   // 青: その他
        };
    }

    private string GetEventTitle(string eventType)
    {
        return eventType switch
        {
            "ADD_RECORD" => "レコードが作成されました",
            "UPDATE_RECORD" => "レコードが更新されました",
            "DELETE_RECORD" => "レコードが削除されました",
            _ => "レコードイベント"
        };
    }
}
