using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilOps.ViewModels.ExplorerPage
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
     * 
     * Aの混合状態を保持するためのリストが必要になる
     * 
     * ディレクトリスキャンを避けるため、この状態遷移はViewModelで行い、結果だけをここに伝える
     */
    public interface ICheckedDirectoryManager
    {
        public List<string> CompleteDirectories { get; }
        public List<string> ImcompleteDirectories { get; }
    }

    public class CheckedDirectoryManager : ICheckedDirectoryManager
    {
        /// <summary>
        /// 登録した、サブディレクトリを含むディレクトリのリスト
        /// </summary>
        private readonly List<string> _completeDirectories = [];

        /// <summary>
        /// 登録した、サブディレクトリを含むディレクトリのリストを取得する
        /// </summary>
        /// <returns></returns>
        public List<string> CompleteDirectories { get => _completeDirectories; }

        /// <summary>
        /// 登録した、サブディレクトリを含まないディレクトリのリスト
        /// </summary>
        private readonly List<string> _imcompleteDirectories = [];
        /// <summary>
        /// 登録した、サブディレクトリを含まないディレクトリのリストを取得する
        /// </summary>
        /// <returns></returns>
        public List<string> ImcompleteDirectories { get => _imcompleteDirectories; }



    }
}
