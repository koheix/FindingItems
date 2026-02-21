using UnityEngine;

public class PlayerVisualEffect : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private Renderer playerRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerRenderer = GetComponentInChildren<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerRenderer == null)
            return;
        // 無敵時間中は点滅させる
        if (playerHealth != null && playerHealth.IsInvincible)
        {
            Debug.Log("Player is invincible, applying visual effect.");
            float t = Mathf.PingPong(Time.time * 5, 1.0f);
            playerRenderer.material.color = Color.Lerp(Color.white, Color.red, t);

        }
        else
        {
            // 通常時は不透明に戻す
            playerRenderer.material.color = Color.white;
        }
        
    }
}
