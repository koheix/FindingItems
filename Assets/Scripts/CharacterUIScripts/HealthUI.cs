using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    // [SerializeField] private GameObject[] heartIcons; // ハートアイコンの配列
    [SerializeField] private Slider healthSlider;
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
        // 体力スライダーの更新
        if (healthSlider != null)
            healthSlider.value = (float)current / max;
        // 体力テキストの更新
        if (healthText != null)
            healthText.text = $"{current}/{max}";
    }
    // // Update is called once per frame
    // void Update()
    // {
        
    // }
}
