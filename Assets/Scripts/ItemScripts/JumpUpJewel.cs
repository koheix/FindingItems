using UnityEngine;

public class JumpUpJewel : MonoBehaviour, CollectibleItem
{
    [SerializeField] private float jumpPower = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollect(GameObject collector)
    {
        // ジャンプ力を上げる効果を実装
        PlayerController playerController = collector.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.IncreaseJumpPower(jumpPower); // ジャンプ力をjumpPower分増加
        }

        // アイテムを消す
        Destroy(gameObject);
    }

    public float GetValue()
    {
        return jumpPower;
    }

    public string GetItemName()
    {
        return "Jump Up Jewel";
    }
}
