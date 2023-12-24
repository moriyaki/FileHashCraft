using System.Diagnostics;
using System.Linq;
using FilOps.Models;
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
            var dirManager = new ExpandedDirectoryManager(new FileSystemInformationManager());

            var aaa = path + "aaa";
            var bbb = path + "bbb";
            var ccc = path + "ccc";

            dirManager.AddDirectory(aaa);
            dirManager.AddDirectory(bbb);
            dirManager.AddDirectory(ccc);

            Assert.True(dirManager.Directories.Where(dir => dir == aaa).Any());
            Assert.True(dirManager.Directories.Where(dir => dir == bbb).Any());
            Assert.True(dirManager.Directories.Where(dir => dir == ccc).Any());


            var count = dirManager.Directories.Count;
            dirManager.AddDirectory(bbb);
            Assert.Equal(count, dirManager.Directories.Count);

            dirManager.RemoveDirectory(bbb);
            Assert.False(dirManager.Directories.Where(dir => dir == bbb).Any());
        }

        [Fact]
        public void SpecialFolder() 
        {
            var path = @"C:\Users\moriyaki\Documents";

            var dirManager = new ExpandedDirectoryManager(new FileSystemInformationManager());
            Assert.True(dirManager.Directories.Where(dir => dir == path).Any());

            dirManager.RemoveDirectory(path);
            Assert.True(dirManager.Directories.Where(dir => dir == path).Any());
        }

        [Fact]
        public void SubDirectory()
        {
            var path = @"C:\";
            var dirManager = new ExpandedDirectoryManager(new FileSystemInformationManager());

            var aaa = path + "aaa";
            var bbb = Path.Combine(aaa + "bbb");

            dirManager.AddDirectory(aaa);
            dirManager.AddDirectory(bbb);

            Assert.True(dirManager.Directories.Where(dir => dir == aaa).Any());
            Assert.True(dirManager.Directories.Where(dir => dir == bbb).Any());

            dirManager.RemoveDirectory(aaa);
            Assert.True(dirManager.Directories.Where(dir => dir == bbb).Any());
        }
    }
}