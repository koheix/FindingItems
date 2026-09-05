using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

/// <summary>
/// EditMode テストを実行し、レビュー者がそのまま読める Markdown のレポートを出力する。
///
/// 【使い方】
///   - Unity Editor のメニュー: Tools > Tests > Run EditMode Tests
///   - スクリプトから: <c>EditorTestReporter.RunEditModeTests()</c>
///     （MCP for Unity の execute_code から呼ぶのはこちら）
///
/// テストは非同期に走るので、実行後しばらくしてから <see cref="ReportPath"/> を読むこと。
/// 出力先は生成物なので .gitignore 済み。レポートの中身は PR 本文に貼り付ける。
/// </summary>
public static class EditorTestReporter
{
    /// <summary>レポートの出力先（プロジェクトルートからの相対パス）。</summary>
    public const string ReportPath = "TestResults/edit-mode-report.md";

    [MenuItem("Tools/Tests/Run EditMode Tests")]
    public static void RunEditModeTests()
    {
        // 走り始めたことが分かるように、前回のレポートを消してから実行する
        if (File.Exists(ReportPath))
        {
            File.Delete(ReportPath);
        }

        TestRunnerApi api = ScriptableObject.CreateInstance<TestRunnerApi>();
        api.RegisterCallbacks(new ReportWriter());
        api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode }));
    }

    /// <summary>テスト完了時にレポートを書き出すコールバック。</summary>
    private class ReportWriter : ICallbacks
    {
        public void RunStarted(ITestAdaptor testsToRun)
        {
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            WriteReport(result);
        }
    }

    private static void WriteReport(ITestResultAdaptor rootResult)
    {
        List<ITestResultAdaptor> leaves = new List<ITestResultAdaptor>();
        CollectLeafResults(rootResult, leaves);

        int passed = 0;
        int failed = 0;
        int skipped = 0;
        foreach (ITestResultAdaptor leaf in leaves)
        {
            switch (leaf.TestStatus)
            {
                case TestStatus.Passed:  passed++;  break;
                case TestStatus.Failed:  failed++;  break;
                default:                 skipped++; break;
            }
        }

        StringBuilder report = new StringBuilder();
        report.AppendLine("## 単体テスト結果 (Unity EditMode)");
        report.AppendLine();
        report.AppendLine("| 項目 | 値 |");
        report.AppendLine("| --- | --- |");
        report.AppendLine("| 実行日時 | " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " |");
        report.AppendLine("| Unity | " + Application.unityVersion + " |");
        report.AppendLine("| 総数 | " + leaves.Count + " |");
        report.AppendLine("| 成功 | " + passed + " |");
        report.AppendLine("| 失敗 | " + failed + " |");
        report.AppendLine("| スキップ | " + skipped + " |");
        report.AppendLine("| 所要時間 | " + rootResult.Duration.ToString("0.000") + " 秒 |");
        report.AppendLine("| 判定 | " + (failed == 0 ? "**PASSED**" : "**FAILED**") + " |");
        report.AppendLine();

        report.AppendLine("<details><summary>テストケース一覧</summary>");
        report.AppendLine();
        report.AppendLine("| 結果 | テスト | 時間 |");
        report.AppendLine("| --- | --- | --- |");
        foreach (ITestResultAdaptor leaf in leaves)
        {
            string mark = leaf.TestStatus == TestStatus.Passed ? "PASS"
                        : leaf.TestStatus == TestStatus.Failed ? "FAIL"
                        : "SKIP";
            report.AppendLine("| " + mark + " | `" + leaf.FullName + "` | "
                              + leaf.Duration.ToString("0.000") + "s |");
        }
        report.AppendLine();
        report.AppendLine("</details>");

        if (failed > 0)
        {
            report.AppendLine();
            report.AppendLine("### 失敗の詳細");
            report.AppendLine();
            foreach (ITestResultAdaptor leaf in leaves)
            {
                if (leaf.TestStatus != TestStatus.Failed)
                    continue;

                report.AppendLine("#### " + leaf.FullName);
                report.AppendLine();
                report.AppendLine("```");
                report.AppendLine(leaf.Message);
                report.AppendLine(leaf.StackTrace);
                report.AppendLine("```");
                report.AppendLine();
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
        File.WriteAllText(ReportPath, report.ToString(), new UTF8Encoding(false));

        Debug.Log("EditorTestReporter: " + (failed == 0 ? "PASSED" : "FAILED")
                  + " (" + passed + "/" + leaves.Count + ") -> " + ReportPath);
    }

    /// <summary>結果ツリーの葉（実際のテストケース）だけを集める。</summary>
    private static void CollectLeafResults(ITestResultAdaptor result, List<ITestResultAdaptor> into)
    {
        if (!result.Test.IsSuite)
        {
            into.Add(result);
            return;
        }

        foreach (ITestResultAdaptor child in result.Children)
        {
            CollectLeafResults(child, into);
        }
    }
}
