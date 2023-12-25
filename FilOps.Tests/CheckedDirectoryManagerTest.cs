using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilOps.ViewModels.ExplorerPage;

namespace FilOps.Tests
{
    public class CheckedDirectoryManagerTest
    {
        /// <summary>
        /// 単純に "C:\aaa" をサブディレクトリ含む形で追加して "C:\aaa" を保持しているか確認
        /// </summary>
        [Fact]
        public void CheckHasSimpleWithSubDirectoryTest()
        {
            var path = @"C:\";
            var dirManager = new CheckedDirectoryManager();

            var aaa = path + "aaa";
            dirManager.AddDirectoryWithSubdirectories(aaa);

            Assert.True(dirManager.HasDirectory(aaa));
            Assert.True(dirManager.HasDirectoriesWithSubdirectories(aaa));
        }

        /// <summary>
        /// 単純に "C:\aaa" をサブディレクトリ含まない形で追加して "C:\aaa" を保持しているか確認
        /// </summary>
        [Fact]
        public void CheckHasSimpleOnlyDirectoryTest()
        {
            var path = @"C:\";
            var dirManager = new CheckedDirectoryManager();

            var aaa = path + "aaa";
            dirManager.AddDirectoryOnly(aaa);

            Assert.True(dirManager.HasDirectory(aaa));
            Assert.True(dirManager.HasDirectoriesOnly(aaa));
        }


        /// <summary>
        /// 単純に "C:\aaa" と "C:\aaa\bbb" をサブディレクトリ含む形で追加して "C:\aaa\bbb" が含まれない事を確認
        /// </summary>
        [Fact]
        public void AddSimpleWithSubDirectoryTest()
        {
            var path = @"C:\";
            var dirManager = new CheckedDirectoryManager();

            var aaa = path + "aaa";
            var bbb = Path.Combine(aaa, "bbb");

            dirManager.AddDirectoryWithSubdirectories(bbb);
            dirManager.AddDirectoryWithSubdirectories(aaa);

            Assert.True(dirManager.DirectoriesWithSubdirectories.Where(dir => dir == aaa).Any());
            Assert.False(dirManager.DirectoriesWithSubdirectories.Where(dir => dir == bbb).Any());
        }

        /// <summary>
        /// 単純に "C:\aaa" と "C:\aaa\bbb" をサブディレクトリ含む形で追加して "C:\aaa" を削除したら "C:\aaa\bbb" が含まれない事を確認
        /// </summary>
        [Fact]
        public void RemoveSubDirectoryTest()
        {
            var path = @"C:\";
            var dirManager = new CheckedDirectoryManager();

            var aaa = path + "aaa";
            var bbb = Path.Combine(aaa, "bbb");

            dirManager.AddDirectoryWithSubdirectories(aaa);
            dirManager.AddDirectoryWithSubdirectories(bbb);

            dirManager.RemoveDirectory(aaa);

            Assert.False(dirManager.HasDirectory(aaa));
        }

        /// <summary>
        /// "C:\FLAT", "C:\FLAT\bbb", "C:\FLAT\REBELLIONS" を追加して、
        /// "C:\FLAT\ccc" が削除された時に "C:\FLAT" と "C:\FLAT\REBELLIONS" が監視対象になっているか確認
        /// </summary>
        [Fact]
        public void RemoveSimpleTest()
        {
            var path = @"C:\";
            var dirManager = new CheckedDirectoryManager();

            var aaa = Path.Combine(path, "FLAT");
            var bbb = Path.Combine(aaa, "bbb");
            var ccc = Path.Combine(aaa, "REBELLIONS");

            dirManager.AddDirectoryWithSubdirectories(aaa);
            dirManager.AddDirectoryWithSubdirectories(bbb);
            dirManager.AddDirectoryWithSubdirectories(ccc);

            dirManager.RemoveDirectory(bbb);

            Assert.True(dirManager.HasDirectory(aaa));
            Assert.True(dirManager.HasDirectory(ccc));
        }

        /// <summary>
        /// "C:\FLAT", "C:\FLAT\bbb", "C:\FLAT\REBELLIONS" を追加して、
        /// "C:\aaa\REBELLIONS" が削除された時に "C:\FLAT" が単独監視になっているか確認
        /// "C:\FLAT\REBELLIONS" は単独監視ではない
        /// </summary>
        [Fact]
        public void RemoveOnlyTest()
        {
            var path = @"C:\";
            var dirManager = new CheckedDirectoryManager();

            var aaa = Path.Combine(path, "FLAT");
            var bbb = Path.Combine(aaa, "bbb");
            var ccc = Path.Combine(aaa, "REBELLIONS");

            dirManager.AddDirectoryWithSubdirectories(aaa);
            dirManager.AddDirectoryWithSubdirectories(bbb);
            dirManager.AddDirectoryWithSubdirectories(ccc);

            dirManager.RemoveDirectory(bbb);

            Assert.True(dirManager.HasDirectoriesOnly(aaa));
            Assert.False(dirManager.HasDirectoriesOnly(bbb));
            Assert.True(dirManager.HasDirectoriesWithSubdirectoriesRoot(ccc));

        }
    }
}
