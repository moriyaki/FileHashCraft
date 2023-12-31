namespace FileHashCraft.Models
{
    /* ディレクトリのチェック状態を管理する
     *
     * 初期状態：どこもチェックされていない
     *
     * true  が来たら、その下のディレクトリを全て管理にする、上位にいたら何もしない
     * false が来たら、その下のディレクトリを全て管理から外す、上位下位はUI任せ
     * null  が来たら、そのディレクトリを単独監視にする
     */

    #region インターフェース
    public interface ICheckedDirectoryManager
    {
        /// <summary>
        /// そのディレクトリがチェックされているかどうかを調べる。
        /// </summary>
        /// <param name="fullPath">チェックされているかを調べるディレクトリのフルパス</param>
        /// <returns>チェックされているかどうか</returns>
        public bool IsChecked(string fullPath);

        /// <summary>
        /// ディレクトリのチェック状態を変化させる。
        /// </summary>
        /// <param name="fullPath">変化させるディレクトリのフルパス</param>
        /// <param name="checkedStatus">変化させる状態</param>
        public void CheckChanged(string fullPath, bool? checkedStatus);

        /// <summary>
        /// サブディレクトリを含むチェックされているリスト
        /// </summary>
        public List<string> NestedDirectories { get; }

        /// <summary>
        /// サブディレクトリを含まないチェックされているリスト
        /// </summary>
        public List<string> NonNestedDirectories { get; }

    }
    #endregion インターフェース

    public class CheckedDirectoryManager : ICheckedDirectoryManager
    {
        /// <summary>
        /// サブディレクトリを含むチェックされているリスト
        /// </summary>
        private readonly List<string> _nestedDirectories = [];
        public List<string> NestedDirectories { get => _nestedDirectories;}

        /// <summary>
        /// サブディレクトリを含まないチェックされているリスト
        /// </summary>
        private readonly List<string> _nonNestedDirectories = [];
        public List<string> NonNestedDirectories { get => _nonNestedDirectories; }
        /// <summary>
        /// そのディレクトリがチェックされているかどうかを調べる。
        /// </summary>
        /// <param name="fullPath">チェックされているかを調べるディレクトリのフルパス</param>
        /// <returns>チェックされているかどうか</returns>
        public bool IsChecked(string fullPath)
        {
            if (_nonNestedDirectories.Any(o => o == fullPath)) { return true; }
            return _nestedDirectories.Any(d => fullPath.Contains(d));
        }

        /// <summary>
        /// ディレクトリのチェック状態を変化させる。
        /// </summary>
        /// <param name="fullPath">変化させるディレクトリのフルパス</param>
        /// <param name="checkedStatus">変化させる状態</param>
        public void CheckChanged(string fullPath, bool? checkedStatus)
        {
            switch (checkedStatus)
            {
                case true:
                    CheckedTrue(fullPath);
                    break;
                case false:
                    CheckedFalse(fullPath);
                    break;
                default:
                    CheckedNull(fullPath);
                    break;
            }
        }

        /// <summary>
        /// ディレクトリのチェック状態がチェック状態になった。
        /// </summary>
        /// <param name="fullPath">チェック状態になったディレクトリのフルパス</param>
        private void CheckedTrue(string fullPath)
        {
            if (_nestedDirectories.Contains(fullPath)) { return; }

            // fullPath で始まる要素を全削除
            _nestedDirectories.RemoveAll(c => c.StartsWith(fullPath));

            // 含まれているディレクトリで始まる fullPath なら追加しない
            if (_nestedDirectories.Any(c => fullPath.StartsWith(c))) { return; }

            _nonNestedDirectories.Remove(fullPath);
            _nestedDirectories.Add(fullPath);
        }

        /// <summary>
        /// ディレクトリのチェック状態がチェック解除になった。
        /// </summary>
        /// <param name="fullPath"></param>
        private void CheckedFalse(string fullPath)
        {
            _nonNestedDirectories.Remove(fullPath);
            _nestedDirectories.RemoveAll(c => c.StartsWith(fullPath));
        }

        /// <summary>
        /// ディレクトリのチェック状態が混合状態になった。
        /// </summary>
        /// <param name="fullPath"></param>
        private void CheckedNull(string fullPath)
        {
            _nestedDirectories.Remove(fullPath);
            if (_nonNestedDirectories.Any(c => c == fullPath)) { return; }
            _nonNestedDirectories.Add(fullPath);
        }
    }
}
