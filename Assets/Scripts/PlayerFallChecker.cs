using UnityEngine;
using UnityEngine.SceneManagement; // シーン切り替えに必要

public class PlayerFallChecker : MonoBehaviour
{
    // この数値より下に落ちたらゲームオーバー
    public float fallThreshold = -10f;

    void Update()
    {
        // プレイヤーのY座標がしきい値を下回ったかチェック
        if (transform.position.y < fallThreshold)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        // 現在のシーンの名前を取得して、最初から読み込み直す
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}