using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
