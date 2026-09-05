# プロジェクト概要

## 基本情報

| 項目 | 内容 |
| --- | --- |
| プロジェクト名 | AIUEOWorld |
| リポジトリ | `git@github.com:koheix/FindingItems.git` |
| Unity バージョン | 6000.3.7f1 (Unity 6.3) |
| レンダーパイプライン | Universal Render Pipeline (URP 17.3.0) |
| ジャンル | 3D サードパーソン アクション（アイテム収集 + プラットフォーム） |
| ターゲット | WebGL / モバイル (iOS・Android) / PC |
| バックエンド | PlayFab（アカウント作成・ログイン・セーブデータ保存） |

## ゲームの流れ

```
TitleScene (PlayFab ログイン)
    ↓
Stage1_1 → Stage1_2 → ... (GameMaster.stageNames の順)
    ↓ クリア                      ↓ 落下 / 体力 0
ClearScene                     DeathScene（残機を消費して同ステージへ）
    ↓ 全ステージ踏破                ↓ 残機 0
AllClearScene                  GameOverScene（セーブデータ初期化）
```

## シーン一覧

Build Settings に登録されているシーン（`ProjectSettings/EditorBuildSettings.asset`）:

| シーン | 有効 | 役割 |
| --- | --- | --- |
| `Assets/Scenes/TitleScene.unity` | ✅ | タイトル・PlayFab ログイン |
| `Assets/Scenes/Stage1_1.unity` | ✅ | ステージ 1-1 |
| `Assets/Scenes/Stage1_2.unity` | ✅ | ステージ 1-2 |
| `Assets/Scenes/DeathScene.unity` | ✅ | death 演出（残機消費してリトライ） |
| `Assets/Scenes/ClearScene.unity` | ✅ | ステージクリア |
| `Assets/Scenes/GameOverScene.unity` | ✅ | ゲームオーバー（セーブデータ初期化） |
| `Assets/Scenes/AllClearScene.unity` | ✅ | 全ステージクリア |
| `Assets/Scenes/SampleScene.unity` | ❌ | Unity テンプレートの初期シーン |
| `Assets/StarterAssets/.../Playground.unity` | ❌ | StarterAssets のサンプル |

Build Settings 外の作業用シーン:

| シーン | 役割 |
| --- | --- |
| `Assets/Scenes/RandomStage.unity` | `StageGenerator` によるステージ自動生成の検証用 |
| `Assets/Scenes/TrainingScene.unity` | ML-Agents 学習用（`NaviAgent`） |
| `Assets/Scenes/RollerBallTrainingScene.unity` | ML-Agents チュートリアル（`RollerAgent`） |

> **ステージを追加したら**: Build Settings への登録と、`GameMaster.stageNames` への追記の**両方**が必要です。

## 主要パッケージ

`Packages/manifest.json` に定義。特に開発で意識するもの:

| パッケージ | バージョン | 用途 |
| --- | --- | --- |
| `com.unity.inputsystem` | 1.18.0 | 入力（`Assets/InputSystem_Actions.inputactions`） |
| `com.unity.cinemachine` | 2.10.5 | サードパーソンカメラ |
| `com.unity.render-pipelines.universal` | 17.3.0 | URP |
| `com.unity.ml-agents` | 4.0.1 | AI エージェントの学習 |
| `com.unity.probuilder` | 6.0.8 | ステージのブロックアウト |
| `com.unity.ai.navigation` | 2.0.9 | NavMesh（敵 AI） |

PlayFab SDK は UPM ではなく `Assets/PlayFabSDK/` に直接インポートされています。

> **注意**: UniTask と Odin Inspector は**インストールされていません**。非同期処理は Coroutine か
> PlayFab SDK のコールバックで書いてください。導入する場合は先に `Packages/manifest.json` の更新が必要です。

## レンダリング設定

`Assets/Settings/` に URP のアセットがあります。

- `PC_RPAsset.asset` / `PC_Renderer.asset` — PC 向け
- `Mobile_RPAsset.asset` / `Mobile_Renderer.asset` — モバイル / WebGL 向け
- `Assets/Settings/Build Profiles/` — プラットフォームごとのビルドプロファイル

## ビルド

ビルドは Unity Editor の Build Profiles から行います（CLI ビルドスクリプトは未整備）。
WebGL とモバイルを同時にターゲットにしているため、以下に注意:

- 重いシェーダー / ポストプロセスはモバイルプロファイルで無効化する
- 入力は「キーボード + マウス」と「タッチ（ジョイスティック）」の両方を必ず動かす
