using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using TMPro;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;

    
    public TMP_InputField usernameInput;
    public Button loginButton;

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
    private void Start()
    {
        if(loginButton != null)
        {
            loginButton.onClick.AddListener(Login);
        }
    }

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
        UnityEngine.SceneManagement.SceneManager.LoadScene("Stage1_1");
    }

    void OnError(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    public void GetUserData()
    {
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            result =>
            {
                Debug.Log("データ取得成功");
            },
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
            }
        );
    }
}