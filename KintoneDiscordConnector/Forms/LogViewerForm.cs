using System;
using System.IO;
using System.Windows.Forms;
using KintoneDiscordConnector.Models;

namespace KintoneDiscordConnector.Forms;

public partial class LogViewerForm : Form
{
    private TextBox txtLog;
    private Button btnRefresh;
    private Button btnClear;
    private Button btnOpenFolder;
    private ComboBox cboLogLevel;
    private Label lblStatus;

    public LogViewerForm()
    {
        InitializeComponent();
        LoadLogs();
    }

    private void InitializeComponent()
    {
        // フォーム設定
        this.Text = "ログビューアー - kintone-Discord連携";
        this.Size = new System.Drawing.Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;

        // ツールバー
        var toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40
        };
        this.Controls.Add(toolbar);

        btnRefresh = new Button
        {
            Text = "更新",
            Location = new System.Drawing.Point(10, 8),
            Size = new System.Drawing.Size(80, 25)
        };
        btnRefresh.Click += (s, e) => LoadLogs();
        toolbar.Controls.Add(btnRefresh);

        btnClear = new Button
        {
            Text = "クリア",
            Location = new System.Drawing.Point(100, 8),
            Size = new System.Drawing.Size(80, 25)
        };
        btnClear.Click += BtnClear_Click;
        toolbar.Controls.Add(btnClear);

        btnOpenFolder = new Button
        {
            Text = "フォルダを開く",
            Location = new System.Drawing.Point(190, 8),
            Size = new System.Drawing.Size(120, 25)
        };
        btnOpenFolder.Click += BtnOpenFolder_Click;
        toolbar.Controls.Add(btnOpenFolder);

        // ログレベルフィルター
        var lblFilter = new Label
        {
            Text = "フィルター:",
            Location = new System.Drawing.Point(330, 12),
            Size = new System.Drawing.Size(70, 20)
        };
        toolbar.Controls.Add(lblFilter);

        cboLogLevel = new ComboBox
        {
            Location = new System.Drawing.Point(400, 10),
            Size = new System.Drawing.Size(100, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cboLogLevel.Items.AddRange(new[] { "すべて", "Information", "Warning", "Error" });
        cboLogLevel.SelectedIndex = 0;
        cboLogLevel.SelectedIndexChanged += (s, e) => LoadLogs();
        toolbar.Controls.Add(cboLogLevel);

        // ログテキストボックス
        txtLog = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            Font = new System.Drawing.Font("Consolas", 9),
            BackColor = System.Drawing.Color.Black,
            ForeColor = System.Drawing.Color.LightGreen
        };
        this.Controls.Add(txtLog);

        // ステータスバー
        var statusBar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 25
        };
        this.Controls.Add(statusBar);

        lblStatus = new Label
        {
            Text = "準備完了",
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            Padding = new Padding(5, 0, 0, 0)
        };
        statusBar.Controls.Add(lblStatus);
    }

    private void LoadLogs()
    {
        try
        {
            var configDir = AppSettings.GetConfigDirectory();
            var logDir = Path.Combine(configDir, "logs");

            if (!Directory.Exists(logDir))
            {
                txtLog.Text = "ログファイルが見つかりません。";
                lblStatus.Text = "ログファイルなし";
                return;
            }

            // 最新のログファイルを取得
            var logFiles = Directory.GetFiles(logDir, "app-*.log");
            if (logFiles.Length == 0)
            {
                txtLog.Text = "ログファイルが見つかりません。";
                lblStatus.Text = "ログファイルなし";
                return;
            }

            // 最新のファイルを選択
            Array.Sort(logFiles);
            var latestLogFile = logFiles[logFiles.Length - 1];

            // ログファイルを読み込み
            var lines = File.ReadAllLines(latestLogFile);

            // フィルター適用
            var filter = cboLogLevel.SelectedItem?.ToString() ?? "すべて";
            var filteredLines = lines;

            if (filter != "すべて")
            {
                filteredLines = Array.FindAll(lines, line => line.Contains($"[{filter.Substring(0, 3).ToUpper()}]"));
            }

            // 最新の1000行のみ表示（パフォーマンス対策）
            var displayLines = filteredLines;
            if (filteredLines.Length > 1000)
            {
                displayLines = new string[1000];
                Array.Copy(filteredLines, filteredLines.Length - 1000, displayLines, 0, 1000);
                txtLog.Text = $"[表示制限: 最新1000行のみ表示]\r\n\r\n{string.Join("\r\n", displayLines)}";
            }
            else
            {
                txtLog.Text = string.Join("\r\n", displayLines);
            }

            // 最下部にスクロール
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();

            lblStatus.Text = $"ログファイル: {Path.GetFileName(latestLogFile)} ({filteredLines.Length} 行)";
        }
        catch (Exception ex)
        {
            txtLog.Text = $"ログの読み込み中にエラーが発生しました:\r\n{ex.Message}";
            lblStatus.Text = "エラー";
        }
    }

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "本当にログファイルをクリアしますか？\nこの操作は元に戻せません。",
            "確認",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            try
            {
                var configDir = AppSettings.GetConfigDirectory();
                var logDir = Path.Combine(configDir, "logs");

                if (Directory.Exists(logDir))
                {
                    var logFiles = Directory.GetFiles(logDir, "app-*.log");
                    foreach (var file in logFiles)
                    {
                        File.Delete(file);
                    }

                    MessageBox.Show(
                        "ログファイルをクリアしました。",
                        "成功",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    LoadLogs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"ログのクリア中にエラーが発生しました:\n{ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }

    private void BtnOpenFolder_Click(object? sender, EventArgs e)
    {
        try
        {
            var configDir = AppSettings.GetConfigDirectory();
            var logDir = Path.Combine(configDir, "logs");

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            System.Diagnostics.Process.Start("explorer.exe", logDir);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"フォルダを開く際にエラーが発生しました:\n{ex.Message}",
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
