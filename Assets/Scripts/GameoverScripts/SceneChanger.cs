using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// 3秒後にプレイしていたステージに戻る処理を行うスクリプト
public class SceneChanger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(ReturnAfterDelay());
    }

    IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        // デスシーンなら、プレイしていたステージに戻る
        if(SceneManager.GetActiveScene().name == "DeathScene")
        {
            Debug.Log("デスシーンからステージに戻る");
            SceneManager.LoadScene(PlayFabManager.CurrentSaveData.nowStageName); // プレイしていたステージに戻る
        }
        else if(SceneManager.GetActiveScene().name == "ClearScene") // クリアシーンなら、次のステージに進む
        {
            Debug.Log("クリアシーンから次のステージに進む");
            SceneManager.LoadScene(PlayFabManager.CurrentSaveData.nowStageName); // 次のステージに進む
        }
        else if(SceneManager.GetActiveScene().name == "AllClearScene") // AllClearシーンなら、タイトルシーンに戻る
        {
            Debug.Log("AllClearシーンからタイトルに戻る");
            SceneManager.LoadScene("TitleScene"); // タイトルシーンに戻る
        }
        else // ゲームオーバーシーンなら、タイトルシーンに戻る
        {
            Debug.Log("ゲームオーバーシーンからタイトルに戻る");
            SceneManager.LoadScene("TitleScene"); // タイトルシーンに戻る
        }
    }
}
