# Git ワークフロー

## ブランチ戦略

```
main   ← リリース済みの安定版。直接コミットしない
  ↑ PR（リリース時）
devel  ← 開発の統合ブランチ。ここも直接コミットしない
  ↑ PR（機能・修正ごと）
feat/xxx, fix/xxx, docs/xxx  ← 作業ブランチ
```

- **作業ブランチは必ず `devel` から切ります。**
- 通常の PR は `devel` 向け。`main` 向けの PR はリリースのタイミングだけ。

### ブランチ名

`<type>/<短い英語の説明>` の形式（kebab-case）。

| 例 | 用途 |
| --- | --- |
| `feat/stage-generator` | 新機能 |
| `fix/player-double-jump` | バグ修正 |
| `docs/claude-code-setup` | ドキュメント |
| `refactor/health-ui` | リファクタリング |
| `chore/update-packages` | 雑務・設定 |

## コミット規約

**Conventional Commits** 形式で、英語 1 行の要約を書きます。

```
<type>: <要約（英語・命令形・50 文字程度）>

（必要なら空行を挟んで日本語で詳細）
```

type は `feat` / `fix` / `docs` / `refactor` / `perf` / `chore` / `test` を使います。

```
feat: add random stage generator
fix: prevent double life decrement on player death
docs: add project documentation for Claude Code
```

> 過去のコミットは `add: ...` や `[add] ...` など形式が混在していますが、**今後は上記に統一**します。

### Unity プロジェクト特有の注意

1. **`.meta` は本体とセットでコミットする。**
   ファイルを追加・削除・移動したら、対応する `.meta` も同じコミットに含めてください。
   `.meta` だけ欠けると、他の環境で GUID が再生成されてシーン内の参照が壊れます。

   ```bash
   # .meta の付け忘れチェック（本体はあるのに .meta がないファイルを探す）
   git status --porcelain
   ```

2. **スクリプトを新規追加したら、コミット前に一度 Unity Editor を開く。**
   `.meta` は Editor が生成します。Claude Code が `.meta` を手で作ることはありません。

3. **生成物はコミットしない。** `Library/` `Temp/` `Logs/` `UserSettings/` `Build/` は `.gitignore` 済みです。

4. **シーン / プレハブのコンフリクト。**
   `.unity` や `.prefab` は YAML なので手動マージは危険です。`.gitattributes` で
   UnityYAMLMerge を指定してあります。初回だけ以下の設定をしてください（Windows / Unity 6 の例）:

   ```bash
   git config merge.unityyamlmerge.name "Unity SmartMerge"
   git config merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/6000.3.7f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"'
   git config merge.unityyamlmerge.recursive binary
   ```

   それでも解決できない場合は、片方を採用して Unity Editor 上で作業をやり直すほうが安全です。

5. **同じシーンを複数人・複数ブランチで同時に触らない。** 分割できるならプレハブに切り出してください。

## PR の手順

### 1. 作業ブランチを作る

```bash
git switch devel
git pull origin devel
git switch -c feat/your-feature
```

### 2. コミットする

作業中の無関係な変更（Unity が勝手に書き換える `PlayFabEditorPrefsSO.asset` など）を巻き込まないよう、
**`git add .` ではなくパスを指定して add** します。

```bash
git status
git add Assets/Scripts/Foo/Bar.cs Assets/Scripts/Foo/Bar.cs.meta
git commit -m "feat: add bar system"
```

### 3. push して PR を作成する

```bash
git push -u origin feat/your-feature
```

**GitHub CLI (`gh`) が入っている場合:**

```bash
gh pr create --base devel --title "feat: add bar system" --body-file .github/pull_request_template.md
```

**入っていない場合**（現状のこの環境）: push 後に表示される URL、または以下を開いて作成します。

```
https://github.com/koheix/FindingItems/compare/devel...<ブランチ名>?expand=1
```

`gh` を入れると Claude Code から PR 作成まで完結できます:

```powershell
winget install --id GitHub.cli
gh auth login
```

### 4. PR の内容

`.github/pull_request_template.md` のテンプレートに沿って、以下を必ず書きます。

- 何を変えたか / なぜ変えたか
- **Unity Editor での動作確認結果**（どのシーンで何を確認したか）
- スクリーンショット（見た目の変更がある場合）

## Claude Code に許可している git 操作

`.claude/settings.json` で以下を制御しています。

- **確認なしで許可**: `git status` / `diff` / `log` / `show` / `branch` / `switch` / `checkout -b` /
  `fetch` / `pull` / `add` / `commit`、および読み取り系コマンド（`ls`, `cat`, `grep`, `find` など）
- **毎回確認**: `git push`（許可リストに入れていません。ブランチを目視確認してから承認してください）
- **禁止 (deny)**: `git push --force` / `-f`、`git push origin main`、`git push origin devel`、
  `git reset --hard`、`git clean -f`、`git rebase`、`git filter-branch`、`rm -rf`
- **編集禁止**: `.unity` / `.prefab` / `.asset` / `.meta`（Unity Editor で扱うファイル）

個人的に緩めたい場合は `.claude/settings.local.json`（gitignore 済み）に書いてください。

Claude Code に作業を任せるときも、**マージは人間が GitHub 上で行ってください。**
