---
name: unity-test
description: Unity の単体テスト（EditMode）を実行し、レビュー者向けのテスト結果レポートを作る。PR を作成・更新する前に必ず使う。テストの追加、Test Runner の実行、テスト結果の出力、PR 前の検証を行うときに使用する。
---

# Unity 単体テストと結果レポート

**このプロジェクトでは、PR を作成・更新する前に必ず単体テストを実行し、結果を PR 上に出力する。**
テスト未実行での `gh pr create` は PreToolUse フック（`.claude/hooks/require-test-report.ps1`）が拒否する。

## 手順

### 1. 変更に対応するテストを書く / 更新する

- テストの置き場所: `Assets/Tests/Editor/`（`Editor` フォルダなので Editor 専用アセンブリに入り、ビルドには含まれない）
- ファイル名は `<対象クラス>Tests.cs`
- テストメソッド名は `対象_条件_期待結果` の英語（例: `Begin_WithEmptySteps_FinishesImmediately`）
- `Assert` の第2引数に**日本語で意図**を書く。レビュー時にそのまま読めるようにするため
- MonoBehaviour は Unity のライフサイクルに依存して単体テストしづらい。
  **判断ロジックはプレーンな C# クラスに切り出し、MonoBehaviour 側は UI・入力・時間制御だけにする。**
  例: `TutorialProgress`（ロジック）と `TutorialManager`（Unity 連携）

### 2. テストを実行する

**MCP for Unity が使える場合（推奨）**

```
mcp__UnityMCP__execute_code:  EditorTestReporter.RunEditModeTests();
```

テストは非同期で走るので、数秒待ってからレポートを読む:

```
mcp__UnityMCP__read_console      # コンパイルエラー・実行ログの確認
cat TestResults/edit-mode-report.md
```

`EditorTestReporter` は実行前に前回のレポートを消すので、
**ファイルが再生成されていない = まだ終わっていない or 実行できていない**と判断してよい。

**MCP が使えない場合**

Unity Editor のメニュー `Tools > Tests > Run EditMode Tests` を実行してもらう
（`Window > General > Test Runner` から Run All でも可だが、その場合レポートは生成されない）。
どちらも人手が要るので、ユーザーに依頼して結果を待つこと。**勝手に「テスト済み」と書かない。**

### 3. 結果を確認する

- `TestResults/edit-mode-report.md` を読む（生成物なので gitignore 済み。コミットしない）
- **1件でも FAILED があれば PR を作らない。** 直してから再実行する
- コンパイルエラーがあるとテストは走らない。`read_console` で 0 error を確認する

### 4. レビュー者に見える形で出力する

レポートの内容を**そのまま PR 本文に貼る**。新規 PR なら `gh pr create --body-file`、
既存 PR の更新なら `gh pr comment` を使う。

PR には最低限これを含める:

- `TestResults/edit-mode-report.md` の中身（成功/失敗の表 + テストケース一覧）
- **自動テストで担保できていない項目**を明示したチェックリスト
  （実機のタップ、見た目、Play モードでの通し確認など、人間が確認するもの）

自動テストの結果と、人間に確認してほしいことを混ぜて書かない。分けて書く。

## 書いてはいけないこと

- 実行していないテストの結果を書く
- 「テストは通るはず」「問題ないと思われる」といった推測をテスト結果として書く
- 失敗したテストを隠す、あるいは失敗したまま PR を作る

テストが実行できなかった場合は、**その理由をそのままユーザーに伝えて指示を仰ぐ**。
