# General Coding Guidelines (C# / Windows Forms)

## 命名規則
- メソッド名、プロパティ名、クラス名は **PascalCase**。
- 変数名、フィールド名は **camelCase**。
- 定数は **大文字 + アンダースコア区切り**。
- UIイベントハンドラは `{ControlName}_{EventName}` とする。
  - 例: `buttonSave_Click`, `textBoxInput_TextChanged`

## コーディングスタイル
- 必ず `var` ではなく明示的な型を使用（例: `string text = "Hello";`）。
- 例外処理には `try-catch-finally` を使用。
- ファイルの先頭に `using System;` 等を整理。

## コメント
- 日本語で簡潔に書く。
- 関数・クラスにはXMLドキュメントコメント（`///`）を使用。

# Documentation & Comments

- すべての **public メンバー** に XML コメント (`///`) を付与。
- コメントは簡潔で、英語で要点のみを記述。
- 自動生成された Designer ファイルにはコメントを追加しない。
- 例：
  ```csharp
  /// <summary>
  /// ユーザー情報を読み込む
  /// </summary>
  /// <param name="userId">対象ユーザーのID</param>
  /// <returns>ユーザー情報を格納したオブジェクト</returns>
  public User LoadUser(int userId) { ... }

  # Backend / Business Logic Guidelines

- データアクセスには `using` ブロックを使用し、リソースリークを防止。