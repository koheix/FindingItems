using UnityEngine;
using UnityEngine.SceneManagement; // シーン切り替えに必要

public class GameMaster : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void GameOver()
    {
        // // 現在のシーンの名前を取得して、最初から読み込み直す
        // string currentSceneName = SceneManager.GetActiveScene().name;
        // SceneManager.LoadScene(currentSceneName);
        GameData.previousSceneName = SceneManager.GetActiveScene().name; // どのシーンから来たのかを保存
        SceneManager.LoadScene("GameOverScene"); // ゲームオーバーシーンに切り替える
    }

    public static void Death()
    {
        // 現在のシーンの名前を取得して、最初から読み込み直す
        GameData.previousSceneName = SceneManager.GetActiveScene().name; // どのシーンから来たのかを保存
        SceneManager.LoadScene("DeathScene"); // 死亡シーンに切り替える
    }
}
