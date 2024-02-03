/*  PageSelectTargetViewModel.PartialExpert.cs

    エキスパート向けの検索条件を提供するタブの ViewModel を提供します。
 */

using System.IO;
using CommunityToolkit.Mvvm.Input;
using FileHashCraft.Models;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public partial class PageSelectTargetViewModel
    {
        #region バインディング
        /// <summary>
        ///  読み取り専用ファイルを対象にするかどうか
        /// </summary>
        private bool _IsReadOnlyFileInclude;
        public bool IsReadOnlyFileInclude
        {
            get => _IsReadOnlyFileInclude;
            set
            {
                if (value == _IsReadOnlyFileInclude) { return; }
                SetProperty(ref _IsReadOnlyFileInclude, value);
                _messageServices.SendZeroSizeFileDelete(value);
            }
        }

        /// <summary>
        /// 隠しファイルを対象にするかどうか
        /// </summary>
        private bool _IsHiddenFileInclude;
        public bool IsHiddenFileInclude
        {
            get => _IsHiddenFileInclude;
            set
            {
                if (value == _IsHiddenFileInclude) { return; }
                SetProperty(ref _IsHiddenFileInclude, value);
                _messageServices.SendEmptyDirectoryDelete(value);
            }
        }
        /// <summary>
        ///  0 サイズのファイルを削除するかどうか
        /// </summary>
        private bool _IsZeroSizeFileDelete;
        public bool IsZeroSizeFileDelete
        {
            get => _IsZeroSizeFileDelete;
            set
            {
                if (value == _IsZeroSizeFileDelete) { return; }
                SetProperty(ref _IsZeroSizeFileDelete, value);
                _messageServices.SendZeroSizeFileDelete(value);
            }
        }

        /// <summary>
        /// 空のフォルダを削除するかどうか
        /// </summary>
        private bool _IsEmptyDirectoryDelete;
        public bool IsEmptyDirectoryDelete
        {
            get => _IsEmptyDirectoryDelete;
            set
            {
                if (value == _IsEmptyDirectoryDelete) { return; }
                SetProperty(ref _IsEmptyDirectoryDelete, value);
                _messageServices.SendEmptyDirectoryDelete(value);
            }
        }
        #endregion バインディング

        #region コマンド
        /// <summary>
        /// 読み取り専用ファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
        /// </summary>
        public RelayCommand IsReadOnlyFileIncludeClicked { get; set; }
        /// <summary>
        /// 隠しファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
        /// </summary>
        public RelayCommand IsHiddenFileIncludeClicked { get; set; }
        /// <summary>
        ///  0 サイズのファイルを削除するかどうかのテキストがクリックされた時のコマンド
        /// </summary>
        public RelayCommand IsZeroSizeFIleDeleteClicked { get; set; }
        /// <summary>
        /// 空のフォルダを削除するかどうかのテキストがクリックされた時のコマンド
        /// </summary>
        public RelayCommand IsEmptyDirectoryDeleteClicked { get; set; }
        #endregion コマンド

        #region 検索条件の操作
        /// <summary>
        /// ロックオブジェクト
        /// </summary>
        private readonly object _conditionLock = new();

        /// <summary>
        /// 正規表現なら正しければ、それ以外は type=None でなければ無条件に検索条件リストに追加します。
        /// </summary>
        /// <param name="type">検索条件タイプ</param>
        /// <param name="contidionString">検索条件</param>
        /// <returns>成功の可否</returns>
        public void AddCondition(SearchConditionType type, string contidionString)
        {
            var condition = SearchCondition.AddCondition(type, contidionString);
            if (condition == null) { return; }
            lock (_conditionLock)
            {
                switch (type)
                {
                    case SearchConditionType.Extention:
                        foreach (var extentionFile in AllFiles.Values.Where(c => string.Equals(Path.GetExtension(c.FileFullPath), contidionString, StringComparison.OrdinalIgnoreCase)))
                        {
                            // 条件辞書にファイルを登録する
                            if (!ConditionFiles.TryGetValue(condition, out HashSet<HashFile>? value))
                            {
                                value = ([]);
                                ConditionFiles.Add(condition, value);
                            }
                            value.Add(extentionFile);
                        }
                        break;
                    case SearchConditionType.WildCard:
                        break;
                    case SearchConditionType.RegularExprettion:
                        break;
                }
            }
        }

        /// <summary>
        /// 検索条件を削除します。
        /// </summary>
        /// <param name="type">検索条件のタイプ</param>
        /// <param name="contidionString">検索条件</param>
        public void RemoveCondition(SearchConditionType type, string contidionString)
        {
            var condition = ConditionFiles.Keys.FirstOrDefault(c => c.Type == type && c.ConditionString == contidionString);
            if (condition == null) { return; }

            lock (_conditionLock)
            {
                foreach (var file in ConditionFiles[condition])
                {
                    ConditionFiles.Remove(condition);
                }
            }
        }
        #endregion 検索条件の操作
    }
}
