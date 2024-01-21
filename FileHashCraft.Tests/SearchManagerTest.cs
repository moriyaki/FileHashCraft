using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FileHashCraft.Models;

namespace FileHashCraft.Tests
{
    public class SearchManagerTest
    {
        private readonly SearchManager sm = new();

        private void SetFilesDQ()
        {
            const string path = @"D:\DragonQuest\";
            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                sm.AddFile(file);
            }
        }

        [Fact]
        public void AddContidionTest()
        {
            SetFilesDQ();
            sm.AddCondition(SearchConditionType.Extention, ".bin");

            Assert.Equal(2, sm.AllConditionFiles.Count);
        }

        [Fact]
        public void RemoveContidionTest()
        {
            SetFilesDQ();
            sm.AddCondition(SearchConditionType.Extention, ".bin");
            sm.AddCondition(SearchConditionType.Extention, ".iso");
            sm.RemoveCondition(SearchConditionType.Extention, ".bin");

            Assert.Single(sm.AllConditionFiles);
        }
    }
}
