using UnityEngine;

public class HealHeart : MonoBehaviour, CollectibleItem
{
    [Header("回復量")]
    [SerializeField] private int healAmount = 1; // 回復量

    public void OnCollect(GameObject collector)
    {
        // プレイヤーの体力を回復する効果を実装
        PlayerHealth playerHealth = collector.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount); // プレイヤーの体力をhealAmount分回復
        }

        // アイテムを消す
        Destroy(gameObject);
    }

    public float GetValue()
    {
        return healAmount;
    }

    public string GetItemName()
    {
        return "Heal Heart";
    }
}
