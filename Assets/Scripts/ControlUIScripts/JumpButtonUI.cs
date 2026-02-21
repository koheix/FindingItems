using UnityEngine;
using UnityEngine.UI;

public class JumpButtonUI : MonoBehaviour
{
    [Header("PlayerController Reference")]
    public PlayerController playerController;

    void Start()
    {
        // ボタンが押されたときのイベントリスナーを設定
        Button jumpButton = GetComponent<Button>();
        if (jumpButton != null)
        {
            Debug.Log("OnJumpButtonPressed event listener added.");
            jumpButton.onClick.AddListener(OnJumpButtonPressed);
        }
        else
        {
            Debug.LogError("JumpButtonUI: Button component not found on the GameObject.");
        }
    }

    public void OnJumpButtonPressed()
    {
        if (playerController != null)
        {
            Debug.Log("Jump button pressed!");
            playerController.JumpButtonPressed();
        }
    }

}
