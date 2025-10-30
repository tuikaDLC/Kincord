using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using KintoneDiscordConnector.Models;

namespace KintoneDiscordConnector.Services;

public class DiagnosticService
{
    private readonly AppSettings _settings;
    private readonly IDiscordService _discordService;
    private readonly ILogger<DiagnosticService> _logger;

    public DiagnosticService(
        AppSettings settings,
        IDiscordService discordService,
        ILogger<DiagnosticService> logger)
    {
        _settings = settings;
        _discordService = discordService;
        _logger = logger;
    }

    public async Task<DiagnosticReport> RunDiagnosticsAsync()
    {
        var report = new DiagnosticReport();

        try
        {
            // ポート確認
            _logger.LogInformation("ポート {Port} の可用性を確認中...", _settings.Server.Port);
            report.PortAvailable = CheckPortAvailable(_settings.Server.Port);
            if (!report.PortAvailable)
            {
                report.Errors.Add($"ポート {_settings.Server.Port} は既に使用されています。");
            }

            // Discord Webhook テスト
            _logger.LogInformation("Discord Webhookのテスト中...");
            if (!string.IsNullOrEmpty(_settings.Discord.WebhookUrl))
            {
                report.DiscordWebhookValid = await TestDiscordWebhookAsync();
                if (!report.DiscordWebhookValid)
                {
                    report.Errors.Add("Discord Webhookへの接続に失敗しました。URLを確認してください。");
                }
            }
            else
            {
                report.Warnings.Add("Discord Webhook URLが設定されていません。");
                report.DiscordWebhookValid = false;
            }

            // kintone接続テスト（オプション）
            if (!string.IsNullOrEmpty(_settings.Kintone.Subdomain))
            {
                _logger.LogInformation("kintone接続のテスト中...");
                report.KintoneReachable = await TestKintoneConnectionAsync();
                if (!report.KintoneReachable)
                {
                    report.Warnings.Add("kintoneサブドメインへの接続を確認できませんでした。");
                }
            }

            // ファイアウォール状態
            _logger.LogInformation("ファイアウォール状態の確認中...");
            report.FirewallStatus = CheckFirewallStatus();

            _logger.LogInformation("診断完了。エラー: {ErrorCount}, 警告: {WarningCount}",
                report.Errors.Count, report.Warnings.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "診断中にエラーが発生しました。");
            report.Errors.Add($"診断エラー: {ex.Message}");
        }

        return report;
    }

    private bool CheckPortAvailable(int port)
    {
        try
        {
            var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    private async Task<bool> TestDiscordWebhookAsync()
    {
        try
        {
            return await _discordService.TestWebhookAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Discord Webhookテスト中にエラーが発生しました。");
            return false;
        }
    }

    private async Task<bool> TestKintoneConnectionAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.Kintone.Subdomain))
            {
                return false;
            }

            var url = $"https://{_settings.Kintone.Subdomain}.cybozu.com";
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await httpClient.GetAsync(url);
            return response.StatusCode == HttpStatusCode.OK ||
                   response.StatusCode == HttpStatusCode.Redirect ||
                   response.StatusCode == HttpStatusCode.MovedPermanently;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "kintone接続テスト中にエラーが発生しました。");
            return false;
        }
    }

    private string CheckFirewallStatus()
    {
        try
        {
            // Windows環境でのファイアウォール状態確認は複雑なため、簡易的なメッセージを返す
            return "ファイアウォールの設定を確認してください。";
        }
        catch
        {
            return "ファイアウォール状態を確認できませんでした。";
        }
    }

    public int FindAvailablePort(int startPort = 3000)
    {
        for (int port = startPort; port <= startPort + 100; port++)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return port;
            }
            catch (SocketException)
            {
                continue;
            }
        }
        throw new Exception("利用可能なポートが見つかりません（3000-3100の範囲で検索）");
    }
}
