# コーディング規約 (C# / Unity)

## 基本方針

**周囲のコードに合わせることを最優先します。** 以下のルールは新規コードに適用するもので、
既存コードを一括で書き換えるためのものではありません（依頼された場合を除く）。

## 命名

| 対象 | 規則 | 例 |
| --- | --- | --- |
| クラス / メソッド / public プロパティ | PascalCase | `PlayerHealth`, `TakeDamage()` |
| private フィールド | camelCase | `lastDamageTime` |
| 定数 | UPPER_SNAKE_CASE | `MAX_HEALTH` |
| ファイル名 | クラス名と一致させる | `PlayerHealth.cs` |

- 識別子は英語、**コメントは日本語**で書きます（既存コードがそうなっています）。
- namespace は現状使っていません。新規追加でも既存に合わせて namespace なしで構いません。

## インスペクター公開

新規コードでは `[SerializeField] private` を使います。

```csharp
[Header("Movement")]
[SerializeField, Tooltip("通常移動速度 (m/s)")]
private float moveSpeed = 2.0f;

// 外部に読ませたいときは読み取り専用プロパティで
public float MoveSpeed => moveSpeed;
```

ただし `Assets/Scripts/CharacterScripts/PlayerController.cs` など StarterAssets 由来のコードは
`public float MoveSpeed` というスタイルです。**そのファイルを編集するときは既存スタイルを踏襲**してください
（public → SerializeField に変えるとインスペクターの設定値が飛びます）。

`[Header]` / `[Tooltip]` は積極的に付けます。インスペクターで調整するのはコードを読まない人なので、
**Tooltip は単位（m/s、秒、m）まで書く**こと。

## パフォーマンス

1. `GameObject.Find()` / `GetComponent<T>()` を `Update()` の中で呼ばない。`Awake()` / `Start()` でキャッシュする。
2. `Update()` 内で毎フレーム文字列を作らない（`"HP: " + hp` や `ToString()`）。UI 更新は
   `PlayerHealth.OnHealthChanged` のように**値が変わったときだけイベントで通知**する。
3. 物理演算に関わる移動は `FixedUpdate()`、入力の取得は `Update()`。
4. WebGL / モバイルがターゲットなので、GC アロケーションを増やす書き方（毎フレームの `new`、LINQ）は避ける。

## null チェック

Unity のオブジェクトは `Destroy` 後も `== null` が true になる「偽 null」になります。

```csharp
// OK — Unity のライフタイムチェックが働く
if (target == null) return;

// NG — UnityEngine.Object には ?? / ?. を使わない
var t = target ?? fallback;
```

`GameObject.Find(...)?.GetComponent<T>()` のように **`Find` の戻り値に対する `?.` は既存コードにありますが、
新規コードでは明示的な `if (obj == null)` を推奨**します。

## 非同期処理

- **UniTask はこのプロジェクトに入っていません。** `UniTask` / `UniTaskVoid` を使うコードは書かないでください。
- 時間待ちや順次処理は Coroutine（`StartCoroutine` + `IEnumerator`）で書きます（`SceneChanger` が手本）。
- PlayFab の API はコールバック形式です。`SaveGameData(data, onComplete: () => { ... })` のように渡します。
- 導入したい場合は `Packages/manifest.json` の更新を含む別 PR にしてください。

## イベント

シーンをまたがない範囲の通知は C# の `event` を使います。

```csharp
public event System.Action<int, int> OnHealthChanged; // (現在の体力, 最大体力)
...
OnHealthChanged?.Invoke(health, MAX_HEALTH);
```

購読側は `OnEnable` で登録し、**`OnDisable` で必ず解除**します（`PlayFabManager` の `sceneLoaded` が手本）。

## MonoBehaviour のテンプレート

Unity が自動生成する空の `Start()` / `Update()` は、**中身がないなら消してください**。
空メソッドでも毎フレーム呼び出しコストが発生します。

## コメント

- 「何をしているか」ではなく「なぜそうしているか」を書く。
- クラスの役割は XML ドキュメントコメント（`/// <summary>`）で書く（`PlayerController` が手本）。
- 既存のコメントや `// region` 的な区切り線（`// ─────`）は、依頼がない限り消さない。

## 禁止・注意事項

- `.meta` ファイルを手で作らない・編集しない（Unity Editor が生成します）。
- `.unity` / `.prefab` / `.asset` をテキストエディタで書き換えない。
- 未使用の `using` を残さない。
- `Debug.Log` はデバッグ目的なら残して構いませんが、毎フレーム出力するものは残さない。
