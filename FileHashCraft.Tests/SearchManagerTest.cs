using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FileHashCraft.Models;
using Moq;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace FileHashCraft.Tests
{
    public class SearchManagerTest
    {
        [Fact]
        public async Task AddConditionTest()
        {
            // Arrange
            var searchFileManagerMock = new Mock<ISearchFileManager>();

            var sm = new SearchConditionsManager(searchFileManagerMock.Object);

            // テストで使用するファイル情報をISearchFileManagerのモックに登録
            var path1 = new HashFile(@"D:\DragonQuest\DQ7\Dragon Quest VII - Eden no Senshitachi (J) (Disc 1).bin");
            var path2 = new HashFile(@"D:\DragonQuest\DQ7\Dragon Quest VII - Eden no Senshitachi (J) (Disc 2).bin");

            searchFileManagerMock.Setup(m => m.AllFiles)
                .Returns(new Dictionary<string, HashFile>
                {
                    { path1.FileFullPath, path1 },
                    { path2.FileFullPath, path2 },
                });

            // Act
            await sm.AddCondition(SearchConditionType.Extention, ".bin");

            var condition = new SearchCondition(SearchConditionType.Extention, ".bin");
            // Assert
            Assert.Equal(2, sm.ConditionFiles[condition].Count);
        }
    }
}
