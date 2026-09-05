# PR 作成前に単体テストの実行を強制する PreToolUse フック。
#
# `gh pr create` を実行しようとしたとき、TestResults/edit-mode-report.md が
#   - 存在しない
#   - FAILED を含む
#   - 変更したソースより古い
# のいずれかなら、終了コード 2 で拒否して理由を stderr に出す。
#
# 詳しい手順は .claude/skills/unity-test/SKILL.md を参照。

$ErrorActionPreference = 'Stop'

# Windows PowerShell 5.1 の既定は OEM コードページなので、日本語が化けないよう UTF-8 にする
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$raw = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($raw)) { exit 0 }

try { $payload = $raw | ConvertFrom-Json } catch { exit 0 }

$command = ''
if ($payload.tool_input -and $payload.tool_input.command) {
    $command = [string]$payload.tool_input.command
}
if ([string]::IsNullOrWhiteSpace($command)) { exit 0 }

# PR を新規作成するときだけチェックする（view / list / diff などは素通し）。
# コマンドの先頭か区切り文字の直後だけを見る。そうしないと、コミットメッセージ本文に
# 「gh pr create」という文字列が入っているだけで誤検知する。
if ($command -notmatch '(?:^|[;&|]\s*)\s*gh(?:\.exe)?\s+pr\s+create') { exit 0 }

$projectDir = $env:CLAUDE_PROJECT_DIR
if ([string]::IsNullOrWhiteSpace($projectDir)) { $projectDir = (Get-Location).Path }

$reportPath = Join-Path $projectDir 'TestResults\edit-mode-report.md'
$skillPath  = '.claude/skills/unity-test/SKILL.md'

function Deny([string]$reason) {
    [Console]::Error.WriteLine("[PR ブロック] $reason")
    [Console]::Error.WriteLine("")
    [Console]::Error.WriteLine("このプロジェクトでは PR 作成前の単体テストが必須です。")
    [Console]::Error.WriteLine("unity-test スキル ($skillPath) に従って以下を行ってください:")
    [Console]::Error.WriteLine("  1. 変更に対応するテストを Assets/Tests/Editor/ に用意する")
    [Console]::Error.WriteLine("  2. MCP for Unity で EditorTestReporter.RunEditModeTests(); を実行する")
    [Console]::Error.WriteLine("     (MCP が使えないときはユーザーに Tools > Tests > Run EditMode Tests を依頼する)")
    [Console]::Error.WriteLine("  3. TestResults/edit-mode-report.md が PASSED であることを確認する")
    [Console]::Error.WriteLine("  4. レポートの内容を PR 本文に貼り付けてから PR を作成する")
    exit 2
}

if (-not (Test-Path $reportPath)) {
    Deny "テスト結果レポートがありません: TestResults/edit-mode-report.md"
}

$report = Get-Content -LiteralPath $reportPath -Raw -Encoding UTF8

if ($report -match '\*\*FAILED\*\*') {
    Deny "テストが失敗しています。修正して再実行してください。"
}
if ($report -notmatch '\*\*PASSED\*\*') {
    Deny "レポートに判定 (PASSED) が見つかりません。レポートが壊れている可能性があります。"
}

# レポートより後に触られたソースがあれば、テストは古い
$reportTime = (Get-Item -LiteralPath $reportPath).LastWriteTimeUtc
$sourceDirs = @('Assets\Scripts', 'Assets\Tests') |
    ForEach-Object { Join-Path $projectDir $_ } |
    Where-Object { Test-Path $_ }

$stale = $sourceDirs |
    ForEach-Object { Get-ChildItem -LiteralPath $_ -Recurse -Filter *.cs -File } |
    Where-Object { $_.LastWriteTimeUtc -gt $reportTime } |
    Select-Object -First 1

if ($stale) {
    $name = $stale.FullName.Substring($projectDir.Length).TrimStart('\', '/')
    Deny "テスト実行後に $name が変更されています。テストを再実行してください。"
}

exit 0
