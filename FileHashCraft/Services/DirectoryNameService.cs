using System.IO;

namespace FileHashCraft.Services
{
    public static class DirectoryNameService
    {
        /// <summary>
        /// 親ディレクトリから順に、現在のディレクトリまでのコレクションを取得します。
        /// </summary>
        /// <param name="path">コレクションを取得するディレクトリ</param>
        /// <param name="rootPath">ルートとなるディレクトリフルパス</param>
        /// <returns>親ディレクトリからのコレクション</returns>
        public static IList<string> GetDirectoryNames(string path, string rootPath = "")
        {
            var list = new List<string>();
            string? parent = path;
            list.Add(path);
            while ((parent = Path.GetDirectoryName(parent)) != null)
            {
                list.Add(parent);
            }
            if (!string.IsNullOrEmpty(path))
            {
                list.Remove(rootPath);
                string? root = rootPath;
                while ((root = Path.GetDirectoryName(root)) != null)
                {
                    list.Remove(root);
                }
            }
            list.Reverse();
            return list;
        }
    }
}