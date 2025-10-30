namespace KintoneDiscordConnector.Models;

public class DiagnosticReport
{
    public bool PortAvailable { get; set; }
    public bool KintoneReachable { get; set; }
    public bool DiscordWebhookValid { get; set; }
    public string FirewallStatus { get; set; } = "";
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public bool IsHealthy => PortAvailable && DiscordWebhookValid && Errors.Count == 0;

    public string GetSummary()
    {
        if (IsHealthy)
            return "すべての診断項目が正常です。";

        var summary = "以下の問題が検出されました:\n";
        foreach (var error in Errors)
        {
            summary += $"- {error}\n";
        }

        if (Warnings.Count > 0)
        {
            summary += "\n警告:\n";
            foreach (var warning in Warnings)
            {
                summary += $"- {warning}\n";
            }
        }

        return summary;
    }
}
