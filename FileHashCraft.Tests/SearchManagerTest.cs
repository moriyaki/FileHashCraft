using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FileHashCraft.Models;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace FileHashCraft.Tests
{
    public class SearchManagerTest
    {
        private readonly SearchConditionsManager sm = new();

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
        public async Task AddContidionTest()
        {
            SetFilesDQ();
            await sm.AddCondition(SearchConditionType.Extention, ".bin");

            Assert.Equal(2, sm.AllConditionFiles.Count);
        }

        [Fact]
        public async Task RemoveContidionTest()
        {
            SetFilesDQ();
            await sm.AddCondition(SearchConditionType.Extention, ".bin");
            await sm.AddCondition(SearchConditionType.Extention, ".iso");
            await sm.RemoveCondition(SearchConditionType.Extention, ".bin");

            Assert.Single(sm.AllConditionFiles);
        }
    }
}
