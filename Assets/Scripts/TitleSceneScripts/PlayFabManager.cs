using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;
    // ユーザデータ
    public static SaveData CurrentSaveData { get; set; }

    // 消えないようにコードで設定
    private TMP_InputField usernameInput;
    private Button loginButton;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // シーンロード完了時に再取得
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // シーンごとにUIを再取得
        usernameInput  = GameObject.Find("UserNameInputField")?.GetComponent<TMP_InputField>();
        loginButton = GameObject.Find("LoginButton")?.GetComponent<Button>();

        // ボタンの参照が変わるので、リスナーも再設定
        if (loginButton != null)
        {
            loginButton.onClick.RemoveAllListeners();
            loginButton.onClick.AddListener(Login);
        }
    }

    // private void Start()
    // {
    //     if(loginButton != null)
    //     {
    //         loginButton.onClick.AddListener(Login);
    //     }
    // }

    public void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = usernameInput.text,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("ログイン成功");
        // LoadGameData(); // ログイン成功後にデータをロード
        // UnityEngine.SceneManagement.SceneManager.LoadScene("Stage1_1");
        LoadGameData(onComplete: () =>
        {
            // ここでは確実にデータが入っている
            UnityEngine.SceneManagement.SceneManager.LoadScene(CurrentSaveData.nowStageName);
        });
    }

    void OnError(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    // public void GetUserData()
    // {
    //     PlayFabClientAPI.GetUserData(
    //         new GetUserDataRequest(),
    //         result =>
    //         {
    //             Debug.Log("データ取得成功");
    //         },
    //         error =>
    //         {
    //             Debug.LogError(error.GenerateErrorReport());
    //         }
    //     );
    // }

    public void SaveGameData(SaveData data, System.Action onComplete = null)
    {
        string json = JsonUtility.ToJson(data);

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "save", json }
            }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => {
                Debug.Log("保存成功");
                onComplete?.Invoke(); // コールバック
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    public void LoadGameData( System.Action onComplete = null)
    {
        Debug.Log("データロード開始");
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.ContainsKey("save"))
                {
                    string json = result.Data["save"].Value;
                    SaveData data = JsonUtility.FromJson<SaveData>(json);
                    CurrentSaveData = data; // ロードしたデータを保存
                    Debug.Log("データロード成功");

                    Debug.Log("remainingLives: " + data.remainingLives);
                    onComplete?.Invoke(); // コールバック
                }
                else
                {
                    Debug.Log("保存されたデータがありません");
                    CurrentSaveData = new SaveData(); // データがない場合は新規作成
                    SaveGameData(
                        CurrentSaveData,
                        onComplete: () =>
                        {
                            Debug.Log("新規データ保存成功");
                            onComplete?.Invoke(); // コールバック
                        }
                    ); // 新規データを保存
                }
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }
}