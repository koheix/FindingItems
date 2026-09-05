# CLAUDE.md

このファイルは Claude Code がセッション開始時に自動で読み込むプロジェクトメモリです。
詳細なルールは `docs/` 配下に分割してあります。**作業を始める前に、関連する docs を必ず参照してください。**

## プロジェクト概要

- **名称**: AIUEOWorld (リポジトリ名: `FindingItems`)
- **エンジン**: Unity 6000.3.7f1 (Unity 6.3) / Universal Render Pipeline
- **ジャンル**: 3D サードパーソン アクション（アイテム収集 + プラットフォーム）
- **ターゲット**: WebGL / モバイル (iOS・Android) / PC
- **バックエンド**: PlayFab（ログイン・セーブデータ）
- **主要パッケージ**: Input System (New), Cinemachine, ML-Agents, ProBuilder, AI Navigation, TextMesh Pro

詳細は [docs/project-overview.md](docs/project-overview.md) を参照。

## ドキュメント索引

| ドキュメント | 内容 |
| --- | --- |
| [docs/project-overview.md](docs/project-overview.md) | ゲーム内容・技術スタック・シーン構成・ビルド設定 |
| [docs/unity-project-structure.md](docs/unity-project-structure.md) | `Assets/` 以下のディレクトリ構成と配置ルール |
| [docs/architecture.md](docs/architecture.md) | 主要システム（進行管理・プレイヤー・アイテム・セーブ）の設計 |
| [docs/coding-guidelines.md](docs/coding-guidelines.md) | C# / Unity のコーディング規約 |
| [docs/testing.md](docs/testing.md) | テスト方針・単体テストの書き方と実行手順・結果の出力 |
| [docs/git-workflow.md](docs/git-workflow.md) | ブランチ戦略・コミット規約・PR 手順 |

## 作業の基本ルール

1. **ブランチ**: `main` および `devel` に直接コミットしない。必ず作業ブランチを切り、PR を作成する。詳細は [docs/git-workflow.md](docs/git-workflow.md)。
2. **テスト**: **PR を作る前に必ず単体テストを実行し、結果を PR に貼る。** 手順は `unity-test` スキル
   （`.claude/skills/unity-test/SKILL.md`）と [docs/testing.md](docs/testing.md) にある。
   テスト未実行の `gh pr create` は PreToolUse フックが拒否する。拒否されたら回避せずテストを実行すること。
3. **配置**: 新規ファイルは [docs/unity-project-structure.md](docs/unity-project-structure.md) の構成に従って置く。既存フォルダに該当するものがあれば新設しない。
4. **既存コードに合わせる**: 命名・コメント量・書き方は周囲のコードに合わせる。大規模なリファクタリングは依頼されたときだけ行う。
5. **コメント言語**: コード内のコメント・XML ドキュメントコメントは**日本語**。識別子（クラス名・変数名）は英語。

## Unity 固有の注意（重要）

1. **`.meta` ファイルを手で作らない**
   `.meta` は Unity Editor が生成します。ターミナルから `.meta` を新規作成・偽装しないこと。
   スクリプトを新規追加したら「Unity Editor を開いて `.meta` を生成させ、その後コミットする」ようユーザーに伝える。
2. **`.meta` と本体はセットでコミットする**
   ファイルの追加・削除・リネーム時は、対応する `.meta` も同じコミットに含める。`.meta` の GUID が変わると参照が壊れる。
3. **シーン / プレハブ (`.unity`, `.prefab`) をテキスト編集しない**
   YAML なので読むのは可。ただし手編集は破損リスクが高いので、変更は Unity Editor 上で行う。
   どうしても差分を確認したいときは `git diff -- '*.unity'` で読むだけにとどめる。
4. **コンパイル確認は Editor 側で行う**
   このリポジトリに CLI ビルドの仕組みはありません。コード変更後は「Unity Editor でコンパイルエラーがないか確認してください」とユーザーに依頼する。
   Unity MCP（`com.coplaydev.unity-mcp`）が有効な環境では、その MCP ツール経由で
   コンパイル状況の確認や Editor 操作ができます。使えるなら優先して使ってください。
5. **`Library/`, `Temp/`, `Logs/`, `UserSettings/` は生成物**。読むのは可、コミットは不可（`.gitignore` 済み）。

## Claude Code の権限設定

`.claude/settings.json` に、読み取り系コマンドと安全な git 操作の許可リストを定義しています。
`git push --force` / `git reset --hard` / `main` への直接 push は deny 済みです。
個人用の上書きは `.claude/settings.local.json`（gitignore 済み）に書いてください。

## よく使うカスタムコマンド

- `/commit` — 変更内容を確認し、Conventional Commits 形式でコミットする
- `/pr` — 単体テストを実行してから push し、PR を作成する（または PR 作成 URL を提示する）

## スキルとフック

| 種類 | 場所 | 役割 |
| --- | --- | --- |
| スキル `unity-test` | `.claude/skills/unity-test/SKILL.md` | 単体テストの実行手順とレポート形式 |
| PreToolUse フック | `.claude/hooks/require-test-report.ps1` | テスト未実行・失敗・古い場合に `gh pr create` を拒否する |
