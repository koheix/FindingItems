---
description: 単体テストを実行してから、現在のブランチを push して PR を作成する
---

現在のブランチの変更を PR にしてください。手順:

1. `git status` でコミット漏れがないか確認する。未コミットの変更があれば先に知らせる。
2. `git log devel..HEAD --oneline` で、この PR に含まれるコミットを確認する。
3. **単体テストを実行する。** `unity-test` スキル（`.claude/skills/unity-test/SKILL.md`）に従うこと。
   - 変更に対応するテストが `Assets/Tests/Editor/` にあるか確認し、無ければ書く
   - `EditorTestReporter.RunEditModeTests();` を MCP for Unity 経由で実行する
   - `TestResults/edit-mode-report.md` が **PASSED** であることを確認する
   - 失敗していたら PR を作らずに直す
   - MCP が使えないときはユーザーに Editor での実行を依頼し、**結果が出るまで PR を作らない**
4. ブランチが未 push なら `git push -u origin <branch>` を実行する
   （`main` / `devel` への push は禁止設定です）。
5. PR を作成する:
   - `gh` が使える場合:
     `gh pr create --base devel --title "<Conventional Commits 形式のタイトル>" --body-file <本文のファイル>`
   - `gh` が無い場合: 以下の URL をユーザーに提示する。
     `https://github.com/koheix/FindingItems/compare/devel...<branch>?expand=1`
     あわせて、そのまま貼り付けられる PR 本文（Markdown）を出力する。
6. PR 本文は `.github/pull_request_template.md` の項目に沿って書き、必ず次の 2 つを分けて含める:
   - **自動テストの結果**: `TestResults/edit-mode-report.md` の中身をそのまま貼る
   - **人間に確認してほしいこと**: 自動テストで担保できない項目をチェックリストで具体的に列挙する
     （例: 「Stage1_1 を再生し、OK ボタンをマウスでクリックできるか」「実機でタップできるか」）

`gh pr create` は PreToolUse フックでチェックされます。テスト未実行だと拒否されるので、
拒否されたら**回避しようとせず**、手順 3 に戻ってテストを実行してください。

$ARGUMENTS
