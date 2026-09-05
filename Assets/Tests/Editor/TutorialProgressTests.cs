using NUnit.Framework;

/// <summary>
/// <see cref="TutorialProgress"/> の EditMode テスト。
/// Unity のライフサイクルを動かさずに、チュートリアルの進行ルールだけを検証する。
/// </summary>
public class TutorialProgressTests
{
    private const string HealHeart = "Heal Heart";
    private const string JumpJewel = "Jump Up Jewel";

    /// <summary>テスト用のステップを作るヘルパー。</summary>
    private static TutorialManager.TutorialStep Step(
        string message,
        TutorialManager.TutorialTrigger waitFor = TutorialManager.TutorialTrigger.None,
        string itemName = null)
    {
        return new TutorialManager.TutorialStep
        {
            message = message,
            waitFor = waitFor,
            itemName = itemName,
        };
    }

    // ─── Begin ───────────────────────────────────────────

    [Test]
    public void Begin_WithSteps_ShowsFirstMessage()
    {
        var progress = new TutorialProgress(new[] { Step("1つめ"), Step("2つめ") });

        Assert.IsTrue(progress.Begin(), "最初のメッセージを表示するので true になるはず");
        Assert.AreEqual(0, progress.CurrentIndex);
        Assert.AreEqual("1つめ", progress.CurrentStep.message);
        Assert.IsTrue(progress.IsShowingMessage);
        Assert.IsFalse(progress.IsFinished);
    }

    [Test]
    public void Begin_WithEmptySteps_FinishesImmediately()
    {
        var progress = new TutorialProgress(new TutorialManager.TutorialStep[0]);

        Assert.IsFalse(progress.Begin(), "表示するメッセージがないので false になるはず");
        Assert.IsTrue(progress.IsFinished);
        Assert.IsFalse(progress.IsShowingMessage);
    }

    [Test]
    public void Begin_WithNullSteps_FinishesImmediately()
    {
        var progress = new TutorialProgress(null);

        Assert.IsFalse(progress.Begin());
        Assert.IsTrue(progress.IsFinished);
        Assert.IsNull(progress.CurrentStep);
    }

    // ─── OK ボタン（ConfirmCurrentStep） ──────────────────

    [Test]
    public void ConfirmCurrentStep_WithoutTrigger_ShowsNextMessage()
    {
        var progress = new TutorialProgress(new[] { Step("1つめ"), Step("2つめ") });
        progress.Begin();

        Assert.IsTrue(progress.ConfirmCurrentStep(), "条件なしなので続けて次を表示するはず");
        Assert.AreEqual(1, progress.CurrentIndex);
        Assert.AreEqual("2つめ", progress.CurrentStep.message);
        Assert.IsTrue(progress.IsShowingMessage);
    }

    [Test]
    public void ConfirmCurrentStep_WithTrigger_WaitsForTrigger()
    {
        var progress = new TutorialProgress(new[]
        {
            Step("当たってみよう", TutorialManager.TutorialTrigger.PlayerDamaged),
            Step("ライフが減ります"),
        });
        progress.Begin();

        Assert.IsFalse(progress.ConfirmCurrentStep(), "条件待ちに入るのでメッセージは閉じたまま");
        Assert.IsFalse(progress.IsShowingMessage, "UI を閉じてプレイヤーに操作させる");
        Assert.IsTrue(progress.IsWaitingForTrigger);
        Assert.AreEqual(0, progress.CurrentIndex, "条件が満たされるまでステップは進まない");
    }

    [Test]
    public void ConfirmCurrentStep_OnLastStep_Finishes()
    {
        var progress = new TutorialProgress(new[] { Step("これで最後") });
        progress.Begin();

        Assert.IsFalse(progress.ConfirmCurrentStep());
        Assert.IsTrue(progress.IsFinished);
        Assert.IsFalse(progress.IsShowingMessage);
    }

    [Test]
    public void ConfirmCurrentStep_WhileHidden_IsIgnored()
    {
        // OK 連打などでメッセージ非表示中に呼ばれても、勝手に進まないこと
        var progress = new TutorialProgress(new[]
        {
            Step("当たってみよう", TutorialManager.TutorialTrigger.PlayerDamaged),
            Step("ライフが減ります"),
        });
        progress.Begin();
        progress.ConfirmCurrentStep(); // 条件待ちに入る

        Assert.IsFalse(progress.ConfirmCurrentStep());
        Assert.AreEqual(0, progress.CurrentIndex);
        Assert.IsTrue(progress.IsWaitingForTrigger, "待機状態が壊れていないこと");
    }

    // ─── 被弾（NotifyPlayerDamaged） ─────────────────────

    [Test]
    public void NotifyPlayerDamaged_WhileWaiting_ShowsNextMessage()
    {
        var progress = new TutorialProgress(new[]
        {
            Step("当たってみよう", TutorialManager.TutorialTrigger.PlayerDamaged),
            Step("ライフが減ります"),
        });
        progress.Begin();
        progress.ConfirmCurrentStep();

        Assert.IsTrue(progress.NotifyPlayerDamaged());
        Assert.AreEqual(1, progress.CurrentIndex);
        Assert.AreEqual("ライフが減ります", progress.CurrentStep.message);
        Assert.IsTrue(progress.IsShowingMessage);
        Assert.IsFalse(progress.IsWaitingForTrigger);
    }

    [Test]
    public void NotifyPlayerDamaged_WhileMessageIsShowing_IsIgnored()
    {
        // メッセージ表示中（時間停止中）に被弾イベントが飛んできても進まないこと
        var progress = new TutorialProgress(new[]
        {
            Step("当たってみよう", TutorialManager.TutorialTrigger.PlayerDamaged),
            Step("ライフが減ります"),
        });
        progress.Begin();

        Assert.IsFalse(progress.NotifyPlayerDamaged());
        Assert.AreEqual(0, progress.CurrentIndex);
    }

    [Test]
    public void NotifyPlayerDamaged_WhenWaitingForItem_IsIgnored()
    {
        var progress = new TutorialProgress(new[]
        {
            Step("ハートを取ろう", TutorialManager.TutorialTrigger.ItemCollected, HealHeart),
            Step("回復しました"),
        });
        progress.Begin();
        progress.ConfirmCurrentStep();

        Assert.IsFalse(progress.NotifyPlayerDamaged(), "待っている条件の種類が違う");
        Assert.AreEqual(0, progress.CurrentIndex);
    }

    // ─── アイテム取得（NotifyItemCollected） ──────────────

    [Test]
    public void NotifyItemCollected_WithMatchingName_ShowsNextMessage()
    {
        var progress = new TutorialProgress(new[]
        {
            Step("ハートを取ろう", TutorialManager.TutorialTrigger.ItemCollected, HealHeart),
            Step("回復しました"),
        });
        progress.Begin();
        progress.ConfirmCurrentStep();

        Assert.IsTrue(progress.NotifyItemCollected(HealHeart));
        Assert.AreEqual(1, progress.CurrentIndex);
        Assert.IsTrue(progress.IsShowingMessage);
    }

    [Test]
    public void NotifyItemCollected_WithDifferentName_IsIgnored()
    {
        var progress = new TutorialProgress(new[]
        {
            Step("ハートを取ろう", TutorialManager.TutorialTrigger.ItemCollected, HealHeart),
            Step("回復しました"),
        });
        progress.Begin();
        progress.ConfirmCurrentStep();

        Assert.IsFalse(progress.NotifyItemCollected(JumpJewel), "対象外のアイテムでは進まない");
        Assert.AreEqual(0, progress.CurrentIndex);
        Assert.IsTrue(progress.IsWaitingForTrigger);
    }

    // ─── 完了後 ──────────────────────────────────────────

    [Test]
    public void NotifyAfterFinished_IsIgnored()
    {
        var progress = new TutorialProgress(new[] { Step("これで最後") });
        progress.Begin();
        progress.ConfirmCurrentStep();

        Assert.IsTrue(progress.IsFinished);
        Assert.IsFalse(progress.NotifyPlayerDamaged());
        Assert.IsFalse(progress.NotifyItemCollected(HealHeart));
        Assert.IsFalse(progress.ConfirmCurrentStep());
    }

    // ─── 通し ────────────────────────────────────────────

    [Test]
    public void FullScenario_MatchesStage1_1Flow()
    {
        // Stage1_1 の既定シナリオと同じ並びで、最後まで通ることを確認する
        var progress = new TutorialProgress(new[]
        {
            Step("当たってみよう", TutorialManager.TutorialTrigger.PlayerDamaged),
            Step("ライフが減ります"),
            Step("ハートを取ろう", TutorialManager.TutorialTrigger.ItemCollected, HealHeart),
            Step("回復しました"),
            Step("ジュエルを取ろう", TutorialManager.TutorialTrigger.ItemCollected, JumpJewel),
            Step("ジャンプ力が上がります"),
            Step("鍵を取るとクリアです"),
        });

        Assert.IsTrue(progress.Begin());                        // [0] 表示
        Assert.IsFalse(progress.ConfirmCurrentStep());          // OK → 被弾待ち
        Assert.IsTrue(progress.NotifyPlayerDamaged());          // [1] 表示
        Assert.IsTrue(progress.ConfirmCurrentStep());           // OK → [2] 表示
        Assert.IsFalse(progress.ConfirmCurrentStep());          // OK → ハート待ち
        Assert.IsTrue(progress.NotifyItemCollected(HealHeart)); // [3] 表示
        Assert.IsTrue(progress.ConfirmCurrentStep());           // OK → [4] 表示
        Assert.IsFalse(progress.ConfirmCurrentStep());          // OK → ジュエル待ち
        Assert.IsTrue(progress.NotifyItemCollected(JumpJewel)); // [5] 表示
        Assert.IsTrue(progress.ConfirmCurrentStep());           // OK → [6] 表示
        Assert.AreEqual("鍵を取るとクリアです", progress.CurrentStep.message);

        Assert.IsFalse(progress.ConfirmCurrentStep());          // OK → 完了
        Assert.IsTrue(progress.IsFinished);
        Assert.IsFalse(progress.IsShowingMessage);
    }
}
