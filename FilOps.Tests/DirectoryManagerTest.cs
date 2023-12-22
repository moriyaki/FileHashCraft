using System.Diagnostics;
using System.Linq;
using FilOps.ViewModels;
using FilOps.ViewModels.ExplorerPage;

namespace FilOps.Tests
{
    public class ModelTest
    {
        [Fact]
        public void AddRemoveSimpleTest()
        {
            var path = @"C:\";
            var dirManager = new ExpandedDirectoryManager();

            dirManager.AddDirectory(path + "aaa");
            dirManager.AddDirectory(path + "bbb");
            dirManager.AddDirectory(path + "ccc");

            dirManager.AddDirectory(path + "bbb");
            Assert.Equal(3, dirManager.Directories.Count);

            dirManager.RemoveDirectory(path + "bbb");
            Assert.Equal(2, dirManager.Directories.Count);
        }

        [Fact]
        public void AddRemoveRangeTest()
        {
            var dirManager = new ExpandedDirectoryManager();

            var dirs = new string[] { @"C:\aaa", @"C:\bbb", @"C:\ccc" };
            dirManager.AddDirectory(dirs);

            Assert.Equal(3, dirManager.Directories.Count);

            var removeDirs = new string[] { @"C:\bbb", @"C:\ccc" };
            dirManager.RemoveDirectory(removeDirs);

            Assert.Single(dirManager.Directories);
        }
    }
}