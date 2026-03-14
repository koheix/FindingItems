using UnityEngine;
using UnityEngine.SceneManagement; // シーン切り替えに必要

public class GameMaster : MonoBehaviour
{
    // deathが呼ばれているときのフラグ
    public static bool isDying = false;

    // ステージ名のリスト
    public static readonly string[] stageNames = {
        "Stage1_1",
        "Stage1_2",
    };
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
        // CurrentSaveDataを初期化
        PlayFabManager.CurrentSaveData = new SaveData();
        // playfabのセーブデータも初期化
        PlayFabManager.Instance.SaveGameData(
                PlayFabManager.CurrentSaveData,
                onComplete: () =>
                {
                    Debug.Log("ゲームオーバーシーンに切り替える");
                    SceneManager.LoadScene("GameOverScene"); // ゲームオーバーシーンに切り替える
                }
        );
    }

    public static void Death()
    {
        if (isDying)
            return; // すでにdeath処理が行われている場合は何もしない
        isDying = true;

        PlayFabManager.CurrentSaveData.wholeLife--; // 残機を1減らす
        // 残基がある場合はリスポーン、ない場合はゲームオーバー
        if(PlayFabManager.CurrentSaveData.wholeLife > 0)
        {
            PlayFabManager.Instance.SaveGameData(PlayFabManager.CurrentSaveData); // セーブデータを更新
            // リスポーン処理をここに実装（例：プレイヤーの位置を初期位置に戻すなど）
            Debug.Log("Player has died. Remaining lives: " + PlayFabManager.CurrentSaveData.wholeLife);
            SceneManager.LoadScene("DeathScene"); // 死亡シーンに切り替える
        }
        else
        {
            Debug.Log("No remaining lives. Game Over.");
            GameOver(); // ゲームオーバー処理を呼び出す
        }
        isDying = false;
        // // 現在のシーンの名前を取得して、最初から読み込み直す
        // GameData.previousSceneName = SceneManager.GetActiveScene().name; // どのシーンから来たのかを保存
        // SceneManager.LoadScene("DeathScene"); // 死亡シーンに切り替える
    }

    public static void Clear()
    {
        // 現在のステージ名のインデックスを取得
        int currentIndex = System.Array.IndexOf(stageNames, PlayFabManager.CurrentSaveData.nowStageName);
        // 次のステージのインデックスを計算
        int nextIndex = currentIndex + 1;
        // 次のステージが存在するかチェック
        if (nextIndex < stageNames.Length)
        {
            PlayFabManager.CurrentSaveData.nowStageName = stageNames[nextIndex]; // 次のステージに進む
            PlayFabManager.Instance.SaveGameData(PlayFabManager.CurrentSaveData); // セーブデータを更新
            SceneManager.LoadScene("ClearScene"); // クリアシーンに切り替える
        }
        else
        {
            // 最終ステージの場合、AllClearシーンとタイトルシーンに戻る
            PlayFabManager.CurrentSaveData.nowStageName = "Stage1_1"; // 最初のステージに戻るための情報を保存
            PlayFabManager.Instance.SaveGameData(PlayFabManager.CurrentSaveData); // セーブデータを更新
            SceneManager.LoadScene("AllClearScene"); // AllClearシーンに切り替える
        }
    }
}
