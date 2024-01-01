# 開発メモ

## 各ウィンドウ/ページに対してやること

### フォントが関わるところ

```xml
FontFamily="{Binding UsingFont}" FontSize="{Binding FontSize}"
```

### ViewModel でのバインディング

```cs
    /// <summary>
    /// フォントの設定
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
    /// フォントサイズの設定
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