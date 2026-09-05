# アーキテクチャ

主要なシステムと、その責務・依存関係をまとめます。コードを変更する前にここを読んでください。

## 全体像

```
                    ┌──────────────────────┐
                    │  PlayFabManager      │  シングルトン (DontDestroyOnLoad)
                    │  - ログイン           │  static SaveData CurrentSaveData
                    │  - セーブ/ロード      │
                    └──────────┬───────────┘
                               │ 参照
                    ┌──────────┴───────────┐
                    │  GameMaster (static) │  進行管理（Death / Clear / GameOver）
                    └──────────┬───────────┘
          ┌────────────────────┼────────────────────┐
          │                    │                    │
  ┌───────┴───────┐   ┌────────┴────────┐  ┌────────┴────────┐
  │ PlayerHealth  │   │ PlayerFallChecker│  │ ClearKey        │
  │ 体力 0 → Death │   │ 落下 → Death     │  │ 取得 → Clear    │
  └───────┬───────┘   └─────────────────┘  └─────────────────┘
          │ OnHealthChanged (C# event)
  ┌───────┴───────┐
  │ HealthUI      │
  └───────────────┘
```

## ゲーム進行 — `GameMaster`

`Assets/Scripts/GameMasterScripts/GameMaster.cs` — インスタンスを持たない静的クラス。

| メンバー | 役割 |
| --- | --- |
| `static string[] stageNames` | ステージの並び順。**ステージ追加時はここと Build Settings の両方を更新する** |
| `static bool isDying` | `Death()` の多重呼び出しガード |
| `static void Death()` | 残機を 1 減らす。残っていれば `DeathScene`、0 なら `GameOver()` |
| `static void Clear()` | `stageNames` の次ステージへ進める。最終ステージなら `AllClearScene` |
| `static void GameOver()` | セーブデータを初期化して `GameOverScene` へ |

いずれも `PlayFabManager.Instance` / `PlayFabManager.CurrentSaveData` に依存します。
**`PlayFabManager` が存在しないシーンから呼ぶと NullReferenceException になります**（デバッグ用シーンで単体再生するときの注意点）。

## シーン遷移 — `SceneChanger`

`Assets/Scripts/GameoverScripts/SceneChanger.cs` — `DeathScene` / `ClearScene` / `AllClearScene` /
`GameOverScene` に共通で置く演出スクリプト。アクティブなシーン名で分岐し、3 秒後に次のシーンへ遷移します。

- `DeathScene` → `CurrentSaveData.nowStageName`（同じステージへリトライ）
- `ClearScene` → `CurrentSaveData.nowStageName`（`Clear()` で次ステージ名に更新済み）
- `AllClearScene` / `GameOverScene` → `TitleScene`

シーン内の特定 GameObject 名（`ClearStageName`, `RemainingLivesText`）を `GameObject.Find()` で取得しているため、
**該当シーンの UI オブジェクト名を変更すると壊れます**。

## セーブデータ — `SaveData` / `PlayFabManager`

- `Assets/Scripts/UtilScripts/SaveData.cs` — `[System.Serializable]` な POCO。
  `nowStageName` / `previousStageName` / `remainingLives` / `score` / `coinNum`。
- `Assets/Scripts/TitleSceneScripts/PlayFabManager.cs` — シングルトン。`Awake()` で `DontDestroyOnLoad`。
  `sceneLoaded` イベントでシーンごとに UI（`UserNameInputField`, `LoginButton`）を取得し直します。

セーブデータのフィールドを増やすときは、PlayFab 側の User Data のキー構成にも影響するため、
既存プレイヤーのデータでもデシリアライズできるか（欠損フィールドの既定値）に注意してください。

## プレイヤー — `Assets/Scripts/CharacterScripts/`

Unity StarterAssets の ThirdPersonController をベースにした実装です。そのため
**public フィールド + `[Header]` / `[Tooltip]` というスタイルを踏襲**しています（[coding-guidelines.md](coding-guidelines.md) 参照）。

| スクリプト | 責務 |
| --- | --- |
| `PlayerController` | 移動・ジャンプ・カメラ。Input System (`PlayerInput`) + タッチ (`VariableJoystick`, `TouchLookZone`) の両対応。`OnTriggerEnter` で `CollectibleItem` を検出して `OnCollect()` を呼ぶ |
| `PlayerHealth` | 体力・無敵時間。`OnHealthChanged(現在値, 最大値)` イベントを発火。0 で `GameMaster.Death()` |
| `PlayerFallChecker` | `fallThreshold` より下に落ちたら `GameMaster.Death()` |
| `PlayerKnockback` | ダメージ時のノックバック |
| `PlayerVisualEffect` | 被弾などの見た目の演出 |

## アイテム — `Assets/Scripts/ItemScripts/`

`CollectibleItem` インターフェース（`OnCollect(GameObject collector)` / `GetValue()` / `GetItemName()`）を実装します。
取得の起点は `PlayerController` の `OnTriggerEnter` 側にあるので、**アイテム側にトリガーコライダーを付けるだけ**で動きます。

- `HealHeart` — 体力回復
- `JumpUpJewel` — ジャンプ力アップ（一時的）
- `ClearKey` — `GameMaster.Clear()` を呼ぶステージクリア鍵

新しいアイテムを追加するときは `CollectibleItem` を実装し、プレハブを `Assets/Prefabs/Items/` に置いてください。

## ステージ

- `Assets/Scripts/BlockScripts/` — `MovingBlock`（往復移動）、`DamageBlock`（接触ダメージ）
- `Assets/Scripts/StageGeneratorScripts/StageGenerator.cs` — `BlockConfig` の配列からランダムに
  プラットフォームコースを生成する。ジャンプ到達距離（`PlayerController.JumpHeight` / `Gravity`）に
  合わせて `maxStepUp` / `maxGapWidth` を設定する必要があるため、**プレイヤーのジャンプ性能を変えたら
  StageGenerator のパラメータも見直す**こと。検証シーンは `Assets/Scenes/RandomStage.unity`。

## ML-Agents

- `Assets/Scripts/TrainingScripts/NaviAgent.cs` — 本編ステージでの学習エージェント（`TrainingScene`）
- `Assets/Scripts/RollerBallTrainingScripts/RollerAgent.cs` — 公式チュートリアルの RollerBall（`RollerBallTrainingScene`）
- 学習結果のタイマー出力は `Assets/ML-Agents/Timers/`

学習は Python 側（`mlagents-learn`）から実行します。ゲーム本編のロジックとは切り離して扱ってください。

## 設計上の約束

1. **疎結合を保つ**: 直接の相互参照より、C# の `event`（`PlayerHealth.OnHealthChanged` が手本）や
   静的なマネージャー経由を優先する。
2. **シーンをまたぐ状態は `PlayFabManager.CurrentSaveData` に集約する**。新しいグローバル変数を
   別の static クラスに増やさない。
3. **`GameObject.Find()` は既存箇所の踏襲にとどめる**。新規コードでは `[SerializeField]` で参照を渡す。
