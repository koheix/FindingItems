using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

// 3秒後にプレイしていたステージに戻る処理を行うスクリプト
public class SceneChanger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // クリアステージならClearStageNameのTMProTextを取得して、クリアしたステージ名を表示する
        if(SceneManager.GetActiveScene().name == "ClearScene")
        {
            string clearStageName = PlayFabManager.CurrentSaveData.previousStageName; // プレイしていたステージ名を取得
            Debug.Log("クリアしたステージ: " + clearStageName);
            // ClearStageNameのTMProTextを取得して、クリアしたステージ名を表示する
            TMPro.TextMeshProUGUI clearStageNameText = GameObject.Find("ClearStageName").GetComponent<TMPro.TextMeshProUGUI>();
            clearStageNameText.text = clearStageName; // クリアしたステージ名を表示する
        }
        else if(SceneManager.GetActiveScene().name == "DeathScene")
        {
            // 残基の数をRemainingLivesTextのTMProTextに表示する
            int remainingLives = PlayFabManager.CurrentSaveData.remainingLives; // 残基の数を取得
            Debug.Log("残基の数: " + remainingLives);
            TextMeshProUGUI remainingLivesText = GameObject.Find("RemainingLivesText").GetComponent<TMPro.TextMeshProUGUI>();
            remainingLivesText.text = "残りライフ: " + remainingLives.ToString(); // 残基の数を表示する
        }
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
