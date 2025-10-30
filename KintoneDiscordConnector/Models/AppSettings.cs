using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace KintoneDiscordConnector.Models;

public class AppSettings
{
    public ServerSettings Server { get; set; } = new();
    public KintoneSettings Kintone { get; set; } = new();
    public DiscordSettings Discord { get; set; } = new();
    public AppConfig App { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();

    private static string ConfigDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "KintoneDiscordConnector");

    private static string ConfigFilePath => Path.Combine(ConfigDirectory, "config.json");

    public static AppSettings Load()
    {
        try
        {
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }

            if (!File.Exists(ConfigFilePath))
            {
                var defaultSettings = new AppSettings();
                defaultSettings.Save();
                return defaultSettings;
            }

            var json = File.ReadAllText(ConfigFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();

            // 暗号化されたトークンを復号化
            if (!string.IsNullOrEmpty(settings.Kintone.WebhookToken))
            {
                try
                {
                    settings.Kintone.WebhookToken = CredentialManager.Decrypt(settings.Kintone.WebhookToken);
                }
                catch
                {
                    // 復号化失敗（平文の場合など）はそのまま使用
                }
            }

            return settings;
        }
        catch (Exception ex)
        {
            throw new Exception($"設定ファイルの読み込みに失敗しました: {ex.Message}", ex);
        }
    }

    public void Save()
    {
        try
        {
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }

            // トークンを暗号化してから保存
            var settingsToSave = (AppSettings)this.MemberwiseClone();
            settingsToSave.Server = this.Server;
            settingsToSave.Kintone = new KintoneSettings
            {
                WebhookToken = string.IsNullOrEmpty(this.Kintone.WebhookToken)
                    ? ""
                    : CredentialManager.Encrypt(this.Kintone.WebhookToken),
                Subdomain = this.Kintone.Subdomain
            };
            settingsToSave.Discord = this.Discord;
            settingsToSave.App = this.App;
            settingsToSave.Logging = this.Logging;

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(settingsToSave, options);
            File.WriteAllText(ConfigFilePath, json);
        }
        catch (Exception ex)
        {
            throw new Exception($"設定ファイルの保存に失敗しました: {ex.Message}", ex);
        }
    }

    public static string GetConfigDirectory() => ConfigDirectory;
}

public class ServerSettings
{
    public int Port { get; set; } = 3000;
    public bool AutoStart { get; set; } = true;
}

public class KintoneSettings
{
    public string WebhookToken { get; set; } = "";
    public string Subdomain { get; set; } = "";
}

public class DiscordSettings
{
    public string WebhookUrl { get; set; } = "";
    public string Username { get; set; } = "kintone Bot";
    public string AvatarUrl { get; set; } = "";
}

public class AppConfig
{
    public bool StartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public bool ShowNotifications { get; set; } = true;
}

public class LoggingSettings
{
    public string Level { get; set; } = "Information";
    public string FilePath { get; set; } = "%APPDATA%\\KintoneDiscordConnector\\logs\\app.log";
}

// 暗号化・復号化ユーティリティ
public static class CredentialManager
{
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        try
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch
        {
            return plainText; // 暗号化失敗時は平文を返す
        }
    }

    public static string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return encryptedText;

        try
        {
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            return encryptedText; // 復号化失敗時は平文として返す
        }
    }
}
