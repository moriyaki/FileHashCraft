# 開発メモ

## 各ウィンドウ/ページに対してやること

### フォントが関わるところ

```xml
FontFamily="{Binding UsingFont}" FontSize="{Binding FontSize}"
```

### ViewModel でのバインディング

```cs
    /// <summary>
    /// フォントの取得と設定
    /// </summary>
    public FontFamily UsingFont
    {
        get => _MainWindowViewModel.UsingFont;
        set
        {
            _MainWindowViewModel.UsingFont = value;
            OnPropertyChanged(nameof(UsingFont));
        }
    }

    /// <summary>
    /// フォントサイズの取得と設定
    /// </summary>
    public double FontSize
    {
        get => _MainWindowViewModel.FontSize;
        set
        {
            _MainWindowViewModel.FontSize = value;
            OnPropertyChanged(nameof(FontSize));
        }
    }
```

### コンストラクタでフォント関連変更メッセージの受信設定

```cs
    // メインウィンドウからのフォント変更メッセージ受信
    WeakReferenceMessenger.Default.Register<FontChanged>(this, (_, message) => UsingFont = message.UsingFont);

    // メインウィンドウからのフォントサイズ変更メッセージ受信
    WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, message) => FontSize = message.FontSize);
```

## メッセージ送受信サンプル

- メッセージ送信サンプル

```cs
    WeakReferenceMessenger.Default.Send(new FontChanged(value));
```

- メッセージ受信サンプル

```cs
    WeakReferenceMessenger.Default.Register<FontChanged>(this, (recipient, message) =>
    {
        FontSize = message.FontSize;
    });
```

## ファイルハッシュ取得についてのメモ

ハッシュ絞り込み画面→計算画面→削除支援画面

### ハッシュ絞り込み画面( `PageTargetFileSelect` )

ファイルハッシュアルゴリズムをここでも選択できるようにする。
ファイル総数、計算済みファイル総数、要計算ファイル総数、絞り込み結果総数を表示する。

- ツリービュー(左側に表示)

チェックされたフォルダを一覧
その下にnullチェックのフォルダを追加

- ファイル絞り込み(右ペインに表示)

対象拡張子を設定できる

### 詳細設定画面( `ExpertFileSelect` )

- 0サイズファイル、空フォルダを削除対象にするか選べる。
- Hidden ファイル、ReadOnly ファイルを削除対象にするか選べる。
- 絞り込みに正規表現が使える。

### 内部処理

チェックされたフォルダ以下全てのファイルをスキャンする。
nullチェックされたフォルダの全てのファイルをスキャンする。
データ読み込みが行われたら、ハッシュデータを紐付けるする。

### 内部データクラス( HashFile 以下これを内部データとして List<HashFile> を作成する)

- フルパスファイル名
  - 登録時にフォルダとファイル名も内部データとして保持する。
- サイズ
- 更新日時
- 各ハッシュアルゴリズムのハッシュ値

## 計算画面 ( `PageHashCalcing` )

ひたすら計算してその進捗を表示する
自動で同一と思われるファイルのデータを読み込む。

### 計算中
	
アルゴリズム、計算中のファイル名、総処理ファイル数、処理済ファイル数、残ファイル数を表示する。
プログレスバーも？要検討

### 計算後

ハッシュ等から同一ファイルを検出する。
ここでは同一ファイル数を表示する。
中断ボタンも作る。

### 内部処理

HashFile のハッシュ値にぶち込む。
`HashDictionary = Dictionary<string Hash, List<HashFile>>`  形式で、ハッシュ毎に HashFile を管理する。
この時、 `List<HashFile>` に値が入っていたら、同一ファイル数をインクリメントする。

###	HashDirectory クラスの構造

フォルダ名
List<HashFile>

## 削除支援画面 ( `PageFileDelete` )

HashDictionary で List<HashFile> が2個以上のものを抽出する。
次に、この抽出した List<HashFile> から HashDirectory をフォルダごとに作成する。

この重複ファイルを持つフォルダリスト HashDirectory は、左ペインの ListBox にバインドする。
フォルダを選択したら、右下ペインに横並びにリストビューで同一ファイルが表示される。
右上ペインに、チェックの法則を登録する。

###	チェック法則：

左リストビュー、カレントフォルダを優先するか(左リストビューの一括チェックあり)
右リストビュー、他フォルダを優先するか
		
他フォルダを優先する選択肢は

- カスタム
- 同一ディレクトリが多いものから優先
- フルパスが長いものから優先
- フルパスが短いものから優先
- ファイル名が長いものから優先
- ファイル名が短いものから優先
- ドライブレターが先のものから優先
- ドライブレターが後のものから優先

優先で絞り込んだ後、ユーザーが変更をしたら自動で「カスタム」になる

削除ボタンで、チェックしたファイルを削除する(ごみ箱)と同時に、
削除したファイルは HashDictionary(保存処理用) と HashDirectory(削除支援用) から削除する。

この時、ファイルが一つになったらそのファイルも、`HashDictionary` (保存処理用) と `HashDirectory` (削除支援用) から削除する
	
`HashDirectory` (削除支援用) の List が空になったら ListBox からも削除する

