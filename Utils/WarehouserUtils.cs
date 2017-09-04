/*
 * Author:  Rick
 * Create:  2017/8/4 11:12:00
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System.IO;

namespace Plugins.Warehouser
{
    /// <summary>
    /// WarehouserUtils
    /// </summary>
    public class WarehouserUtils
    {
        /// <summary>
        /// 是否在Resources目录下
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsResource(string path)
        {
            return path.Contains("Resources");
        }

        public static bool IsDirectory(string path)
        {
            FileAttributes attr = File.GetAttributes(path);
            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }

        /// <summary>
        /// 转换路径
        /// </summary>
        /// <param name="path">完整路径</param>
        /// <param name="directory">目录名</param>
        /// <param name="withDirectory">包含目录</param>
        /// <param name="withExtension">包含拓展名</param>
        /// <returns></returns>
<<<<<<< Updated upstream
        public static string Convert2ResourcesPath(string path)
=======
        public static string ConvertPath(string path,string directory,bool withDirectory,bool withExtension)
>>>>>>> Stashed changes
        {
            int start = path.IndexOf(directory);
            if (withDirectory)
            {
                path = path.Substring(start);
            }
            else
            {
                path = path.Substring(start + directory.Length);
            }
            if (!withExtension)
            {
                path = Path.ChangeExtension(path, null);
            }
            return path;
        }

<<<<<<< Updated upstream
        public static string FullName2AssetPath(string fullName)
        {
            int assetIndx = fullName.IndexOf("Assets");
            string assetPath = fullName.Substring(assetIndx);
            return assetPath.Replace('\\', '/');
        }

=======
>>>>>>> Stashed changes
    }
}

