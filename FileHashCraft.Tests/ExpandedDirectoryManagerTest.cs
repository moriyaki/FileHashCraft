using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.Tests
{
    public class ExpandedDirectoryManagerTest
    {
        [Fact]
        public void AddRemoveSimpleTest()
        {
            const string path = @"C:\";
            var dirManager = new DirectoryTreeExpandedDirectoryManager();

            const string aaa = path + "aaa";
            const string bbb = path + "bbb";
            const string ccc = path + "ccc";

            dirManager.AddDirectory(aaa);
            dirManager.AddDirectory(bbb);
            dirManager.AddDirectory(ccc);

            Assert.Contains(dirManager.Directories, dir => dir == aaa);
            Assert.Contains(dirManager.Directories, dir => dir == bbb);
            Assert.Contains(dirManager.Directories, dir => dir == ccc);

            var count = dirManager.Directories.Count;
            dirManager.AddDirectory(bbb);
            Assert.Equal(count, dirManager.Directories.Count);

            dirManager.RemoveDirectory(bbb);
            Assert.DoesNotContain(dirManager.Directories, dir => dir == bbb);
        }

        [Fact]
        public void SpecialFolder()
        {
            const string path = @"C:\Users\moriyaki\Documents";

            var dirManager = new DirectoryTreeExpandedDirectoryManager();
            Assert.Contains(dirManager.Directories, dir => dir == path);

            dirManager.RemoveDirectory(path);
            Assert.Contains(dirManager.Directories, dir => dir == path);
        }

        [Fact]
        public void SubDirectory()
        {
            const string path = @"C:\";
            var dirManager = new DirectoryTreeExpandedDirectoryManager();

            const string aaa = path + "aaa";
            var bbb = Path.Combine(aaa + "bbb");

            dirManager.AddDirectory(aaa);
            dirManager.AddDirectory(bbb);

            Assert.Contains(dirManager.Directories, dir => dir == aaa);
            Assert.Contains(dirManager.Directories, dir => dir == bbb);

            dirManager.RemoveDirectory(aaa);
            Assert.Contains(dirManager.Directories, dir => dir == bbb);
        }
    }
}