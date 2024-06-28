namespace FileHashCraft.Services.Messages
{
    public class FileHashCalcFinishedMessage;

    public class StartCalcingFile
    {
        public string BeforeFile { get; set; } = string.Empty;
        public string CalcingFile { get; set; } = string.Empty;
        public StartCalcingFile() { throw new NotImplementedException(nameof(StartCalcingFile)); }
        public StartCalcingFile(string beforeFile, string calcingFile)
        {
            BeforeFile = beforeFile;
            CalcingFile = calcingFile;
        }
    }

    public class EndCalcingFile
    {
        public string CalcingFile { get; set; } = string.Empty;
        public EndCalcingFile() { throw new NotImplementedException(nameof(EndCalcingFile)); }
        public EndCalcingFile(string calcingFile)
        {
            CalcingFile = calcingFile;
        }
    }
}
