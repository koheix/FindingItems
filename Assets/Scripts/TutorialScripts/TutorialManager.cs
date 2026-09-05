using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// チュートリアル用のメッセージ UI と進行管理。
///
/// 【使い方】
///   1. ステージのシーンに空の GameObject (例: "TutorialManager") を作り、このスクリプトをアタッチする
///   2. Player Health にプレイヤーの PlayerHealth をアサインする（未設定ならシーンから自動取得）
///   3. Steps はインスペクターで編集できる（既定で Stage1_1 用の文言が入っている）
///
/// 【挙動】
///   メッセージ表示中は Time.timeScale = 0 でゲームを止め、OK ボタン（またはキーボードの Enter）で再開する。
///   その後 WaitFor に指定した条件（被弾・アイテム取得）が満たされると次のメッセージへ進む。
///   PC ではゲーム中カーソルがロックされているため、表示中だけロックを解除して
///   マウスで OK を押せるようにし、閉じるときに元の状態へ戻す。
/// </summary>
public class TutorialManager : MonoBehaviour
{
    /// <summary>メッセージを閉じたあと、次のメッセージへ進むための条件。</summary>
    public enum TutorialTrigger
    {
        None,           // 条件なし。OK を押したらすぐ次のメッセージへ
        PlayerDamaged,  // プレイヤーがダメージを受けたら次へ
        ItemCollected,  // ItemName のアイテムを取得したら次へ
    }

    [System.Serializable]
    public class TutorialStep
    {
        [Tooltip("表示するメッセージ")]
        [TextArea(2, 5)]
        public string message;

        [Tooltip("OK を押したあと、次のメッセージへ進むための条件")]
        public TutorialTrigger waitFor = TutorialTrigger.None;

        [Tooltip("WaitFor が ItemCollected のときの対象アイテム名 (CollectibleItem.GetItemName() の戻り値)")]
        public string itemName;
    }

    [Header("参照")]
    [Tooltip("プレイヤーの PlayerHealth。未設定ならシーンから自動取得する")]
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("メッセージ表示に使うフォント。未設定なら TMP の既定フォントを使う（日本語を出すには日本語フォントが必要）")]
    [SerializeField] private TMP_FontAsset fontAsset;

    [Header("チュートリアルの内容")]
    [Tooltip("上から順に表示される")]
    [SerializeField]
    private TutorialStep[] steps =
    {
        new TutorialStep
        {
            message = "このステージはチュートリアルです。\nまずはトゲのあるダメージブロックにわざと当たってみよう。",
            waitFor = TutorialTrigger.PlayerDamaged,
        },
        new TutorialStep
        {
            message = "ダメージブロックに当たるとライフが1つ減ります。\nライフが0になるとゲームオーバーです。",
            waitFor = TutorialTrigger.None,
        },
        new TutorialStep
        {
            message = "減ったライフは回復できます。\nハートのアイテムを取りに行こう。",
            waitFor = TutorialTrigger.ItemCollected,
            itemName = "Heal Heart",
        },
        new TutorialStep
        {
            message = "ハートを取るとライフが回復しました。\nダメージを受けたら探してみましょう。",
            waitFor = TutorialTrigger.None,
        },
        new TutorialStep
        {
            message = "次はジャンプのアイテムです。\n光っているジュエルを取りに行こう。",
            waitFor = TutorialTrigger.ItemCollected,
            itemName = "Jump Up Jewel",
        },
        new TutorialStep
        {
            message = "ジュエルを取ると、しばらくジャンプ力が上がります。\n高い足場はこれを使って越えましょう。",
            waitFor = TutorialTrigger.None,
        },
        new TutorialStep
        {
            message = "最後に鍵を取るとステージクリアです。\nゴールを目指してがんばってください！",
            waitFor = TutorialTrigger.None,
        },
    };

    [Header("表示設定")]
    [Tooltip("他の UI より手前に出すための描画順")]
    [SerializeField] private int sortingOrder = 100;

    [Tooltip("メッセージ表示中に Time.timeScale = 0 でゲームを止める")]
    [SerializeField] private bool pauseWhileShowing = true;

    [Tooltip("OK ボタンのラベル")]
    [SerializeField] private string okButtonLabel = "OK";

    [Tooltip("OK ボタンの下に出す操作ヒント。空にすると表示しない")]
    [SerializeField] private string hintLabel = "Enter キー / OK をタップで次へ";

    [Header("操作")]
    [Tooltip("キーボードの Enter でも OK と同じ動作をする")]
    [SerializeField] private bool submitWithEnterKey = true;

    [Tooltip("表示中だけマウスカーソルのロックを解除する（PC でクリックできるようにするため）")]
    [SerializeField] private bool unlockCursorWhileShowing = true;

    // 実行時に生成する UI
    private GameObject panelRoot;
    private TextMeshProUGUI messageText;

    // 進行状態
    private int stepIndex;
    private bool isWaitingForTrigger; // メッセージを閉じて、次に進む条件を待っている間だけ true
    private int lastHealth;
    private bool isFinished;
    private bool isShowingMessage;

    // 表示前のカーソル状態（閉じるときに戻す）
    private bool isCursorOverridden;
    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;

    private void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        BuildUI();
        HidePanel();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
        }
        PlayerController.OnItemCollected += HandleItemCollected;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
        }
        PlayerController.OnItemCollected -= HandleItemCollected;

        // 表示途中でシーン遷移されても、時間やカーソルが止まったままにならないようにする
        ResumeGame();
        RestoreCursor();
    }

    private void Update()
    {
        // Time.timeScale = 0 でも Update は回るので、ここで Enter を拾う
        if (!isShowingMessage || !submitWithEnterKey)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            OnOkButtonPressed();
        }
    }

    private void Start()
    {
        if (playerHealth != null)
        {
            lastHealth = playerHealth.health;
        }

        if (steps == null || steps.Length == 0)
        {
            Debug.LogWarning("TutorialManager: steps が空のためチュートリアルを開始しません。");
            isFinished = true;
            return;
        }

        ShowStep(0);
    }

    // ─── 進行 ────────────────────────────────────────────

    private void ShowStep(int index)
    {
        stepIndex = index;
        isWaitingForTrigger = false;
        isShowingMessage = true;

        messageText.text = steps[index].message;
        panelRoot.SetActive(true);

        PauseGame();
        UnlockCursor();
    }

    /// <summary>OK ボタンまたは Enter が押されたとき。ゲームを再開し、条件付きなら待機状態に入る。</summary>
    private void OnOkButtonPressed()
    {
        HidePanel();
        ResumeGame();
        RestoreCursor();

        if (steps[stepIndex].waitFor == TutorialTrigger.None)
        {
            AdvanceToNextStep();
        }
        else
        {
            // 条件が満たされるまでプレイヤーに操作させる
            isWaitingForTrigger = true;
        }
    }

    private void AdvanceToNextStep()
    {
        isWaitingForTrigger = false;

        int nextIndex = stepIndex + 1;
        if (nextIndex < steps.Length)
        {
            ShowStep(nextIndex);
        }
        else
        {
            Finish();
        }
    }

    private void Finish()
    {
        isFinished = true;
        isWaitingForTrigger = false;
        HidePanel();
        ResumeGame();
        RestoreCursor();
    }

    // ─── 条件の判定 ──────────────────────────────────────

    private void HandleHealthChanged(int current, int max)
    {
        int previous = lastHealth;
        lastHealth = current;

        if (isFinished || !isWaitingForTrigger)
            return;

        // 体力が減っていたら被弾とみなす
        if (steps[stepIndex].waitFor == TutorialTrigger.PlayerDamaged && current < previous)
        {
            AdvanceToNextStep();
        }
    }

    private void HandleItemCollected(string itemName)
    {
        if (isFinished || !isWaitingForTrigger)
            return;

        TutorialStep step = steps[stepIndex];
        if (step.waitFor == TutorialTrigger.ItemCollected && step.itemName == itemName)
        {
            AdvanceToNextStep();
        }
    }

    // ─── 時間停止 ────────────────────────────────────────

    private void PauseGame()
    {
        if (pauseWhileShowing)
        {
            Time.timeScale = 0f;
        }
    }

    private void ResumeGame()
    {
        if (pauseWhileShowing)
        {
            Time.timeScale = 1f;
        }
    }

    // ─── カーソル ────────────────────────────────────────
    // ゲーム中は PlayerController がカーソルをロックして非表示にしているため、
    // そのままだと PC で OK ボタンをクリックできない。表示中だけ解除する。

    private void UnlockCursor()
    {
        if (!unlockCursorWhileShowing || isCursorOverridden)
            return;

        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        isCursorOverridden = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestoreCursor()
    {
        if (!isCursorOverridden)
            return;

        Cursor.lockState = previousCursorLockState;
        Cursor.visible = previousCursorVisible;
        isCursorOverridden = false;
    }

    // ─── UI の組み立て ───────────────────────────────────
    // シーン側に UI を用意しなくても動くように、実行時にコードで生成する。

    private void BuildUI()
    {
        EnsureEventSystem();

        GameObject canvasObject = new GameObject(
            "TutorialCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // 画面全体を暗くする背景。裏側の UI（ジャンプボタンなど）へのタップも吸収する
        panelRoot = CreateUIObject("TutorialPanel", canvasObject.transform);
        Image dimmer = panelRoot.AddComponent<Image>();
        dimmer.color = new Color(0f, 0f, 0f, 0.6f);
        Stretch(dimmer.rectTransform);

        // メッセージボックス
        GameObject box = CreateUIObject("MessageBox", panelRoot.transform);
        Image boxImage = box.AddComponent<Image>();
        boxImage.color = new Color(0.07f, 0.08f, 0.12f, 0.96f);
        RectTransform boxRect = boxImage.rectTransform;
        boxRect.anchorMin = new Vector2(0.5f, 0.5f);
        boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.pivot = new Vector2(0.5f, 0.5f);
        boxRect.sizeDelta = new Vector2(1100f, 480f);
        boxRect.anchoredPosition = Vector2.zero;

        // メッセージ本文
        GameObject textObject = CreateUIObject("MessageText", box.transform);
        messageText = textObject.AddComponent<TextMeshProUGUI>();
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.textWrappingMode = TextWrappingModes.Normal;
        messageText.fontSize = 44f;
        messageText.color = Color.white;
        if (fontAsset != null)
        {
            messageText.font = fontAsset;
        }
        RectTransform textRect = messageText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(60f, 196f);  // 下側は OK ボタンとヒントの分だけ空ける
        textRect.offsetMax = new Vector2(-60f, -50f);

        // OK ボタン
        GameObject buttonObject = CreateUIObject("OkButton", box.transform);
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.22f, 0.55f, 0.95f, 1f);
        RectTransform buttonRect = buttonImage.rectTransform;
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.sizeDelta = new Vector2(320f, 96f);
        buttonRect.anchoredPosition = new Vector2(0f, 44f);

        Button okButton = buttonObject.AddComponent<Button>();
        okButton.targetGraphic = buttonImage;
        okButton.onClick.AddListener(OnOkButtonPressed);

        GameObject buttonLabelObject = CreateUIObject("Label", buttonObject.transform);
        TextMeshProUGUI buttonLabel = buttonLabelObject.AddComponent<TextMeshProUGUI>();
        buttonLabel.text = okButtonLabel;
        buttonLabel.alignment = TextAlignmentOptions.Center;
        buttonLabel.fontSize = 40f;
        buttonLabel.color = Color.white;
        if (fontAsset != null)
        {
            buttonLabel.font = fontAsset;
        }
        Stretch(buttonLabel.rectTransform);

        // 操作ヒント（OK ボタンのすぐ上）
        if (!string.IsNullOrEmpty(hintLabel))
        {
            GameObject hintObject = CreateUIObject("HintText", box.transform);
            TextMeshProUGUI hintText = hintObject.AddComponent<TextMeshProUGUI>();
            hintText.text = hintLabel;
            hintText.alignment = TextAlignmentOptions.Center;
            hintText.fontSize = 28f;
            hintText.color = new Color(1f, 1f, 1f, 0.7f);
            if (fontAsset != null)
            {
                hintText.font = fontAsset;
            }
            RectTransform hintRect = hintText.rectTransform;
            hintRect.anchorMin = new Vector2(0f, 0f);
            hintRect.anchorMax = new Vector2(1f, 0f);
            hintRect.pivot = new Vector2(0.5f, 0f);
            hintRect.sizeDelta = new Vector2(-120f, 36f);
            hintRect.anchoredPosition = new Vector2(0f, 150f);
        }
    }

    private void HidePanel()
    {
        isShowingMessage = false;

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    /// <summary>ボタンのクリック判定に EventSystem が要るので、シーンに無ければ作る。</summary>
    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
        eventSystemObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
