using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilOps.Models;

namespace FilOps.Tests
{
    public class CheckedDirectoryManagerTest
    {
        /// <summary>
        /// ルートディレクトリを登録し、単純に登録されているか
        /// </summary>
        [Fact]
        public void TestRootChecked()
        {
            var root = @"C:\";
            var manager = new CheckedDirectoryManager();
            manager.CheckChanged(root, true);

            Assert.True(manager.IsChecked(root));
        }

        /// <summary>
        /// ルートディレクトリとその直下に a ディレクトリを作り
        /// ルートディレクトリを登録したら a も登録されているか
        /// </summary>
        [Fact]
        public void TestSubChecked()
        {
            var root = @"C:\";
            var a = Path.Combine(root, "a");
            var manager = new CheckedDirectoryManager();
            manager.CheckChanged(root, true);

            Assert.True(manager.IsChecked(a));
        }

        /// <summary>
        /// ルートディレクトリとその直下に a ディレクトリを作り
        /// 先に a を登録し、ルートディレクトリを登録したら a はリストに残っていないか
        /// </summary>
        [Fact]
        public void TestSubFirstChecked()
        {
            var root = @"C:\";
            var a = Path.Combine(root, "a");
            var manager = new CheckedDirectoryManager();
            manager.CheckChanged(a, true);
            manager.CheckChanged(root, true);

            Assert.DoesNotContain(a, manager.NestedDirectories);
        }

        /// <summary>
        /// ルートディレクトリとその直下に a ディレクトリを作り
        /// 先に a を登録し、ルートディレクトリを登録したら a はリストに残っていないか
        /// </summary>
        [Fact]
        public void TestRootFirstChecked()
        {
            var root = @"C:\";
            var a = Path.Combine(root, "a");
            var manager = new CheckedDirectoryManager();
            manager.CheckChanged(root, true);
            manager.CheckChanged(a, true);

            Assert.DoesNotContain(a, manager.NestedDirectories);
        }

        /// <summary>
        /// ルートディレクトリとその直下に a ディレクトリを作り
        /// ルートディレクトリを登録したら a は実際に登録されていないか
        /// </summary>
        [Fact]
        public void TestSubNotRegistered()
        {
            var root = @"C:\";
            var a = Path.Combine(root, "a");
            var manager = new CheckedDirectoryManager();
            manager.CheckChanged(root, true);

            Assert.DoesNotContain(manager.NestedDirectories, n => n == a);
        }

        /// <summary>
        /// ルートディレクトリを登録し、登録解除する
        /// </summary>
        [Fact]
        public void TestUnchecked()
        {
            var root = @"C:\";
            var manager = new CheckedDirectoryManager();
            manager.CheckChanged(root, true);
            manager.CheckChanged(root, false);

            Assert.False(manager.IsChecked(root));
        }

        /// <summary>
        /// ルートディレクトリとその直下に a ディレクトリを作り
        /// ルートディレクトリを登録解除したら a も登録されているか
        /// </summary>
        [Fact]
        public void TestSubUnCecked()
        {
            var root = @"C:\";
            var a = Path.Combine(root, "a");
            var manager = new CheckedDirectoryManager();
            manager.CheckChanged(root, false);

            Assert.False(manager.IsChecked(a));
        }

        /// <summary>
        /// ルートディレクトリが混合状態になったら、混合状態としてチェックされているか
        /// </summary>
        [Fact]
        public void TestMixChecked()
        {
            var root = @"C:\";
            var manager = new CheckedDirectoryManager();
            manager.CheckChanged(root, null);

            Assert.True(manager.IsChecked(root));
        }

        /// <summary>
        /// ルートディレクトリが混合状態になったら、混合状態として登録されているか
        /// </summary>
        [Fact]
        public void TestMixExistNotNested()
        {
            var root = @"C:\";
            var manager = new CheckedDirectoryManager();
            manager.CheckChanged(root, null);

            Assert.Contains(root, manager.NonNestedDirectories);
            Assert.DoesNotContain(root, manager.NestedDirectories);
        }

        /// <summary>
        /// ルートディレクトリが混合状態になっ時、削除したら登録解除されているか
        /// </summary>
        [Fact]
        public void TestMixExistRemove()
        {
            var root = @"C:\";
            var manager = new CheckedDirectoryManager();
            manager.CheckChanged(root, null);
            manager.CheckChanged(root, false);

            Assert.DoesNotContain(root, manager.NonNestedDirectories);
            Assert.DoesNotContain(root, manager.NestedDirectories);
        }

        [Fact]
        public void TestAddRepeat()
        {
            var root = @"C:\";
            var manager = new CheckedDirectoryManager();
            manager.CheckChanged(root, true);
            manager.CheckChanged(root, true);
            var a = Path.Combine(root, "a");
            manager.CheckChanged(a, true);
            manager.CheckChanged(a, false);
            manager.CheckChanged(a, true);

            Assert.Equal(1, manager.NestedDirectories.Count(c => c == root));
        }

        [Fact]
        public void TestAddRemoveAdd()
        {
            var root = @"C:\";
            var manager = new CheckedDirectoryManager();
            manager.CheckChanged(root, true);
            manager.CheckChanged(root, false);
            manager.CheckChanged(root, true);
            var a = Path.Combine(root, "a");
            manager.CheckChanged(a, true);
            manager.CheckChanged(a, false);
            manager.CheckChanged(a, true);
            manager.CheckChanged(a, true);

            Assert.Equal(1, manager.NestedDirectories.Count(c => c == root));
            Assert.Equal(0, manager.NestedDirectories.Count(c => c == a));
        }
    }
}
