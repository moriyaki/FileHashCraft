/*  ResourceService.cs

    表示言語の切り替えを行うためのリソースサービスです。
 */

using System.Globalization;
using System.Reflection;
using System.Resources;

namespace FileHashCraft.Services
{
    public static class ResourceService
    {
        private static readonly ResourceManager resourceManager = new("FileHashCraft.Resources.Resources", Assembly.GetExecutingAssembly());

        public static void ChangeCulture(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);
            CultureInfo.CurrentUICulture = culture;
        }

        public static string GetString(string key)
        {
            return resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? string.Empty;
        }
    }
}