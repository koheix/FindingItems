using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private const int MAX_HEALTH = 3;
    // 外部から参照用
    public int MaxHealth => MAX_HEALTH;

    [Header("プレイヤーの体力")]
    // getter setter付きのプロパティ
    public int health {get; private set;}
    // [SerializeField]private int health = MAX_HEALTH;

    // 無敵時間
    private float invincibleTime = 1.0f; // 無敵時間（秒）
    private float lastDamageTime = -10.0f; // 最後にダメージを受けた時間

    // 無敵状態かどうかを返すメソッド
    public bool IsInvincible => (Time.time - lastDamageTime) < invincibleTime;
    

    // 体力変更時のイベント
    public event System.Action<int, int> OnHealthChanged; // 引数は(現在の体力, 最大体力)

    void Start()
    {
        health = MAX_HEALTH;
        // 初期体力をUIに反映
        OnHealthChanged?.Invoke(health, MAX_HEALTH);
    }

    // ダメージを受けるメソッド
    public void TakeDamage(int damage)
    {
        if (IsInvincible)
            return; // 無敵状態ならダメージを受けない

        health -= damage;
        lastDamageTime = Time.time; // ダメージを受けた時間を更新
        // 体力が0以下になった場合の処理
        if (health <= 0)
        {
            // Die();
            // Dieメソッドは別のスクリプトで実装予定
        }
        // UI更新のためにイベントを呼び出す
        OnHealthChanged?.Invoke(health, MAX_HEALTH);
    }
    // // プレイヤーが死亡したときの処理
    // private void Die()
    // {
    //     // 死亡処理をここに実装（例：リスポーン、ゲームオーバー画面の表示など）
    //     Debug.Log("Player has died.");
    // }

    // 体力を回復するメソッド
    public void Heal(int amount)
    {
        // 最大体力を超えないように回復
        health = Mathf.Min(health + amount, MAX_HEALTH);
        // UI更新のためにイベントを呼び出す
        OnHealthChanged?.Invoke(health, MAX_HEALTH);
    }
}
