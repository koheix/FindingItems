using UnityEngine;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject[] heartIcons; // ハートアイコンの配列
    [SerializeField] private TextMeshProUGUI healthText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHealth.OnHealthChanged += UpdateHealthUI;
        UpdateHealthUI(playerHealth.health, playerHealth.MaxHealth); // 初期体力をUIに反映
    }

    void OnDestroy()
    {
        playerHealth.OnHealthChanged -= UpdateHealthUI;
    }

    private void UpdateHealthUI(int current, int max)
    {
        // ハートアイコンの更新
        if (heartIcons != null && heartIcons.Length > 0)
        {
            // maxに合わせてハートの数を調整
            for (int i = 0; i < heartIcons.Length; i++)
            {
                heartIcons[i].SetActive(i < current);
            }
        }
        // 体力テキストの更新
        if (healthText != null)
            healthText.text = $"{current}/{max}";
    }
    // // Update is called once per frame
    // void Update()
    // {
        
    // }
}
