using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilOps.Models
{
    /* IsChecked用ディレクトリ管理クラス
     * 
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
    public class CheckedDirectoryManager : IDirectoryManager
    {
        /// <summary>
        /// 登録したディレクトリのリスト
        /// </summary>
        private readonly List<string> _directories = [];

        /// <summary>
        /// 登録したディレクトリのリストを取得する
        /// </summary>
        /// <returns></returns>
        public List<string> Directories { get => _directories; }

        public bool HasDirectory(string path) => _directories.Where(dir => path.Contains(dir)).Any();

        /// <summary>
        /// 指定したパスを管理対象に追加する
        /// </summary>
        /// <param name="path">追加するパス</param>
        public void AddDirectory(string path)
        {
            if (!HasDirectory(path)) { _directories.Add(path); }
        }

        /// <summary>
        /// 指定したパスを管理対象から外す
        /// </summary>
        /// <param name="path">削除するパス</param>
        public void RemoveDirectory(string path)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 指定したパスコレクションを管理対象に追加する
        /// </summary>
        /// <param name="pathCollection">追加するパスコレクション</param>
        public void AddDirectory(IEnumerable<string> pathCollection)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  指定したパスコレクションを管理対象から外す
        /// </summary>
        /// <param name="pathCollection">削除するパスコレクションを</param>

        public void RemoveDirectory(IEnumerable<string> pathCollection)
        {
            throw new NotImplementedException();
        }
    }
}
