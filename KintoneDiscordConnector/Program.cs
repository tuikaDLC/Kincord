using System;
using System.IO;
using System.Windows.Forms;
using Serilog;
using KintoneDiscordConnector.Models;

namespace KintoneDiscordConnector;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            // ロギングの設定
            ConfigureLogging();

            Log.Information("=== kintone-Discord連携アプリケーション起動 ===");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            var app = new TrayApplication();
            Application.Run(app);

            Log.Information("=== kintone-Discord連携アプリケーション終了 ===");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "アプリケーションの起動中に致命的なエラーが発生しました。");

            MessageBox.Show(
                $"アプリケーションの起動中にエラーが発生しました:\n{ex.Message}",
                "致命的なエラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureLogging()
    {
        try
        {
            var configDir = AppSettings.GetConfigDirectory();
            var logDir = Path.Combine(configDir, "logs");

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            var logFilePath = Path.Combine(logDir, "app-.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message}{NewLine}{Exception}")
                .WriteTo.Console()
                .CreateLogger();
        }
        catch (Exception ex)
        {
            // ロギング設定失敗時は最低限のコンソール出力
            Console.WriteLine($"ロギング設定エラー: {ex.Message}");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}
