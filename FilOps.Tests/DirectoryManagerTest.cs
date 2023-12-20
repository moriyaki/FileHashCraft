using System.Diagnostics;
using System.Linq;
using FilOps.Models;
using static FilOps.Models.DirectoryManager;

namespace FilOps.Tests
{
    public class ModelTest
    {
        [Fact]
        public void AddRemoveSimpleTest()
        {
            var path = @"C:\";
            var dirManager = new DirectoryManager(ManagementType.ForWatcher);

            dirManager.AddDirectory(path + "aaa");
            dirManager.AddDirectory(path + "bbb");
            dirManager.AddDirectory(path + "ccc");

            dirManager.AddDirectory(path + "bbb");
            Assert.Equal(3, dirManager.Count());

            dirManager.RemoveDirectory(path + "bbb");
            Assert.Equal(2, dirManager.Count());
        }

        [Fact]
        public void AddRemoveRangeTest()
        {
            var dirManager = new DirectoryManager(ManagementType.ForWatcher);

            var dirs = new string[] { @"C:\aaa", @"C:\bbb", @"C:\ccc" };
            dirManager.AddDirectory(dirs);

            Assert.Equal(3, dirManager.Count());

            var removeDirs = new string[] { @"C:\bbb", @"C:\ccc" };
            dirManager.RemoveDirectory(removeDirs);

            Assert.Equal(1, dirManager.Count());
        }

        [Fact]
        public void AddSubDirTest()
        {
            var path = @"C:\";
            var dirManager = new DirectoryManager(ManagementType.ForChecked);

            dirManager.AddDirectory(path + @"aaa");
            dirManager.AddDirectory(path + @"aaa\bbb");
            dirManager.AddDirectory(path + @"aaa\ccc");
            dirManager.AddDirectory(path + @"aaa\ddd");
            dirManager.AddDirectory(path + @"aaa\ddd\eee");

            Assert.Equal(1, dirManager.Count());

            Assert.True(dirManager.HasDirectory(path + @"aaa\bbb"));

        }
    }
}