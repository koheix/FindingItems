## 概要

<!-- 何を・なぜ変えたのかを 1〜3 行で -->

## 変更内容

<!-- 箇条書き。スクリプト / シーン / プレハブ / 設定 のどれを触ったか分かるように -->

-

## 単体テスト結果

<!--
  必須。TestResults/edit-mode-report.md の中身をそのまま貼ってください。
  生成方法: Unity Editor の Tools > Tests > Run EditMode Tests
  詳細: docs/testing.md
-->

## 人間に確認してほしいこと

自動テストで担保できない項目です。Unity Editor / 実機で確認してください。

- [ ] コンパイルエラー・警告が出ていない
- [ ] 確認シーン: <!-- 例: Stage1_1 -->
- [ ] 確認内容: <!-- 例: アイテム取得で HP が回復し、HealthUI が更新される -->
- [ ] タッチ操作（モバイル / Device Simulator）でも動作する <!-- 入力に関わる変更の場合 -->

## スクリーンショット / 動画

<!-- 見た目の変更がある場合は貼ってください -->

## チェックリスト

- [ ] `docs/unity-project-structure.md` の構成に従ってファイルを配置した
- [ ] 追加・削除・移動したファイルの `.meta` を過不足なくコミットした
- [ ] 生成物（`Library/`, `Temp/`, `UserSettings/` など）を含めていない
- [ ] 今回の作業と無関係な差分（`PlayFabEditorPrefsSO.asset` など）を含めていない
- [ ] ロジックを変更した場合、`Assets/Tests/Editor/` のテストを追加・更新した
- [ ] 単体テストを実行し、結果を上に貼った（全件 PASSED）
- [ ] ベースブランチは `devel`（リリース PR の場合のみ `main`）
