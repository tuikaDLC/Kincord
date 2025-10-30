# kintone-Discord連携 Windows常駐アプリケーション

kintoneのWebhookを受信してDiscordに通知を送信する、Windows常駐型アプリケーションです。

## 特徴

- **Windows常駐**: システムトレイに常駐し、バックグラウンドで動作
- **シンプルな設定**: GUIで簡単に設定可能
- **セキュア**: Webhookトークンは暗号化して保存
- **診断機能**: 接続テストや診断ツールを搭載
- **ログ管理**: 詳細なログ記録とビューアー機能

## システム要件

- Windows 10/11 (64bit)
- .NET 8.0 Runtime (アプリに同梱)

## インストール

### 方法1: ビルド済みバイナリ（推奨）

1. [Releases](https://github.com/tuikaDLC/Kincord/releases)から最新版をダウンロード
2. ZIPファイルを解凍
3. `KintoneDiscordConnector.exe` を実行

### 方法2: ソースからビルド

```bash
# リポジトリをクローン
git clone https://github.com/tuikaDLC/Kincord.git
cd Kincord/KintoneDiscordConnector

# ビルド
dotnet build -c Release

# または、単一EXEファイルとして発行
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

ビルドされたファイルは `bin/Release/net8.0-windows/win-x64/publish/` に出力されます。

## 初期設定

1. アプリケーションを起動すると、システムトレイにアイコンが表示されます
2. トレイアイコンを右クリック → **設定** を選択
3. 必要な情報を入力:

### kintone設定

- **Webhookトークン**: kintoneアプリの設定で生成されたWebhookトークン
- **サブドメイン**: kintoneのサブドメイン（例: `example` → `example.cybozu.com`）

### Discord設定

- **Webhook URL**: DiscordチャンネルのWebhook URL
  - Discordの設定 → 連携サービス → ウェブフックから作成
- **ボット名**: Discord通知時に表示される名前（デフォルト: `kintone Bot`）

### サーバー設定

- **ポート番号**: Webhookを受信するポート（デフォルト: `3000`）
- **サーバー自動起動**: アプリ起動時に自動でサーバーを開始

### アプリケーション設定

- **Windows起動時に自動起動**: Windowsログイン時に自動で起動
- **最小化時にトレイに格納**: ウィンドウを最小化するとトレイに格納
- **バルーン通知を表示**: Webhook受信時に通知を表示

4. **保存** をクリック

## kintoneの設定

1. kintoneアプリの設定画面を開く
2. **設定** → **Webhook** → **追加**
3. Webhook URLに以下を入力:
   ```
   http://localhost:3000/webhook/kintone
   ```
   ※ 別のPCから受信する場合は、`localhost` をPCのIPアドレスに変更

4. トークンを生成し、アプリの設定画面に入力
5. 通知するイベントを選択（例: レコード追加、更新、削除）
6. 保存

## 使い方

### サーバーの開始/停止

- トレイアイコンを右クリック → **サーバー開始** / **サーバー停止**

### ログの確認

- トレイアイコンを右クリック → **ログ表示**
- ログファイルの場所: `%APPDATA%\KintoneDiscordConnector\logs\`

### 診断

- トレイアイコンを右クリック → **診断**
- 接続状態やポートの可用性をチェック

### 終了

- トレイアイコンを右クリック → **終了**

## Webhook URLの例

### ローカル環境
```
http://localhost:3000/webhook/kintone
```

### 同一ネットワーク内の別PC
```
http://192.168.1.100:3000/webhook/kintone
```

### インターネット経由（ngrokなど使用）
```
https://your-domain.ngrok.io/webhook/kintone
```

## トラブルシューティング

### Webhookが受信できない

1. **診断ツールを実行**: トレイメニュー → 診断
2. **ポートの確認**:
   - 他のアプリが同じポートを使用していないか確認
   - Windowsファイアウォールで許可されているか確認
3. **kintoneの設定を確認**:
   - Webhook URLが正しいか
   - トークンが一致しているか

### Discord通知が送信されない

1. **Discord Webhook URLを確認**: 設定画面で正しいURLを入力
2. **接続テストを実行**: 設定画面 → 接続テスト
3. **ログを確認**: トレイメニュー → ログ表示

### アプリが起動しない

1. ログファイルを確認: `%APPDATA%\KintoneDiscordConnector\logs\`
2. .NET 8.0 Runtimeがインストールされているか確認
3. 管理者権限で実行してみる

## 設定ファイルの場所

- 設定ファイル: `%APPDATA%\KintoneDiscordConnector\config.json`
- ログファイル: `%APPDATA%\KintoneDiscordConnector\logs\`

設定ファイルは手動で編集も可能ですが、GUIからの設定を推奨します。

## 開発

### 技術スタック

- .NET 8.0
- ASP.NET Core (Webhookサーバー)
- Windows Forms (GUI)
- Serilog (ロギング)
- Polly (リトライポリシー)

### プロジェクト構造

```
KintoneDiscordConnector/
├── Controllers/          # Webhookコントローラー
├── Forms/                # GUIフォーム
├── Models/               # データモデル
├── Services/             # ビジネスロジック
├── Program.cs            # エントリーポイント
├── Startup.cs            # DIコンテナ設定
└── TrayApplication.cs    # トレイアプリケーション
```

### ビルドコマンド

```bash
# デバッグビルド
dotnet build

# リリースビルド
dotnet build -c Release

# 単一EXEファイルとして発行
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## ライセンス

MIT License

## サポート

問題が発生した場合は、[Issues](https://github.com/tuikaDLC/Kincord/issues)で報告してください。

## 貢献

プルリクエストを歓迎します！

1. このリポジトリをフォーク
2. 機能ブランチを作成 (`git checkout -b feature/amazing-feature`)
3. 変更をコミット (`git commit -m 'Add amazing feature'`)
4. ブランチをプッシュ (`git push origin feature/amazing-feature`)
5. プルリクエストを作成

## 更新履歴

### v1.0.0 (2025-10-30)
- 初回リリース
- kintone Webhook受信機能
- Discord通知送信機能
- GUIベースの設定画面
- ログビューアー
- システム診断ツール
