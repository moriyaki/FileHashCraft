﻿using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.SelectTargetPage
{
    #region インターフェース

    public interface IWildcardCriteriaItemViewModel
    {
        /// <summary>
        /// 検索条件
        /// </summary>
        string Criteria { get; set; }
    }

    #endregion インターフェース

    public class WildcardCriteriaItemViewModel : BaseCriteriaItemViewModel, IWildcardCriteriaItemViewModel
    {
        #region バインディング

        /// <summary>
        /// ワイルドカード検索条件
        /// </summary>
        public override string Criteria
        {
            get => _Criteria;
            set
            {
                _CriteriaConditionCorrent = _Messanger.Send(new SelectedChangedWildcardCriteriaRequestMessage(value, OriginalCriteria));
                SetProperty(ref _Criteria, value);
            }
        }

        /// <summary>
        /// 編集モードかどうか
        /// </summary>
        public override bool IsEditMode
        {
            get => _IsEditMode;
            set
            {
                if (_IsEditMode == value) { return; }
                SetProperty(ref _IsEditMode, value);
                if (value)
                {
                    // 表示モードになったら、オリジナルを保存して編集モードに入ります。
                    OriginalCriteria = Criteria;
                    _Messanger.Send(new ListBoxSeletedWildcardTextBoxFocusMessage());
                }
                else
                {
                    // エラーが出ているなら、オリジナルの検索条件に戻します。
                    RestoreCriteria();
                }
                OnPropertyChanged(nameof(ItemBackgroudColor));
                OnPropertyChanged(nameof(BorderTickness));
                _Messanger.Send(new IsEditModeChangedMessage());
                _Messanger.Send(new SelectedChangedWildcardCriteriaRequestMessage(Criteria, OriginalCriteria));
            }
        }

        /// <summary>
        /// 該当アイテムが選択されているかのプロパティです。
        /// </summary>
        public override bool IsSelected
        {
            get => _IsSelected;
            set
            {
                SetProperty(ref _IsSelected, value);
                if (!value)
                {
                    RestoreCriteria();
                }
                _Messanger.Send(new IsSelectedWildcardChangedMessage(value, this));
            }
        }

        /// <summary>
        /// リストボックスアイテムのテキストボックスがクリックされた時の処理です。
        /// </summary>
        public RelayCommand ListBoxItemTextBoxWildcardCriteriaClicked { get; set; }

        #endregion バインディング

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ：リストボックスでアイテムがクリックされたかのイベント処理をします。
        /// </summary>
        /// <param name="settingsService"></param>
        public WildcardCriteriaItemViewModel(
            IMessenger messenger,
            ISettingsService settingsService
        ) : base(messenger, settingsService)
        {
            ListBoxItemTextBoxWildcardCriteriaClicked = new RelayCommand(() =>
            {
                IsEditMode = true;
                if (!IsSelected)
                {
                    // 選択状態が外れたら、新規入力画面にキャレットを当てます。
                    _Messanger.Send(new NewWildcardCriteriaFocusMessage());
                }
            });
        }

        #endregion コンストラクタ
    }
}