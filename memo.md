# �J������

## �e�E�B���h�E/�y�[�W�ɑ΂��Ă�邱��

### �t�H���g���ւ��Ƃ���

```xml
FontFamily="{Binding UsingFont}" FontSize="{Binding FontSize}"
```

### ViewModel �ł̃o�C���f�B���O

```cs
    /// <summary>
    /// �t�H���g�̐ݒ�
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
    /// �t�H���g�T�C�Y�̐ݒ�
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

### �R���X�g���N�^�Ńt�H���g�֘A�ύX���b�Z�[�W�̎�M�ݒ�

```cs
    // ���C���E�B���h�E����̃t�H���g�ύX���b�Z�[�W��M
    WeakReferenceMessenger.Default.Register<FontChanged>(this, (_, message) => UsingFont = message.UsingFont);

    // ���C���E�B���h�E����̃t�H���g�T�C�Y�ύX���b�Z�[�W��M
    WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, message) => FontSize = message.FontSize);
```

## ���b�Z�[�W����M�T���v��

- ���b�Z�[�W���M�T���v��

```cs
    WeakReferenceMessenger.Default.Send(new FontChanged(value));
```

- ���b�Z�[�W��M�T���v��

```cs
    WeakReferenceMessenger.Default.Register<FontChanged>(this, (recipient, message) =>
    {
        FontSize = message.FontSize;
    });
```