@echo off
echo ========================================
echo kintone-Discord Connector Build Script
echo ========================================
echo.

REM プロジェクトディレクトリに移動
cd /d "%~dp0"

echo [1/4] 依存関係の復元中...
dotnet restore KintoneDiscordConnector\KintoneDiscordConnector.csproj
if errorlevel 1 goto error

echo.
echo [2/4] ビルド中...
dotnet build KintoneDiscordConnector\KintoneDiscordConnector.csproj -c Release
if errorlevel 1 goto error

echo.
echo [3/4] 発行中（単一EXEファイル）...
dotnet publish KintoneDiscordConnector\KintoneDiscordConnector.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
if errorlevel 1 goto error

echo.
echo [4/4] 完了！
echo.
echo ビルドされたファイル: .\publish\KintoneDiscordConnector.exe
echo.
pause
exit /b 0

:error
echo.
echo エラーが発生しました。
pause
exit /b 1
