using System.Windows.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using KintoneDiscordConnector.Models;
using KintoneDiscordConnector.Forms;
using Serilog;

namespace KintoneDiscordConnector;

public class TrayApplication : ApplicationContext
{
    private NotifyIcon? _trayIcon;
    private IHost? _webHost;
    private readonly AppSettings _settings;
    private bool _isServerRunning = false;

    public TrayApplication()
    {
        try
        {
            _settings = AppSettings.Load();
            InitializeTrayIcon();

            if (_settings.Server.AutoStart)
            {
                StartWebServer();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"アプリケーションの初期化中にエラーが発生しました:\n{ex.Message}",
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            Log.Error(ex, "アプリケーション初期化エラー");
            Application.Exit();
        }
    }

    private void InitializeTrayIcon()
    {
        _trayIcon = new NotifyIcon
        {
            Text = "kintone-Discord連携",
            Visible = true,
            ContextMenuStrip = CreateContextMenu()
        };

        // アイコンの設定（デフォルトアイコンを使用）
        try
        {
            _trayIcon.Icon = SystemIcons.Application;
        }
        catch
        {
            // アイコンの読み込みに失敗した場合はデフォルトを使用
        }

        _trayIcon.DoubleClick += OnTrayIconDoubleClick;
    }

    private ContextMenuStrip CreateContextMenu()
    {
        var menu = new ContextMenuStrip();

        var startStopItem = new ToolStripMenuItem("サーバー開始", null, OnStartStopServer);
        menu.Items.Add(startStopItem);

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("設定", null, OnSettings);
        menu.Items.Add("ログ表示", null, OnShowLogs);
        menu.Items.Add("診断", null, OnDiagnostics);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("終了", null, OnExit);

        // メニューが開かれる前にサーバー状態に応じてテキストを更新
        menu.Opening += (sender, e) =>
        {
            startStopItem.Text = _isServerRunning ? "サーバー停止" : "サーバー開始";
        };

        return menu;
    }

    private void OnTrayIconDoubleClick(object? sender, EventArgs e)
    {
        OnSettings(sender, e);
    }

    private async void OnStartStopServer(object? sender, EventArgs e)
    {
        if (_isServerRunning)
        {
            await StopWebServer();
        }
        else
        {
            StartWebServer();
        }
    }

    private void StartWebServer()
    {
        try
        {
            if (_isServerRunning)
            {
                ShowBalloonTip("サーバーは既に実行中です。", ToolTipIcon.Info);
                return;
            }

            _webHost = CreateWebHost();
            _webHost.Start();
            _isServerRunning = true;

            var url = $"http://localhost:{_settings.Server.Port}";
            ShowBalloonTip($"サーバーを開始しました\n{url}", ToolTipIcon.Info);
            Log.Information("Webサーバーを開始しました。ポート: {Port}", _settings.Server.Port);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"サーバーの起動に失敗しました:\n{ex.Message}",
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            Log.Error(ex, "Webサーバー起動エラー");
            _isServerRunning = false;
        }
    }

    private async Task StopWebServer()
    {
        try
        {
            if (_webHost != null)
            {
                await _webHost.StopAsync();
                _webHost.Dispose();
                _webHost = null;
            }

            _isServerRunning = false;
            ShowBalloonTip("サーバーを停止しました", ToolTipIcon.Info);
            Log.Information("Webサーバーを停止しました。");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Webサーバー停止エラー");
        }
    }

    private IHost CreateWebHost()
    {
        return Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls($"http://localhost:{_settings.Server.Port}");
            })
            .Build();
    }

    private void OnSettings(object? sender, EventArgs e)
    {
        try
        {
            var settingsForm = new SettingsForm(_settings, this);
            settingsForm.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"設定画面の表示中にエラーが発生しました:\n{ex.Message}",
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            Log.Error(ex, "設定画面表示エラー");
        }
    }

    private void OnShowLogs(object? sender, EventArgs e)
    {
        try
        {
            var logViewerForm = new LogViewerForm();
            logViewerForm.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"ログ画面の表示中にエラーが発生しました:\n{ex.Message}",
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            Log.Error(ex, "ログ画面表示エラー");
        }
    }

    private async void OnDiagnostics(object? sender, EventArgs e)
    {
        try
        {
            var diagnosticsForm = new DiagnosticsForm(_settings);
            diagnosticsForm.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"診断画面の表示中にエラーが発生しました:\n{ex.Message}",
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            Log.Error(ex, "診断画面表示エラー");
        }
    }

    private async void OnExit(object? sender, EventArgs e)
    {
        try
        {
            var result = MessageBox.Show(
                "アプリケーションを終了しますか？",
                "確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                await StopWebServer();

                if (_trayIcon != null)
                {
                    _trayIcon.Visible = false;
                    _trayIcon.Dispose();
                }

                Log.Information("アプリケーションを終了します。");
                Application.Exit();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "アプリケーション終了エラー");
            Application.Exit();
        }
    }

    public void ShowBalloonTip(string message, ToolTipIcon icon = ToolTipIcon.Info)
    {
        if (_settings.App.ShowNotifications && _trayIcon != null)
        {
            _trayIcon.ShowBalloonTip(3000, "kintone-Discord連携", message, icon);
        }
    }

    public async Task RestartServerAsync()
    {
        await StopWebServer();
        await Task.Delay(1000); // 1秒待機
        StartWebServer();
    }
}
