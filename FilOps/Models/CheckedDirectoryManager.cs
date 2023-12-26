using System.IO;
using FilOps.Models;

namespace FilOps.ViewModels.ExplorerPage
{
    /* ディレクトリのチェック状態を管理する
     * 
     * 考えていたような、チェック状態からサブディレクトリを含むか含まないかは不適切なアプローチだった
     * 
     * 初期状態：どこもチェックされていない
     * 複雑な処理は不要かもしれない
     * true が来たディレクトリを全て管理にする(除外リストに入っていたら削除する)
     * false が来たディレクトリを全て管理から外す(管理に入ってなければ除外リストに加える)
     * null が来ても何もしない(子ディレクトリで false が発生しただけだから)
     */

    #region インターフェース
    public interface ICheckedDirectoryManager
    {
        
    }
    #endregion インターフェース

    public class CheckedDirectoryManager : ICheckedDirectoryManager
    {
    }
}
