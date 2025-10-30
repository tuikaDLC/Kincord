using System;
using System.Windows.Forms;
using Microsoft.Win32;
using KintoneDiscordConnector.Models;

namespace KintoneDiscordConnector.Forms;

public partial class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly TrayApplication _trayApp;

    private TextBox txtKintoneToken = null!;
    private TextBox txtKintoneSubdomain = null!;
    private TextBox txtDiscordWebhook = null!;
    private TextBox txtDiscordUsername = null!;
    private NumericUpDown numPort = null!;
    private CheckBox chkStartWithWindows = null!;
    private CheckBox chkMinimizeToTray = null!;
    private CheckBox chkShowNotifications = null!;
    private CheckBox chkAutoStart = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;
    private Button btnTest = null!;

    public SettingsForm(AppSettings settings, TrayApplication trayApp)
    {
        _settings = settings;
        _trayApp = trayApp;
        InitializeComponent();
        LoadSettings();
    }

    private void InitializeComponent()
    {
        // フォーム設定
        this.Text = "設定 - kintone-Discord連携";
        this.Size = new System.Drawing.Size(500, 550);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;

        int yPos = 20;
        int labelWidth = 150;
        int controlWidth = 300;

        // kintone設定グループ
        var grpKintone = new GroupBox
        {
            Text = "kintone設定",
            Location = new System.Drawing.Point(20, yPos),
            Size = new System.Drawing.Size(450, 120)
        };
        this.Controls.Add(grpKintone);

        // kintone Webhookトークン
        var lblKintoneToken = new Label
        {
            Text = "Webhookトークン:",
            Location = new System.Drawing.Point(10, 25),
            Size = new System.Drawing.Size(labelWidth, 20)
        };
        grpKintone.Controls.Add(lblKintoneToken);

        txtKintoneToken = new TextBox
        {
            Location = new System.Drawing.Point(10, 45),
            Size = new System.Drawing.Size(controlWidth, 20),
            PasswordChar = '*'
        };
        grpKintone.Controls.Add(txtKintoneToken);

        // kintone サブドメイン
        var lblSubdomain = new Label
        {
            Text = "サブドメイン:",
            Location = new System.Drawing.Point(10, 75),
            Size = new System.Drawing.Size(labelWidth, 20)
        };
        grpKintone.Controls.Add(lblSubdomain);

        txtKintoneSubdomain = new TextBox
        {
            Location = new System.Drawing.Point(10, 95),
            Size = new System.Drawing.Size(200, 20),
            PlaceholderText = "example"
        };
        grpKintone.Controls.Add(txtKintoneSubdomain);

        yPos += 130;

        // Discord設定グループ
        var grpDiscord = new GroupBox
        {
            Text = "Discord設定",
            Location = new System.Drawing.Point(20, yPos),
            Size = new System.Drawing.Size(450, 120)
        };
        this.Controls.Add(grpDiscord);

        // Discord Webhook URL
        var lblDiscordWebhook = new Label
        {
            Text = "Webhook URL:",
            Location = new System.Drawing.Point(10, 25),
            Size = new System.Drawing.Size(labelWidth, 20)
        };
        grpDiscord.Controls.Add(lblDiscordWebhook);

        txtDiscordWebhook = new TextBox
        {
            Location = new System.Drawing.Point(10, 45),
            Size = new System.Drawing.Size(controlWidth, 20)
        };
        grpDiscord.Controls.Add(txtDiscordWebhook);

        // Discord ユーザー名
        var lblDiscordUsername = new Label
        {
            Text = "ボット名:",
            Location = new System.Drawing.Point(10, 75),
            Size = new System.Drawing.Size(labelWidth, 20)
        };
        grpDiscord.Controls.Add(lblDiscordUsername);

        txtDiscordUsername = new TextBox
        {
            Location = new System.Drawing.Point(10, 95),
            Size = new System.Drawing.Size(200, 20),
            Text = "kintone Bot"
        };
        grpDiscord.Controls.Add(txtDiscordUsername);

        yPos += 130;

        // サーバー設定グループ
        var grpServer = new GroupBox
        {
            Text = "サーバー設定",
            Location = new System.Drawing.Point(20, yPos),
            Size = new System.Drawing.Size(450, 80)
        };
        this.Controls.Add(grpServer);

        // ポート番号
        var lblPort = new Label
        {
            Text = "ポート番号:",
            Location = new System.Drawing.Point(10, 25),
            Size = new System.Drawing.Size(labelWidth, 20)
        };
        grpServer.Controls.Add(lblPort);

        numPort = new NumericUpDown
        {
            Location = new System.Drawing.Point(10, 45),
            Size = new System.Drawing.Size(100, 20),
            Minimum = 1024,
            Maximum = 65535,
            Value = 3000
        };
        grpServer.Controls.Add(numPort);

        // 自動起動
        chkAutoStart = new CheckBox
        {
            Text = "サーバー自動起動",
            Location = new System.Drawing.Point(130, 47),
            Size = new System.Drawing.Size(150, 20)
        };
        grpServer.Controls.Add(chkAutoStart);

        yPos += 90;

        // アプリケーション設定グループ
        var grpApp = new GroupBox
        {
            Text = "アプリケーション設定",
            Location = new System.Drawing.Point(20, yPos),
            Size = new System.Drawing.Size(450, 100)
        };
        this.Controls.Add(grpApp);

        chkStartWithWindows = new CheckBox
        {
            Text = "Windows起動時に自動起動",
            Location = new System.Drawing.Point(10, 25),
            Size = new System.Drawing.Size(250, 20)
        };
        grpApp.Controls.Add(chkStartWithWindows);

        chkMinimizeToTray = new CheckBox
        {
            Text = "最小化時にトレイに格納",
            Location = new System.Drawing.Point(10, 50),
            Size = new System.Drawing.Size(250, 20)
        };
        grpApp.Controls.Add(chkMinimizeToTray);

        chkShowNotifications = new CheckBox
        {
            Text = "バルーン通知を表示",
            Location = new System.Drawing.Point(10, 75),
            Size = new System.Drawing.Size(250, 20)
        };
        grpApp.Controls.Add(chkShowNotifications);

        yPos += 110;

        // ボタン
        btnTest = new Button
        {
            Text = "接続テスト",
            Location = new System.Drawing.Point(20, yPos),
            Size = new System.Drawing.Size(100, 30)
        };
        btnTest.Click += BtnTest_Click;
        this.Controls.Add(btnTest);

        btnSave = new Button
        {
            Text = "保存",
            Location = new System.Drawing.Point(270, yPos),
            Size = new System.Drawing.Size(90, 30)
        };
        btnSave.Click += BtnSave_Click;
        this.Controls.Add(btnSave);

        btnCancel = new Button
        {
            Text = "キャンセル",
            Location = new System.Drawing.Point(370, yPos),
            Size = new System.Drawing.Size(90, 30)
        };
        btnCancel.Click += (s, e) => this.Close();
        this.Controls.Add(btnCancel);
    }

    private void LoadSettings()
    {
        txtKintoneToken.Text = _settings.Kintone.WebhookToken;
        txtKintoneSubdomain.Text = _settings.Kintone.Subdomain;
        txtDiscordWebhook.Text = _settings.Discord.WebhookUrl;
        txtDiscordUsername.Text = _settings.Discord.Username;
        numPort.Value = _settings.Server.Port;
        chkAutoStart.Checked = _settings.Server.AutoStart;
        chkStartWithWindows.Checked = _settings.App.StartWithWindows;
        chkMinimizeToTray.Checked = _settings.App.MinimizeToTray;
        chkShowNotifications.Checked = _settings.App.ShowNotifications;
    }

    private async void BtnTest_Click(object? sender, EventArgs e)
    {
        btnTest.Enabled = false;
        btnTest.Text = "テスト中...";

        try
        {
            // 一時的に設定を保存
            var tempSettings = new AppSettings
            {
                Discord = new DiscordSettings
                {
                    WebhookUrl = txtDiscordWebhook.Text,
                    Username = txtDiscordUsername.Text
                }
            };

            using var httpClient = new HttpClient();
            var discordService = new Services.DiscordService(
                httpClient,
                tempSettings,
                Microsoft.Extensions.Logging.Abstractions.NullLogger<Services.DiscordService>.Instance);

            var result = await discordService.TestWebhookAsync();

            if (result)
            {
                MessageBox.Show(
                    "Discord Webhookへの接続テストに成功しました！",
                    "成功",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    "Discord Webhookへの接続テストに失敗しました。\nWebhook URLを確認してください。",
                    "失敗",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"接続テスト中にエラーが発生しました:\n{ex.Message}",
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            btnTest.Enabled = true;
            btnTest.Text = "接続テスト";
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            // 設定を更新
            _settings.Kintone.WebhookToken = txtKintoneToken.Text;
            _settings.Kintone.Subdomain = txtKintoneSubdomain.Text;
            _settings.Discord.WebhookUrl = txtDiscordWebhook.Text;
            _settings.Discord.Username = txtDiscordUsername.Text;
            _settings.Server.Port = (int)numPort.Value;
            _settings.Server.AutoStart = chkAutoStart.Checked;
            _settings.App.StartWithWindows = chkStartWithWindows.Checked;
            _settings.App.MinimizeToTray = chkMinimizeToTray.Checked;
            _settings.App.ShowNotifications = chkShowNotifications.Checked;

            // Windows自動起動の設定
            SetStartup(_settings.App.StartWithWindows);

            // 設定を保存
            _settings.Save();

            MessageBox.Show(
                "設定を保存しました。\n\nサーバー設定を変更した場合は、サーバーを再起動してください。",
                "成功",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"設定の保存中にエラーが発生しました:\n{ex.Message}",
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void SetStartup(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (key == null)
                return;

            if (enable)
            {
                key.SetValue("KintoneDiscordConnector", Application.ExecutablePath);
            }
            else
            {
                key.DeleteValue("KintoneDiscordConnector", false);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"自動起動の設定中にエラーが発生しました:\n{ex.Message}",
                "警告",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
