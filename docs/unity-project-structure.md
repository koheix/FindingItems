# ディレクトリ構成と配置ルール

このリポジトリのフォルダ構成と、新しいファイルをどこに置くかのルールです。
アセットやスクリプトを追加・移動するときは、必ずこの構成に従ってください。

## リポジトリルート

```
AIUEOWorld/
├─ Assets/            # Unity のアセット本体（下記参照）
├─ Packages/          # UPM のパッケージ定義（manifest.json / packages-lock.json）
├─ ProjectSettings/   # プロジェクト設定（Build Settings、Player Settings など）
├─ docs/              # プロジェクトドキュメント（このフォルダ）
├─ .claude/           # Claude Code の設定・カスタムコマンド
├─ .github/           # PR テンプレートなど GitHub 用の設定
├─ CLAUDE.md          # Claude Code が最初に読むプロジェクトメモリ
├─ .gitignore / .gitattributes
├─ Library/ Temp/ Logs/ UserSettings/   # 生成物（git 管理外）
└─ .plastic/ ignore.conf                # Plastic SCM 時代の名残（現在は未使用）
```

## `Assets/` の構成

```
Assets/
├─ animator/                  # AnimatorController
├─ GenAISprites/              # 生成 AI で作ったスプライト素材
├─ InputSystem_Actions.inputactions   # Input System のアクション定義
├─ Joystick Pack/             # モバイル操作用ジョイスティック（サードパーティ）
├─ Kenney_assets/             # Kenney の 3D モデル・テクスチャ（サードパーティ）
├─ ML-Agents/                 # ML-Agents の学習データ・タイマー出力
├─ PlayFabEditorExtensions/   # PlayFab のエディタ拡張（自動生成）
├─ PlayFabSDK/                # PlayFab SDK 本体（サードパーティ）
├─ Prefabs/                   # 自作プレハブ
│   ├─ DamageObjects/         # トラップ・攻撃判定を持つオブジェクト
│   ├─ Items/                 # 取得アイテム（回復・鍵・ジュエルなど）
│   ├─ PlayerStatusUI/        # HP などのステータス表示 UI
│   └─ Stages/                # ステージ構成パーツ（足場・橋など）
├─ Resources/                 # スクリプトから動的ロードするアセット
├─ Scenes/                    # ゲームシーン
├─ Scripts/                   # 自作スクリプト（C#）
├─ Settings/                  # URP 設定・ビルドプロファイル
├─ StarterAssets/             # Unity 公式のサードパーソンテンプレート（サードパーティ）
├─ Tests/                     # 単体テスト
│   └─ Editor/                # EditMode テストとテスト実行ユーティリティ
├─ TextMesh Pro/              # TMP のフォント・マテリアル
└─ TutorialInfo/ Readme.asset # Unity テンプレートの残骸
```

## `Assets/Scripts/` の構成

スクリプトは**機能ごとのフォルダ**に置きます。フォルダ名は `〜Scripts` で統一します。

| フォルダ | 置くもの |
| --- | --- |
| `BlockScripts/` | ブロック・ギミック（`MovingBlock`, `DamageBlock`） |
| `CharacterScripts/` | プレイヤー・敵の挙動（`PlayerController`, `PlayerHealth` ほか） |
| `CharacterUIScripts/` | キャラクターに紐づく UI（`HealthUI`） |
| `ControlUIScripts/` | 操作 UI（`JumpButtonUI`, `TouchLookZone`） |
| `GameMasterScripts/` | ゲーム進行・状態遷移（`GameMaster`） |
| `GameoverScripts/` | リザルト系シーンの演出（`SceneChanger`） |
| `ItemScripts/` | アイテムと取得処理（`CollectibleItem` ほか） |
| `RollerBallTrainingScripts/` | ML-Agents チュートリアル用 |
| `StageGeneratorScripts/` | ステージ自動生成（`StageGenerator`） |
| `TitleSceneScripts/` | タイトル画面・PlayFab（`PlayFabManager`） |
| `TutorialScripts/` | チュートリアルの進行とメッセージ UI（`TutorialManager`） |
| `TrainingScripts/` | ML-Agents・AI 挙動（`NaviAgent`, `SimulationPlayer`） |
| `UtilScripts/` | 共通ユーティリティ・データクラス（`SaveData`） |

> `Assets/Scripts/EnemyAI.cs` だけフォルダ直下にあります。整理するなら
> `CharacterScripts/` へ移動するのが構成上は自然です（`.meta` ごと移動すること）。

## 新しいファイルをどこに置くか

| 追加するもの | 置き場所 |
| --- | --- |
| 自作スクリプト | `Assets/Scripts/<機能>Scripts/` （該当がなければ新設し、この表に追記する） |
| 単体テスト | `Assets/Tests/Editor/` （`<対象クラス>Tests.cs`。[docs/testing.md](testing.md) 参照） |
| 自作プレハブ | `Assets/Prefabs/<カテゴリ>/` |
| ステージシーン | `Assets/Scenes/` （Build Settings と `GameMaster.stageNames` にも登録） |
| 動的ロードするアセット | `Assets/Resources/` （多用しない。基本は `[SerializeField]` で参照を渡す） |
| ドキュメント | `docs/` |

## 配置のルール

1. **サードパーティのフォルダを直接編集しない。**
   `StarterAssets/`, `PlayFabSDK/`, `Joystick Pack/`, `Kenney_assets/`, `TextMesh Pro/` は
   再インポートで上書きされます。改変が必要なら `Assets/Scripts/` 側に派生クラスを作るか、
   コピーして自作フォルダに置いてください。

2. **フォルダを新設したら、この表にも追記する。**

3. **移動・リネームは `.meta` ごと行う。**
   `.meta` を置き去りにすると GUID が変わり、シーン内の参照が全部外れます。
   Unity Editor の Project ウィンドウ上で移動するのが最も安全です。

4. **フォルダ名にスペースを使わない。**
   既存の `Joystick Pack` / `TextMesh Pro` はサードパーティ由来なのでそのままにします。
   新規フォルダは `PascalCase` で作ってください。
