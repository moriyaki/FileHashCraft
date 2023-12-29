using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Properties;

namespace FilOps.ViewModels
{
    #region インターフェース
    public interface IResourceService
    {
        public Resources Resources { get; }
        public void ChangeCulture(string name);
        
    }
    #endregion インターフェース

    public class ResourceService : ObservableObject, IResourceService
    {
        /// <summary>
        /// 現在のリソースサービス
        /// </summary>
        private static readonly ResourceService _current = new();
        public static ResourceService Current { get => _current; }

        /// <summary>
        /// 現在のリソース
        /// </summary>
        private readonly Resources _resources = new();
        public Resources Resources { get => _resources; }

        /// <summary>
        /// 言語カルチャを変更する
        /// </summary>
        /// <param name="name">変更するカルチャ(例："ja-JP")</param>
        public void ChangeCulture(string name)
        {
            Resources.Culture = CultureInfo.GetCultureInfo(name);
            OnPropertyChanged(nameof(Resources));
        }
    }
}
