using System.Collections.Generic;
using UnityEngine;

// ───────────────────────────────────────────────────────────────────────────
// ブロック1種類の設定。インスペクターで各プレハブに合わせた値を設定する。
// ───────────────────────────────────────────────────────────────────────────
[System.Serializable]
public class BlockConfig
{
    public GameObject prefab;

    [Tooltip("Z軸方向のブロックの奥行き (m) — プレハブの実サイズに合わせて設定")]
    public float depth = 10f;

    [Tooltip("プレハブのピボット(原点)から歩行面(上面)までのY距離 (m)")]
    public float surfaceOffsetY = 0.5f;
}

// ───────────────────────────────────────────────────────────────────────────
// ランダムなプラットフォームコースを実行時に自動生成するコンポーネント。
//
// 【使い方】
//   1. 空のGameObject (例: "StageGenerator") を作成してこのスクリプトをアタッチ
//   2. インスペクターで各プレハブを割り当てる
//   3. Block Configsの Depth / SurfaceOffsetY を実際のプレハブのサイズに合わせる
//   4. Playするとステージが自動生成される
//   5. インスペクターの歯車メニューから「ステージを再生成」でEditorでも確認できる
//
// 【調整の目安 (PlayerController のデフォルト値基準)】
//   JumpHeight = 1.0f, Gravity = -9.81f の場合:
//     最大ジャンプ高さ ≈ 1.0 m → maxStepUp を 0.8~0.9 に設定
//     最大水平距離 ≈ 4.8 m   → maxGapWidth を 4.0 以下に設定
// ───────────────────────────────────────────────────────────────────────────
public class StageGenerator : MonoBehaviour
{
    [Header("フラットブロック (必須)")]
    [Tooltip("平らな足場として使用するブロックのリスト。複数登録するとランダムに選ばれる")]
    public BlockConfig[] flatBlocks;

    [Header("スロープブロック (オプション)")]
    [Tooltip("坂道として使用するブロックのリスト。空の場合はスロープ生成なし")]
    public BlockConfig[] slopeBlocks;

    [Header("移動ブロック (オプション)")]
    [Tooltip("block-moving プレハブを設定するとコースに登場する")]
    public GameObject movingBlockPrefab;

    [Tooltip("移動ブロックのZ方向の占有幅 (m)")]
    public float movingBlockDepth = 5f;

    [Header("アイテムプレハブ (オプション)")]
    public GameObject clearKeyPrefab;
    public GameObject jumpJewelPrefab;
    public GameObject healHeartPrefab;

    [Header("ハザードプレハブ (オプション)")]
    [Tooltip("spike-block-wide などダメージブロックを設定するとコースに登場する")]
    public GameObject spikePrefab;

    [Header("プレイヤー開始位置 (オプション)")]
    [Tooltip("ここに設定したTransformのpositionをスタート位置に自動調整する")]
    public Transform playerSpawnPoint;

    // ─── 生成パラメータ ──────────────────────────────────────

    [Header("生成パラメータ")]
    [Tooltip("コースのセクション数。多いほど長いステージになる")]
    public int sectionCount = 20;

    [Tooltip("シード値。-1 でランダム。同じ値なら同じコースが再現できる")]
    public int seed = -1;

    [Range(0f, 0.4f)]
    [Tooltip("隙間(ギャップ)が発生する確率")]
    public float gapChance = 0.15f;

    [Range(0f, 0.4f)]
    [Tooltip("移動ブロックが登場する確率")]
    public float movingBlockChance = 0.12f;

    [Range(0f, 0.4f)]
    [Tooltip("スロープが登場する確率")]
    public float slopeChance = 0.2f;

    [Range(0f, 0.5f)]
    [Tooltip("アイテムが配置される確率")]
    public float itemChance = 0.2f;

    [Range(0f, 0.3f)]
    [Tooltip("ハザードが配置される確率")]
    public float hazardChance = 0.1f;

    [Header("高さ設定")]
    [Tooltip("1セクションで上昇できる最大高さ (m) — PlayerController.JumpHeight 未満に設定")]
    public float maxStepUp = 0.85f;

    [Tooltip("1セクションで下降できる最大高さ (m)")]
    public float maxStepDown = 4f;

    [Header("ギャップ設定")]
    [Tooltip("ギャップの最小幅 (m)")]
    public float minGapWidth = 2f;

    [Tooltip("ギャップの最大幅 (m) — スプリントジャンプで越えられる距離未満に設定")]
    public float maxGapWidth = 4f;

    [Header("スタート / ゴール台")]
    [Tooltip("スタートとゴールの固定プラットフォームの奥行き (m)")]
    public float startGoalDepth = 15f;

    // ─── 内部状態 ────────────────────────────────────────────

    private readonly List<GameObject> _spawned = new();
    private float _curZ;  // 次のブロックを置き始めるZ座標
    private float _surY;  // 現在の歩行面Y座標

    // ─── Unity ライフサイクル ─────────────────────────────────

    void Start()
    {
        Generate();
    }

    // ─── パブリック API ──────────────────────────────────────

    /// <summary>既存の生成物を消去して新しいステージを生成する。</summary>
    [ContextMenu("ステージを再生成")]
    public void Generate()
    {
        ClearGenerated();

        if (flatBlocks == null || flatBlocks.Length == 0)
        {
            Debug.LogError("[StageGenerator] flatBlocks が未設定です。インスペクターでブロックを登録してください。");
            return;
        }

        int useSeed = seed >= 0 ? seed : Random.Range(0, int.MaxValue);
        Random.InitState(useSeed);
        Debug.Log($"[StageGenerator] Seed={useSeed}  Sections={sectionCount}");

        _curZ = 0f;
        _surY = 0f;

        PlaceStartPlatform();

        for (int i = 0; i < sectionCount; i++)
            GenerateSection(i);

        PlaceGoalPlatform();
        SetupPlayerSpawn();
    }

    /// <summary>生成されたすべてのオブジェクトを破棄する。</summary>
    [ContextMenu("生成をクリア")]
    public void ClearGenerated()
    {
        foreach (var obj in _spawned)
            if (obj != null) Destroy(obj);
        _spawned.Clear();
    }

    // ─── 生成ステップ ─────────────────────────────────────────

    void PlaceStartPlatform()
    {
        PlaceBlockConfig(flatBlocks[0], 0f, startGoalDepth);
    }

    void GenerateSection(int index)
    {
        // 最初のセクション以外でギャップを生成
        if (index > 0 && Random.value < gapChance)
        {
            _curZ += Random.Range(minGapWidth, maxGapWidth);
            return;
        }

        // 次の高さを決定
        float heightRoll = Random.value;
        if (heightRoll < 0.35f)
            _surY += Random.Range(0.2f, maxStepUp);   // 上昇
        else if (heightRoll < 0.60f)
            _surY -= Random.Range(0.5f, maxStepDown); // 下降
        // 残り 40% は同じ高さ
        _surY = Mathf.Clamp(_surY, -12f, 20f);

        // ブロックタイプを選択して配置
        float typeRoll = Random.value;
        float threshold = 0f;

        threshold += movingBlockChance;
        if (typeRoll < threshold && movingBlockPrefab != null)
        {
            PlaceMovingBlock();
            return;
        }

        threshold += slopeChance;
        bool useSlope = typeRoll < threshold && slopeBlocks != null && slopeBlocks.Length > 0;
        var pool = useSlope ? slopeBlocks : flatBlocks;
        var cfg = pool[Random.Range(0, pool.Length)];

        float sectionStartZ = _curZ;
        PlaceBlockConfig(cfg, _surY, cfg.depth);
        TryPlaceDecoration(sectionStartZ, cfg.depth);
    }

    void PlaceGoalPlatform()
    {
        float goalStartZ = _curZ;
        PlaceBlockConfig(flatBlocks[0], _surY, startGoalDepth);

        if (clearKeyPrefab != null)
        {
            float keyZ = goalStartZ + startGoalDepth * 0.5f;
            Spawn(clearKeyPrefab, new Vector3(0f, _surY + 1.5f, keyZ));
        }
    }

    // ─── アイテム / ハザード配置 ───────────────────────────────

    void TryPlaceDecoration(float blockStartZ, float blockDepth)
    {
        float roll = Random.value;
        float centerZ = blockStartZ + blockDepth * 0.5f;

        if (roll < itemChance)
        {
            var candidates = new List<GameObject>();
            if (jumpJewelPrefab != null) candidates.Add(jumpJewelPrefab);
            if (healHeartPrefab != null) candidates.Add(healHeartPrefab);
            if (candidates.Count == 0) return;

            float offsetX = Random.Range(-1.5f, 1.5f);
            Spawn(candidates[Random.Range(0, candidates.Count)],
                  new Vector3(offsetX, _surY + 1.5f, centerZ));
        }
        else if (roll < itemChance + hazardChance && spikePrefab != null)
        {
            float offsetX = Random.Range(-1f, 1f);
            Spawn(spikePrefab, new Vector3(offsetX, _surY + 0.05f, centerZ));
        }
    }

    // ─── ブロック配置ヘルパー ─────────────────────────────────

    // surfaceY = 歩行面のY座標, depth = ブロックのZ方向長さ
    void PlaceBlockConfig(BlockConfig cfg, float surfaceY, float depth)
    {
        if (cfg?.prefab == null) return;

        float originY = surfaceY - cfg.surfaceOffsetY;
        float centerZ = _curZ + depth * 0.5f;

        var obj = Instantiate(cfg.prefab,
            new Vector3(0f, originY, centerZ),
            Quaternion.identity, transform);
        _spawned.Add(obj);
        _curZ += depth;
    }

    void PlaceMovingBlock()
    {
        var obj = Instantiate(movingBlockPrefab,
            new Vector3(0f, _surY, _curZ + movingBlockDepth * 0.5f),
            Quaternion.identity, transform);
        _spawned.Add(obj);
        _curZ += movingBlockDepth;
    }

    GameObject Spawn(GameObject prefab, Vector3 pos)
    {
        var obj = Instantiate(prefab, pos, Quaternion.identity);
        _spawned.Add(obj);
        return obj;
    }

    // ─── プレイヤー開始位置 ───────────────────────────────────

    void SetupPlayerSpawn()
    {
        // スタート台の中央上にプレイヤーを配置する
        // (スタート台: Z = 0 〜 startGoalDepth, Y = 0 が歩行面)
        Vector3 spawnPos = new Vector3(0f, 1.5f, startGoalDepth * 0.5f);

        // "Player" タグのオブジェクトを探してそこに移動する
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // CharacterController は無効化しないと transform.position が上書きされない
            var cc = player.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
            player.transform.position = spawnPos;
            if (cc) cc.enabled = true;
        }
        else
        {
            Debug.LogWarning("[StageGenerator] 'Player' タグのGameObjectが見つかりません。" +
                             "CharacterにPlayerタグを設定してください。");
        }

        // playerSpawnPoint が設定されていれば参照位置としても更新する
        if (playerSpawnPoint != null) playerSpawnPoint.position = spawnPos;
    }

    // ─── Gizmo (Scene View での可視化) ───────────────────────

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // スタート台のプレイヤー初期位置を青球で表示
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(new Vector3(0f, 1.5f, startGoalDepth * 0.5f), 0.6f);

        // ゴール方向を矢印で示す
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position + Vector3.up, Vector3.forward * 3f);
    }
#endif
}
