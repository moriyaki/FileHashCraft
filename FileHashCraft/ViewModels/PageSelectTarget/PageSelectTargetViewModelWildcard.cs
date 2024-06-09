using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Models.Helpers;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public interface IPageSelectTargetViewModelWildcard;
    public class PageSelectTargetViewModelWildcard : IPageSelectTargetViewModelWildcard
    {
        /// <summary>
        /// ワイルドカードを含むファイル名をRegex型に変換する
        /// </summary>
        /// <param name="pattern">ワイルドカードを含むファイル名</param>
        /// <returns>Regex型</returns>
        public static Regex WildcardToRegexPattern(string pattern)
        {
            return new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
        }
    }
}
