---
description: 変更内容を確認して Conventional Commits 形式でコミットする
---

現在の変更をコミットしてください。手順:

1. `git status` と `git diff` で変更内容を確認する。
2. **今回の作業と無関係な変更を巻き込まない**。特に以下は自動生成・別作業の差分なので、
   明示的に指示されない限りステージしないこと:
   - `Assets/PlayFabEditorExtensions/Editor/Resources/PlayFabEditorPrefsSO.asset`
   - `UserSettings/`, `Library/`, `Temp/`, `Logs/`（gitignore 済みだが念のため）
3. `git add .` は使わず、**パスを明示して `git add`** する。
4. ファイルを追加・削除・移動している場合、対応する `.meta` が同じコミットに含まれているか確認する。
   `.meta` が無い新規ファイルがあれば、コミットせずに「Unity Editor を一度開いて `.meta` を
   生成してください」とユーザーに伝える。
5. `docs/git-workflow.md` の規約に従い、Conventional Commits 形式（英語 1 行）でコミットする。
   例: `feat: add random stage generator`
6. 現在のブランチが `main` または `devel` なら、コミットせずに作業ブランチの作成を提案する。

$ARGUMENTS
