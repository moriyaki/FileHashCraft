using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace FilOps.Models
{
    /* ディレクトリ管理クラス
     * 
     * フルパスで単純に管理するケースと、
     * サブディレクトリの登録状況によって振る舞いが変化するケースがある。
     * 
     * コンストラクタでtrueを設定したら、単純に管理する、
     * これは IsExpand に関わるので複雑に考える必要はない。
     * 
     * コンストラクタでfalseを設定したら、登録状況に酔って振る舞いを変化させる。
     * 状況によって振る舞いが変化するケースは IsChecked に関わる。
     * あるディレクトリ A が登録されていたら、その中にある全てのディレクトリ B,C,D も対象とするが追加はしない。
     * 追加はしないが、B,C,D に問われたら true を返す必要がある。D 内にある E も同様に true だ。
     * 
     * ここで仮に C が削除されたら、A を登録解除して、A の中の B,D を登録する。
     * A は null(混合状態)、C は当然 false、B,D は登録されているので当然 true となる。
     * 
     * さらに、改めて C が登録われたら、A の中にあるディレクトリをチェックする。
     * ディレクトリ全て、すなわち B,C,D が登録されていたら、A を登録して B,C,D を外す、E は変化なし。
     * 
     * ここで仮に D が削除されたら、A を null(混合状態) にして、A の中の B,C を登録する。
     * A,D は当然 false、B,Cは登録されているので当然 true、そして E は D が削除されたので false となる。
     * 
     * ここで仮に A が登録されたら、無条件に B,C,D,E は削除され、Aが登録される。
     */
    public enum ManagementType
    {
        ForWatcher,
        ForChecked,
    }

    public class DirectoryManager
    {
        public DirectoryManager()
        {
            throw new NotImplementedException();
        }


        public DirectoryManager(ManagementType managementType)
        {
            _managementType = managementType;
        }

        private readonly ManagementType _managementType;
        private readonly List<string> _directories = [];

        public int Count() => _directories.Count;

        public bool HasDirectory(string path)
        {
            if (_managementType == ManagementType.ForWatcher)
            {
                return _directories.Where(dir => dir == path).Any();
            }
            else
            {
                return _directories.Where(dir => path.Contains(dir)).Any();
            }
        }

        /// <summary>
        /// 指定したパスを管理対象に追加する
        /// </summary>
        /// <param name="path">追加するパス</param>
        public void AddDirectory(string path)
        {
            if (HasDirectory(path)) { return; }
            _directories.Add(path);
        }

        /// <summary>
        /// 指定したパスを管理対象から外す
        /// </summary>
        /// <param name="path">削除するパス</param>
        public void RemoveDirectory(string path)
        {
            if (_managementType == ManagementType.ForWatcher)
            {
                // pathと同じディレクトリがあったら削除する
                if (HasDirectory(path)) { _directories.Remove(path); }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 指定したパスコレクションを管理対象に追加する
        /// </summary>
        /// <param name="pathCollection">追加するパスコレクション</param>
        public void AddDirectory(IEnumerable<string> pathCollection)
        {
            if (_managementType == ManagementType.ForWatcher)
            {
                // pathと同じディレクトリがあったら追加しない
                _directories.AddRange(pathCollection.Except(_directories));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        ///  指定したパスコレクションを管理対象から外す
        /// </summary>
        /// <param name="pathCollection">削除するパスコレクションを</param>

        public void RemoveDirectory(IEnumerable<string> pathCollection)
        {
            if (_managementType == ManagementType.ForWatcher)
            {
                // pathと同じディレクトリがあったら追加しない
                _directories.RemoveAll(remove => pathCollection.Any(path => path == remove));
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
