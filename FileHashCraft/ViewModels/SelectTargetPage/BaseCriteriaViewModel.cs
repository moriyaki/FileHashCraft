﻿using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.SelectTargetPage
{
    public abstract class BaseCriteriaViewModel : BaseViewModel
    {
        #region バインディング

        /// <summary>
        /// ワイルドカード検索条件コレクション
        /// </summary>
        public virtual ObservableCollection<BaseCriteriaItemViewModel> CriteriaItems { get; set; }

        /// <summary>
        /// 選択された検索条件コレクション
        /// </summary>
        public virtual ObservableCollection<BaseCriteriaItemViewModel> SelectedItems { get; set; }

        /// <summary>
        /// 検索条件エラー出力の背景色
        /// </summary>
        private Brush _SearchErrorBackground = Brushes.Transparent;

        public Brush SearchErrorBackground
        {
            get => _SearchErrorBackground;
            set => SetProperty(ref _SearchErrorBackground, value);
        }

        /// <summary>
        /// 検索条件エラー出力
        /// </summary>
        private string _SearchCriteriaErrorOutput = string.Empty;

        public string SearchCriteriaErrorOutput
        {
            get => _SearchCriteriaErrorOutput;
            set => SetProperty(ref _SearchCriteriaErrorOutput, value);
        }

        /// <summary>
        /// リストボックスで検索条件編集中の時は入力不可の色にする
        /// </summary>
        public Brush CiriteriaAddTextBoxBackgroudColor
        {
            get
            {
                if (SelectedItems.Count > 0)
                {
                    foreach (var item in SelectedItems)
                    {
                        if (item.IsEditMode)
                        {
                            return Brushes.LightGray;
                        }
                    }
                }
                return Brushes.Transparent;
            }
        }

        /// <summary>
        /// ワイルドカード新規検索文字列
        /// </summary>
        private string _SearchCriteriaText = string.Empty;

        public string SearchCriteriaText
        {
            get => _SearchCriteriaText;
            set
            {
                SetProperty(ref _SearchCriteriaText, value);
                AddCriteriaCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// ワイルドカード検索条件を追加します。
        /// </summary>
        public RelayCommand AddCriteriaCommand { get; set; }

        /// <summary>
        /// 登録したワイルドカード検索条件を編集します。
        /// </summary>
        public RelayCommand ModifyCriteriaCommand { get; set; }

        /// <summary>
        /// 登録したワイルドカード検索条件を削除します。
        /// </summary>
        public RelayCommand RemoveCriteriaCommand { get; set; }

        /// <summary>
        /// ヘルプウィンドウを開きます。
        /// </summary>
        public abstract RelayCommand HelpOpenCommand { get; set; }

        #endregion バインディング

        #region コンストラクタ

        protected readonly IFileSearchCriteriaManager _FileSearchCriteriaManager;
        protected readonly IHelpWindowViewModel _HelpWindowViewModel;

        /// <summary>
        /// 引数なしの直接呼び出しは許容しません。
        /// </summary>
        /// <exception cref="NotImplementedException">引数無しの直接呼び出し</exception>
        protected BaseCriteriaViewModel()
        { throw new NotImplementedException(nameof(BaseCriteriaViewModel)); }

        protected BaseCriteriaViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IFileSearchCriteriaManager fileSearchCriteriaManager,
            IHelpWindowViewModel helpWindowViewModel
        ) : base(messenger, settingsService)
        {
            _FileSearchCriteriaManager = fileSearchCriteriaManager;
            _HelpWindowViewModel = helpWindowViewModel;

            // コレクションのインスタンスを生成します。
            CriteriaItems = [];
            SelectedItems = [];

            // ワイルドカードを追加します。
            AddCriteriaCommand = new RelayCommand(
                () => AddCriteria(),
                () => IsCriteriaConditionCorrent(SearchCriteriaText)
            );

            // 検索条件一覧から編集モードにします。
            ModifyCriteriaCommand = new RelayCommand(
                () => SelectedItems[0].IsEditMode = true,
                () => SelectedItems.Count == 1
            );

            // 検索条件一覧削除します。
            RemoveCriteriaCommand = new RelayCommand(
                () => RemoveCriteria(),
                () => SelectedItems.Count > 0
            );

            // ファイルスキャンが終わったら
            _Messanger.Register<FileScanFinished>(this, (_, _) =>
                AddCriteriaCommand.NotifyCanExecuteChanged());

            // リストボックスアイテムの編集状態から抜けた時の処理をします。
            _Messanger.Register<IsEditModeChangedMessage>(this, (_, _) =>
                OnPropertyChanged(nameof(CiriteriaAddTextBoxBackgroudColor)));
        }

        #endregion コンストラクタ

        #region abstractメソッド

        /// <summary>
        /// ワイルドカード検索条件を追加します。
        /// </summary>
        public abstract void AddCriteria();

        /// <summary>
        /// ワイルドカード検索条件を削除します。
        /// </summary>
        public abstract void RemoveCriteria();

        /// <summary>
        /// リストボックスのワイルドカード検索条件から離れます。
        /// </summary>
        public abstract void LeaveListBoxCriteria();

        /// <summary>
        /// リストボックスのワイルドカード検索条件から強制的に離れます。
        /// </summary>
        public abstract void LeaveListBoxCriteriaForce();

        /// <summary>
        /// ワイルドカード検索条件を変更します。
        /// </summary>
        /// <param name="modifiedItem">変更されたワイルドカード検索条件</param>
        public abstract void ModefyCriteria(BaseCriteriaItemViewModel modifiedItem);

        /// <summary>
        /// ワイルドカード文字列が正しいかを検査します。
        /// 注：継承して使います。
        /// </summary>
        /// <param name="pattern">チェックするワイルドカード文字列</param>
        /// <param name="originalPattern">(必要ならば)元のワイルドカード文字列</param>
        /// <returns>ワイルドカード文字列が正当かどうか</returns>
        public abstract bool IsCriteriaConditionCorrent(string pattern, string originalPattern = "");

        #endregion abstractメソッド
    }
}