---
description: 現在のブランチを push して PR を作成する（または作成 URL を提示する）
---

現在のブランチの変更を PR にしてください。手順:

1. `git status` でコミット漏れがないか確認する。未コミットの変更があれば先に知らせる。
2. `git log devel..HEAD --oneline` で、この PR に含まれるコミットを確認する。
3. ブランチが未 push なら `git push -u origin <branch>` を実行する
   （`main` / `devel` への push は禁止設定です）。
4. PR を作成する:
   - `gh` が使える場合:
     `gh pr create --base devel --title "<Conventional Commits 形式のタイトル>" --body "<本文>"`
   - `gh` が無い場合: 以下の URL をユーザーに提示する。
     `https://github.com/koheix/FindingItems/compare/devel...<branch>?expand=1`
     あわせて、そのまま貼り付けられる PR 本文（Markdown）を出力する。
5. PR 本文は `.github/pull_request_template.md` の項目に沿って書く。
   **Unity Editor での動作確認は人間が行う必要がある**ので、確認してほしい項目を
   チェックリストとして具体的に列挙すること（例: 「RandomStage シーンで再生し、
   生成された足場をジャンプで渡れるか」）。

$ARGUMENTS
