/// <summary>
/// チュートリアルの進行状態（どのメッセージを出しているか・次に進む条件を待っているか）を持つロジック部分。
///
/// MonoBehaviour から切り離してあるため、Unity のライフサイクルを動かさずに
/// EditMode テストから直接検証できる。UI と時間停止の制御は <see cref="TutorialManager"/> 側の担当。
/// </summary>
public class TutorialProgress
{
    private readonly TutorialManager.TutorialStep[] steps;

    /// <summary>いま対象になっているステップの番号。</summary>
    public int CurrentIndex { get; private set; }

    /// <summary>メッセージを表示している最中かどうか。</summary>
    public bool IsShowingMessage { get; private set; }

    /// <summary>メッセージを閉じて、次へ進む条件（被弾・アイテム取得）を待っているかどうか。</summary>
    public bool IsWaitingForTrigger { get; private set; }

    /// <summary>すべてのステップを終えたかどうか。</summary>
    public bool IsFinished { get; private set; }

    /// <summary>いま対象になっているステップ。開始前・完了後は null。</summary>
    public TutorialManager.TutorialStep CurrentStep
    {
        get
        {
            if (steps == null || CurrentIndex < 0 || CurrentIndex >= steps.Length)
                return null;

            return steps[CurrentIndex];
        }
    }

    public TutorialProgress(TutorialManager.TutorialStep[] steps)
    {
        this.steps = steps;
    }

    /// <summary>
    /// チュートリアルを開始する。
    /// </summary>
    /// <returns>最初のメッセージを表示すべきなら true。ステップが空なら false（そのまま完了扱い）。</returns>
    public bool Begin()
    {
        if (steps == null || steps.Length == 0)
        {
            IsFinished = true;
            return false;
        }

        CurrentIndex = 0;
        IsShowingMessage = true;
        IsWaitingForTrigger = false;
        return true;
    }

    /// <summary>
    /// 表示中のメッセージを OK で閉じる。
    /// 条件なしのステップならそのまま次のメッセージへ進み、条件つきなら待機状態に入る。
    /// </summary>
    /// <returns>続けて次のメッセージを表示すべきなら true。</returns>
    public bool ConfirmCurrentStep()
    {
        if (!IsShowingMessage)
            return false;

        IsShowingMessage = false;

        if (CurrentStep.waitFor == TutorialManager.TutorialTrigger.None)
        {
            return AdvanceToNextStep();
        }

        // 条件が満たされるまでプレイヤーに操作させる
        IsWaitingForTrigger = true;
        return false;
    }

    /// <summary>プレイヤーが被弾したことを通知する。</summary>
    /// <returns>次のメッセージを表示すべきなら true。</returns>
    public bool NotifyPlayerDamaged()
    {
        if (!CanAcceptTrigger(TutorialManager.TutorialTrigger.PlayerDamaged))
            return false;

        return AdvanceToNextStep();
    }

    /// <summary>アイテムを取得したことを通知する。</summary>
    /// <param name="itemName">取得したアイテム名（<c>CollectibleItem.GetItemName()</c> の戻り値）。</param>
    /// <returns>次のメッセージを表示すべきなら true。</returns>
    public bool NotifyItemCollected(string itemName)
    {
        if (!CanAcceptTrigger(TutorialManager.TutorialTrigger.ItemCollected))
            return false;

        if (CurrentStep.itemName != itemName)
            return false;

        return AdvanceToNextStep();
    }

    /// <summary>待機中で、かつ待っている条件の種類が一致しているか。</summary>
    private bool CanAcceptTrigger(TutorialManager.TutorialTrigger trigger)
    {
        if (IsFinished || !IsWaitingForTrigger)
            return false;

        return CurrentStep != null && CurrentStep.waitFor == trigger;
    }

    /// <summary>次のステップへ進む。残りがなければ完了する。</summary>
    /// <returns>次のメッセージを表示すべきなら true。</returns>
    private bool AdvanceToNextStep()
    {
        IsWaitingForTrigger = false;

        int nextIndex = CurrentIndex + 1;
        if (nextIndex >= steps.Length)
        {
            IsShowingMessage = false;
            IsFinished = true;
            return false;
        }

        CurrentIndex = nextIndex;
        IsShowingMessage = true;
        return true;
    }
}
