---
description: 規約に沿った場所に新しい MonoBehaviour スクリプトを作成する
---

新しいスクリプトを作成してください。対象: $ARGUMENTS

手順:

1. `docs/unity-project-structure.md` の表を参照し、機能に対応する
   `Assets/Scripts/<機能>Scripts/` を決める。該当フォルダが無ければ新設し、
   `docs/unity-project-structure.md` の表にも 1 行追記する。
2. `docs/coding-guidelines.md` に従って実装する。特に:
   - インスペクター公開は `[SerializeField] private` + `[Tooltip]`（単位まで書く）
   - コメントは日本語、識別子は英語
   - 中身のない `Start()` / `Update()` は残さない
   - UniTask は使わない（未導入）。待機処理は Coroutine で書く
3. 既存の似たスクリプトを読み、スタイルを合わせる。
4. **`.meta` ファイルは作らない。** 作成後、ユーザーに次のように伝える:
   「Unity Editor を開いて `.meta` を生成し、コンパイルエラーがないか確認してください。
   その後 `/commit` でコミットできます。」
