namespace FileHashCraft.Models
{
    public class AddConditionFile
    {
        public HashFile ConditionFiles;
        public AddConditionFile() { throw new NotImplementedException(); }
        public AddConditionFile(HashFile conditionFiles)
        {
            ConditionFiles = conditionFiles;
        }
    }
}
