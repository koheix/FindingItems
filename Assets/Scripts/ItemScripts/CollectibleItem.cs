/**
    * CollectibleItem.cs
    * 
    * 収集可能アイテムのインターフェース
*/

using UnityEngine;

public interface CollectibleItem
{
    // アイテムが収集されたときに呼び出されるメソッド
    void OnCollect(GameObject collector);
    // アイテムの効果、価値を取得するメソッド（回復アイテムなら回復量など）
    float GetValue();
    string GetItemName();
}
