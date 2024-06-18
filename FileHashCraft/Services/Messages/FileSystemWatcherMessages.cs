namespace FileHashCraft.Services.Messages
{
    #region カレントディレクトリ変更メッセージ
    /// <summary>
    /// カレントディレクトリのアイテム作成メッセージ
    /// </summary>
    public class CurrentDirectoryItemCreatedMessage
    {
        public string CreatedFullPath { get; set; } = string.Empty;
        public CurrentDirectoryItemCreatedMessage() { throw new NotImplementedException(nameof(CurrentDirectoryItemCreatedMessage)); }
        public CurrentDirectoryItemCreatedMessage(string createdFullPath) => CreatedFullPath = createdFullPath;
    }

    /// <summary>
    /// カレントディレクトリのアイテム削除メッセージ
    /// </summary>
    public class CurrentDirectoryItemDeletedMessage
    {
        public string DeletedFullPath { get; set; } = string.Empty;
        public CurrentDirectoryItemDeletedMessage() { throw new NotImplementedException(nameof(CurrentDirectoryItemDeletedMessage)); }
        public CurrentDirectoryItemDeletedMessage(string deletedFullPath) => DeletedFullPath = deletedFullPath;
    }

    /// <summary>
    /// カレントディレクトリのアイテム名前変更メッセージ
    /// </summary>
    public class CurrentDirectoryItemRenamedMessage
    {
        public string OldFullPath { get; set; } = string.Empty;
        public string NewFullPath { get; set; } = string.Empty;
        public CurrentDirectoryItemRenamedMessage() { throw new NotImplementedException(nameof(CurrentDirectoryItemRenamedMessage)); }
        public CurrentDirectoryItemRenamedMessage(string oldFullPath, string newFullPath)
        {
            OldFullPath = oldFullPath;
            NewFullPath = newFullPath;
        }
    }
    #endregion カレントディレクトリ変更メッセージ

    #region ディレクトリ変更メッセージ
    /// <summary>
    /// ディレクトリのアイテム作成メッセージ
    /// </summary>
    public class DirectoryItemCreatedMessage
    {
        public string CreatedFullPath { get; set; } = string.Empty;
        public DirectoryItemCreatedMessage() { throw new NotImplementedException(nameof(DirectoryItemCreatedMessage)); }
        public DirectoryItemCreatedMessage(string createdFullPath) => CreatedFullPath = createdFullPath;
    }

    /// <summary>
    /// ディレクトリのアイテム削除メッセージ
    /// </summary>
    public class DirectoryItemDeletedMessage
    {
        public string DeletedFullPath { get; set; } = string.Empty;
        public DirectoryItemDeletedMessage() { throw new NotImplementedException(nameof(DirectoryItemDeletedMessage)); }
        public DirectoryItemDeletedMessage(string deletedFullPath) => DeletedFullPath = deletedFullPath;
    }

    /// <summary>
    /// ディレクトリのアイテム名前変更メッセージ
    /// </summary>
    public class DirectoryItemRenamedMessage
    {
        public string OldFullPath { get; set; } = string.Empty;
        public string NewFullPath { get; set; } = string.Empty;
        public DirectoryItemRenamedMessage() { throw new NotImplementedException(nameof(DirectoryItemRenamedMessage)); }
        public DirectoryItemRenamedMessage(string oldFullPath, string newFullPath)
        {
            OldFullPath = oldFullPath;
            NewFullPath = newFullPath;
        }
    }

    /// <summary>
    /// リムーバブルドライブの挿入メッセージ
    /// </summary>
    public class OpticalDriveMediaInsertedMessage
    {
        public string InsertedPath { get; set; } = string.Empty;
        public OpticalDriveMediaInsertedMessage() { throw new NotImplementedException(nameof(OpticalDriveMediaInsertedMessage)); }
        public OpticalDriveMediaInsertedMessage(string insertedPath) => InsertedPath = insertedPath;
    }

    /// <summary>
    /// リムーバブルドライブのイジェクトメッセージ
    /// </summary>
    public class OpticalDriveMediaEjectedMessage
    {
        public string EjectedPath { get; set; } = string.Empty;
        public OpticalDriveMediaEjectedMessage() { throw new NotImplementedException(nameof(OpticalDriveMediaEjectedMessage)); }
        public OpticalDriveMediaEjectedMessage(string ejectedPath) => EjectedPath = ejectedPath;
    }
    #endregion ディレクトリ変更メッセージ
}
