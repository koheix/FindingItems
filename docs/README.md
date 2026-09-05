# ドキュメント

AIUEOWorld（`FindingItems`）の開発ドキュメントです。
Claude Code はルートの [`CLAUDE.md`](../CLAUDE.md) を起点にここを参照します。

| ドキュメント | 内容 |
| --- | --- |
| [project-overview.md](project-overview.md) | ゲーム内容・技術スタック・シーン構成・ビルド設定 |
| [unity-project-structure.md](unity-project-structure.md) | `Assets/` 以下のディレクトリ構成と、新規ファイルの置き場所 |
| [architecture.md](architecture.md) | 進行管理・プレイヤー・アイテム・セーブの設計と依存関係 |
| [coding-guidelines.md](coding-guidelines.md) | C# / Unity のコーディング規約 |
| [git-workflow.md](git-workflow.md) | ブランチ戦略・コミット規約・PR 手順・Unity 特有の git 注意点 |

## はじめて触る人へ

1. Unity Hub で **6000.3.7f1** を入れて、このリポジトリを開く
2. `Assets/Scenes/TitleScene.unity` から再生する
3. コードを触る前に [coding-guidelines.md](coding-guidelines.md) と
   [architecture.md](architecture.md) に目を通す
4. 変更は必ず作業ブランチで行い、`devel` に PR を出す → [git-workflow.md](git-workflow.md)
