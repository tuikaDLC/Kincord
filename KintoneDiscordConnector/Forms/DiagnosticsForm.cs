using System;
using System.Windows.Forms;
using KintoneDiscordConnector.Models;
using KintoneDiscordConnector.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace KintoneDiscordConnector.Forms;

public partial class DiagnosticsForm : Form
{
    private readonly AppSettings _settings;
    private TextBox txtResults = null!;
    private Button btnRunDiagnostics = null!;
    private Button btnClose = null!;
    private ProgressBar progressBar = null!;
    private Label lblStatus = null!;

    public DiagnosticsForm(AppSettings settings)
    {
        _settings = settings;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // フォーム設定
        this.Text = "システム診断 - kintone-Discord連携";
        this.Size = new System.Drawing.Size(600, 500);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;

        // タイトルラベル
        var lblTitle = new Label
        {
            Text = "システム診断ツール",
            Location = new System.Drawing.Point(20, 20),
            Size = new System.Drawing.Size(560, 25),
            Font = new System.Drawing.Font("メイリオ", 12, System.Drawing.FontStyle.Bold)
        };
        this.Controls.Add(lblTitle);

        var lblDescription = new Label
        {
            Text = "アプリケーションの動作状況を診断します。問題がある場合は詳細を確認してください。",
            Location = new System.Drawing.Point(20, 50),
            Size = new System.Drawing.Size(560, 40)
        };
        this.Controls.Add(lblDescription);

        // 診断結果テキストボックス
        txtResults = new TextBox
        {
            Location = new System.Drawing.Point(20, 100),
            Size = new System.Drawing.Size(560, 280),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            Font = new System.Drawing.Font("Consolas", 9)
        };
        this.Controls.Add(txtResults);

        // プログレスバー
        progressBar = new ProgressBar
        {
            Location = new System.Drawing.Point(20, 390),
            Size = new System.Drawing.Size(560, 20),
            Style = ProgressBarStyle.Marquee,
            Visible = false
        };
        this.Controls.Add(progressBar);

        // ステータスラベル
        lblStatus = new Label
        {
            Text = "",
            Location = new System.Drawing.Point(20, 415),
            Size = new System.Drawing.Size(560, 20)
        };
        this.Controls.Add(lblStatus);

        // ボタン
        btnRunDiagnostics = new Button
        {
            Text = "診断実行",
            Location = new System.Drawing.Point(380, 420),
            Size = new System.Drawing.Size(100, 30)
        };
        btnRunDiagnostics.Click += BtnRunDiagnostics_Click;
        this.Controls.Add(btnRunDiagnostics);

        btnClose = new Button
        {
            Text = "閉じる",
            Location = new System.Drawing.Point(490, 420),
            Size = new System.Drawing.Size(90, 30)
        };
        btnClose.Click += (s, e) => this.Close();
        this.Controls.Add(btnClose);
    }

    private async void BtnRunDiagnostics_Click(object? sender, EventArgs e)
    {
        btnRunDiagnostics.Enabled = false;
        progressBar.Visible = true;
        lblStatus.Text = "診断を実行中...";
        txtResults.Text = "診断を開始しています...\r\n\r\n";

        try
        {
            // DiagnosticServiceを作成
            using var httpClient = new HttpClient();
            var discordService = new DiscordService(
                httpClient,
                _settings,
                NullLogger<DiscordService>.Instance);

            var diagnosticService = new DiagnosticService(
                _settings,
                discordService,
                NullLogger<DiagnosticService>.Instance);

            // 診断実行
            var report = await diagnosticService.RunDiagnosticsAsync();

            // 結果を表示
            DisplayReport(report);

            lblStatus.Text = report.IsHealthy ? "診断完了: 問題なし" : "診断完了: 問題が検出されました";
        }
        catch (Exception ex)
        {
            txtResults.Text = $"診断中にエラーが発生しました:\r\n{ex.Message}\r\n\r\n{ex.StackTrace}";
            lblStatus.Text = "エラーが発生しました";
        }
        finally
        {
            progressBar.Visible = false;
            btnRunDiagnostics.Enabled = true;
        }
    }

    private void DisplayReport(DiagnosticReport report)
    {
        var result = "";

        result += "=== システム診断結果 ===\r\n";
        result += $"実行日時: {DateTime.Now:yyyy/MM/dd HH:mm:ss}\r\n\r\n";

        // 総合結果
        result += "【総合結果】\r\n";
        result += report.IsHealthy
            ? "✓ システムは正常に動作しています\r\n\r\n"
            : "✗ いくつかの問題が検出されました\r\n\r\n";

        // 詳細チェック
        result += "【詳細チェック】\r\n";
        result += $"{(report.PortAvailable ? "✓" : "✗")} ポート{_settings.Server.Port}の可用性: {(report.PortAvailable ? "利用可能" : "使用中")}\r\n";
        result += $"{(report.DiscordWebhookValid ? "✓" : "✗")} Discord Webhook: {(report.DiscordWebhookValid ? "接続成功" : "接続失敗")}\r\n";

        if (!string.IsNullOrEmpty(_settings.Kintone.Subdomain))
        {
            result += $"{(report.KintoneReachable ? "✓" : "✗")} kintone接続: {(report.KintoneReachable ? "到達可能" : "到達不可")}\r\n";
        }

        result += $"\r\nファイアウォール: {report.FirewallStatus}\r\n";

        // エラー
        if (report.Errors.Count > 0)
        {
            result += "\r\n【エラー】\r\n";
            foreach (var error in report.Errors)
            {
                result += $"✗ {error}\r\n";
            }
        }

        // 警告
        if (report.Warnings.Count > 0)
        {
            result += "\r\n【警告】\r\n";
            foreach (var warning in report.Warnings)
            {
                result += $"⚠ {warning}\r\n";
            }
        }

        // 推奨アクション
        result += "\r\n【推奨アクション】\r\n";
        if (!report.PortAvailable)
        {
            result += $"- ポート{_settings.Server.Port}が使用中です。設定で別のポートに変更するか、使用中のアプリケーションを終了してください。\r\n";
        }

        if (!report.DiscordWebhookValid)
        {
            result += "- Discord Webhook URLを確認してください。正しいURLを設定画面で入力してください。\r\n";
        }

        if (string.IsNullOrEmpty(_settings.Kintone.WebhookToken))
        {
            result += "- kintone Webhookトークンが設定されていません。セキュリティのため設定することを推奨します。\r\n";
        }

        if (!report.IsHealthy)
        {
            result += "- 問題を修正後、再度診断を実行してください。\r\n";
        }

        txtResults.Text = result;
    }
}
