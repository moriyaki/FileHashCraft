# �J������

## �e�E�B���h�E/�y�[�W�ɑ΂��Ă�邱��

### �t�H���g���ւ��Ƃ���

```xml
FontFamily="{Binding UsingFont}" FontSize="{Binding FontSize}"
```

### ViewModel �ł̃o�C���f�B���O

```cs
    /// <summary>
    /// �t�H���g�̎擾�Ɛݒ�
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
    /// �t�H���g�T�C�Y�̎擾�Ɛݒ�
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

## �t�@�C���n�b�V���擾�ɂ��Ẵ���

�n�b�V���i�荞�݉�ʁ��v�Z��ʁ��폜�x�����

### �n�b�V���i�荞�݉��( `PageTargetFileSelect` )

�t�@�C���n�b�V���A���S���Y���������ł��I���ł���悤�ɂ���B
�t�@�C�������A�v�Z�ς݃t�@�C�������A�v�v�Z�t�@�C�������A�i�荞�݌��ʑ�����\������B

- �c���[�r���[(�����ɕ\��)

�`�F�b�N���ꂽ�t�H���_���ꗗ
���̉���null�`�F�b�N�̃t�H���_��ǉ�

- �t�@�C���i�荞��(�E�y�C���ɕ\��)

�Ώۊg���q��ݒ�ł���

### �ڍאݒ���( `ExpertFileSelect` )

- 0�T�C�Y�t�@�C���A��t�H���_���폜�Ώۂɂ��邩�I�ׂ�B
- Hidden �t�@�C���AReadOnly �t�@�C�����폜�Ώۂɂ��邩�I�ׂ�B
- �i�荞�݂ɐ��K�\�����g����B

### ��������

�`�F�b�N���ꂽ�t�H���_�ȉ��S�Ẵt�@�C�����X�L��������B
null�`�F�b�N���ꂽ�t�H���_�̑S�Ẵt�@�C�����X�L��������B
�f�[�^�ǂݍ��݂��s��ꂽ��A�n�b�V���f�[�^��R�t���邷��B

### �����f�[�^�N���X( HashFile �ȉ����������f�[�^�Ƃ��� List<HashFile> ���쐬����)

- �t���p�X�t�@�C����
  - �o�^���Ƀt�H���_�ƃt�@�C�����������f�[�^�Ƃ��ĕێ�����B
- �T�C�Y
- �X�V����
- �e�n�b�V���A���S���Y���̃n�b�V���l

## �v�Z��� ( `PageHashCalcing` )

�Ђ�����v�Z���Ă��̐i����\������
�����œ���Ǝv����t�@�C���̃f�[�^��ǂݍ��ށB

### �v�Z��
	
�A���S���Y���A�v�Z���̃t�@�C�����A�������t�@�C�����A�����σt�@�C�����A�c�t�@�C������\������B
�v���O���X�o�[���H�v����

### �v�Z��

�n�b�V�������瓯��t�@�C�������o����B
�����ł͓���t�@�C������\������B
���f�{�^�������B

### ��������

HashFile �̃n�b�V���l�ɂԂ����ށB
`HashDictionary = Dictionary<string Hash, List<HashFile>>`  �`���ŁA�n�b�V������ HashFile ���Ǘ�����B
���̎��A `List<HashFile>` �ɒl�������Ă�����A����t�@�C�������C���N�������g����B

###	HashDirectory �N���X�̍\��

�t�H���_��
List<HashFile>

## �폜�x����� ( `PageFileDelete` )

HashDictionary �� List<HashFile> ��2�ȏ�̂��̂𒊏o����B
���ɁA���̒��o���� List<HashFile> ���� HashDirectory ���t�H���_���Ƃɍ쐬����B

���̏d���t�@�C�������t�H���_���X�g HashDirectory �́A���y�C���� ListBox �Ƀo�C���h����B
�t�H���_��I��������A�E���y�C���ɉ����тɃ��X�g�r���[�œ���t�@�C�����\�������B
�E��y�C���ɁA�`�F�b�N�̖@����o�^����B

###	�`�F�b�N�@���F

�����X�g�r���[�A�J�����g�t�H���_��D�悷�邩(�����X�g�r���[�̈ꊇ�`�F�b�N����)
�E���X�g�r���[�A���t�H���_��D�悷�邩
		
���t�H���_��D�悷��I������

- �J�X�^��
- ����f�B���N�g�����������̂���D��
- �t���p�X���������̂���D��
- �t���p�X���Z�����̂���D��
- �t�@�C�������������̂���D��
- �t�@�C�������Z�����̂���D��
- �h���C�u���^�[����̂��̂���D��
- �h���C�u���^�[����̂��̂���D��

�D��ōi�荞�񂾌�A���[�U�[���ύX�������玩���Łu�J�X�^���v�ɂȂ�

�폜�{�^���ŁA�`�F�b�N�����t�@�C�����폜����(���ݔ�)�Ɠ����ɁA
�폜�����t�@�C���� HashDictionary(�ۑ������p) �� HashDirectory(�폜�x���p) ����폜����B

���̎��A�t�@�C������ɂȂ����炻�̃t�@�C�����A`HashDictionary` (�ۑ������p) �� `HashDirectory` (�폜�x���p) ����폜����
	
`HashDirectory` (�폜�x���p) �� List ����ɂȂ����� ListBox ������폜����

