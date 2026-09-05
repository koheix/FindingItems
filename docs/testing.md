# テスト方針

**このプロジェクトでは、PR を作成・更新する前に単体テストを実行し、結果を PR 上に出力することを必須とします。**

## なぜ必須にするか

Unity の変更はコンパイルが通っても壊れていることが多く、レビュー時に差分だけを見ても
「本当に動くのか」が判断できません。そこで、

1. ロジックは自動テストで担保する
2. 自動テストで担保できない部分（見た目・実機のタップ・Play モードでの通し）は、
   **人間が確認する項目としてレビュー者に明示する**

という切り分けをします。

## 何をどうテストするか

| 種類 | 置き場所 | 対象 |
| --- | --- | --- |
| EditMode テスト | `Assets/Tests/Editor/` | プレーンな C# のロジック（状態遷移・計算・判定） |
| 人間による確認 | PR のチェックリスト | 見た目・入力・実機・Play モードでの通し |

`Assets/Tests/Editor/` は `Editor` という名前のフォルダなので Editor 専用アセンブリに入り、
プレイヤービルドには含まれません。

### MonoBehaviour をテストしやすくする

MonoBehaviour は `Awake` / `Start` / `Update` が Unity 側から呼ばれるため、そのままでは単体テストしづらいです。
**判断ロジックをプレーンな C# クラスに切り出し、MonoBehaviour には UI・入力・時間制御だけを残します。**

実例:

| クラス | 役割 | テスト |
| --- | --- | --- |
| `TutorialProgress` | どのメッセージを出すか、いつ次へ進むか | `TutorialProgressTests`（EditMode） |
| `TutorialManager` | UI 生成・`Time.timeScale`・カーソル制御 | Play モードで人間が確認 |

### テストの書き方

- ファイル名: `<対象クラス>Tests.cs`
- メソッド名: `対象_条件_期待結果`（英語）
- `Assert` の第2引数に**日本語で意図**を書く。レビュー時にそのまま読めるようにするため

```csharp
[Test]
public void ConfirmCurrentStep_WithTrigger_WaitsForTrigger()
{
    ...
    Assert.IsFalse(progress.IsShowingMessage, "UI を閉じてプレイヤーに操作させる");
}
```

## テストの実行

### Unity Editor から

メニュー `Tools > Tests > Run EditMode Tests`

`Window > General > Test Runner` の Run All でも実行できますが、レポートは生成されません。

### Claude Code から（MCP for Unity 経由）

```
mcp__UnityMCP__execute_code:  EditorTestReporter.RunEditModeTests();
```

テストは非同期に走ります。数秒待ってから `TestResults/edit-mode-report.md` を読んでください。
`EditorTestReporter` は実行前に前回のレポートを削除するので、
**ファイルが再生成されていない = まだ終わっていない、または実行できていない**と判断できます。

## 結果の出力

`TestResults/edit-mode-report.md`（生成物・gitignore 済み）に Markdown で出力されます。
**この中身をそのまま PR 本文、または `gh pr comment` に貼り付けてください。**

レポートには以下が含まれます。

- 実行日時 / Unity バージョン / 総数 / 成功 / 失敗 / スキップ / 所要時間 / 判定（PASSED or FAILED）
- テストケース一覧（折りたたみ）
- 失敗があればメッセージとスタックトレース

PR には、これとは**別に**「人間に確認してほしいこと」をチェックリストで書きます。
自動テストの結果と人間の確認項目を混ぜて書かないこと。

## 強制の仕組み

| 仕組み | 場所 | 役割 |
| --- | --- | --- |
| スキル `unity-test` | `.claude/skills/unity-test/SKILL.md` | 手順とレポート形式を Claude に読ませる |
| PreToolUse フック | `.claude/hooks/require-test-report.ps1` | 条件を満たさない `gh pr create` を拒否する |
| `/pr` コマンド | `.claude/commands/pr.md` | PR 作成手順にテスト実行を組み込む |

フックは `gh pr create` を検知したとき、以下のいずれかに該当すると**終了コード 2 で拒否**します。

- `TestResults/edit-mode-report.md` が無い
- レポートに `**FAILED**` が含まれる
- レポートより後に `Assets/Scripts/` または `Assets/Tests/` の `.cs` が変更されている（テストが古い）

フックはあくまで最後の関門です。**テストを書かずにレポートだけ通しても意味がありません。**
