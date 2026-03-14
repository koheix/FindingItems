using UnityEngine;

public class ClearKey : MonoBehaviour, CollectibleItem
{
    [Header("クリアキーゲットのスコア")]
    [SerializeField] private float scoreValue = 100f;

    public void OnCollect(GameObject collector)
    {
        // クリアキーを取ったときの処理を実装
        PlayFabManager.CurrentSaveData.score += (int)scoreValue; // スコアを加算
        GameMaster.Clear(); // クリア処理を呼び出す

        // アイテムを消す
        Destroy(gameObject);
    }

    public float GetValue()
    {
        return scoreValue; // クリアキーは指定されたスコアを返す
    }

    public string GetItemName()
    {
        return "Clear Key";
    }
}
