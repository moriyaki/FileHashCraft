namespace FileHashCraft.Services.Messages
{
    public class FileHashCalcFinishedMessage;

    /// <summary>
    /// ハッシュを計算するドライブの送信メッセージ
    /// </summary>
    public class CalcingDriveMessage
    {
        public HashSet<string> Drives { get; set; } = [];

        public CalcingDriveMessage()
        { throw new NotImplementedException(nameof(StartCalcingFileMessage)); }

        public CalcingDriveMessage(HashSet<string> drives)
        {
            Drives = drives;
        }
    }

    /// <summary>
    /// ファイルハッシュ計算開始の送信メッセージ
    /// </summary>
    public class StartCalcingFileMessage
    {
        public string CalcingFile { get; set; } = string.Empty;

        public StartCalcingFileMessage()
        { throw new NotImplementedException(nameof(StartCalcingFileMessage)); }

        public StartCalcingFileMessage(string calcingFile)
        {
            CalcingFile = calcingFile;
        }
    }

    /// <summary>
    /// ファイルハッシュ計算終了の送信メッセージ
    /// </summary>
    public class EndCalcingFileMessage
    {
        public string CalcingFile { get; set; } = string.Empty;

        public EndCalcingFileMessage()
        { throw new NotImplementedException(nameof(EndCalcingFileMessage)); }

        public EndCalcingFileMessage(string calcingFile)
        {
            CalcingFile = calcingFile;
        }
    }

    /// <summary>
    /// 全てのファイルハッシュ計算終了の送信メッセージ
    /// </summary>
    public class FinishedCalcingFileMessage;
}