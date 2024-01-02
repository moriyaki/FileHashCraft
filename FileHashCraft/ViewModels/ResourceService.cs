using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using FileHashCraft.Properties;

namespace FileHashCraft.ViewModels
{
    #region インターフェース
    public interface IResourceService;
    #endregion インターフェース

    public class ResourceService : ObservableObject, IResourceService
    {
        public static ResourceService Current { get; } = new();
        public Resources Resources { get; } = new();

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
