namespace FileHashCraft.Services.Messages
{
    #region カレントディレクトリ変更メッセージ
    /// <summary>
    /// カレントディレクトリのアイテム作成メッセージ
    /// </summary>
    public class CurrentDirectoryItemCreated
    {
        public string CreatedFullPath { get; set; } = string.Empty;
        public CurrentDirectoryItemCreated() { throw new NotImplementedException(nameof(CurrentDirectoryItemCreated)); }
        public CurrentDirectoryItemCreated(string createdFullPath) => CreatedFullPath = createdFullPath;
    }

    /// <summary>
    /// カレントディレクトリのアイテム削除メッセージ
    /// </summary>
    public class CurrentDirectoryItemDeleted
    {
        public string DeletedFullPath { get; set; } = string.Empty;
        public CurrentDirectoryItemDeleted() { throw new NotImplementedException(nameof(CurrentDirectoryItemDeleted)); }
        public CurrentDirectoryItemDeleted(string deletedFullPath) => DeletedFullPath = deletedFullPath;
    }

    /// <summary>
    /// カレントディレクトリのアイテム名前変更メッセージ
    /// </summary>
    public class CurrentDirectoryItemRenamed
    {
        public string OldFullPath { get; set; } = string.Empty;
        public string NewFullPath { get; set; } = string.Empty;
        public CurrentDirectoryItemRenamed() { throw new NotImplementedException(nameof(CurrentDirectoryItemRenamed)); }
        public CurrentDirectoryItemRenamed(string oldFullPath, string newFullPath)
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
    public class DirectoryItemCreated
    {
        public string CreatedFullPath { get; set; } = string.Empty;
        public DirectoryItemCreated() { throw new NotImplementedException(nameof(DirectoryItemCreated)); }
        public DirectoryItemCreated(string createdFullPath) => CreatedFullPath = createdFullPath;
    }

    /// <summary>
    /// ディレクトリのアイテム削除メッセージ
    /// </summary>
    public class DirectoryItemDeleted
    {
        public string DeletedFullPath { get; set; } = string.Empty;
        public DirectoryItemDeleted() { throw new NotImplementedException(nameof(DirectoryItemDeleted)); }
        public DirectoryItemDeleted(string deletedFullPath) => DeletedFullPath = deletedFullPath;
    }

    /// <summary>
    /// ディレクトリのアイテム名前変更メッセージ
    /// </summary>
    public class DirectoryItemRenamed
    {
        public string OldFullPath { get; set; } = string.Empty;
        public string NewFullPath { get; set; } = string.Empty;
        public DirectoryItemRenamed() { throw new NotImplementedException(nameof(DirectoryItemRenamed)); }
        public DirectoryItemRenamed(string oldFullPath, string newFullPath)
        {
            OldFullPath = oldFullPath;
            NewFullPath = newFullPath;
        }
    }

    /// <summary>
    /// リムーバブルドライブの挿入メッセージ
    /// </summary>
    public class OpticalDriveMediaInserted
    {
        public string InsertedPath { get; set; } = string.Empty;
        public OpticalDriveMediaInserted() { throw new NotImplementedException(nameof(OpticalDriveMediaInserted)); }
        public OpticalDriveMediaInserted(string insertedPath) => InsertedPath = insertedPath;
    }

    /// <summary>
    /// リムーバブルドライブのイジェクトメッセージ
    /// </summary>
    public class OpticalDriveMediaEjected
    {
        public string EjectedPath { get; set; } = string.Empty;
        public OpticalDriveMediaEjected() { throw new NotImplementedException(nameof(OpticalDriveMediaEjected)); }
        public OpticalDriveMediaEjected(string ejectedPath) => EjectedPath = ejectedPath;
    }
    #endregion ディレクトリ変更メッセージ
}
