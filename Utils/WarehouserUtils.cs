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
        public static bool InResources(string path)
        {
            return path.Contains("Resources");
        }

        /// <summary>
        /// 将FullName转换成相对于Resources的路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string Convert2ResourcesPath(string path)
        {
            int start = path.IndexOf("Resources");
            string resourcesPath = path.Substring(start + 10);//10 是Resources/ 的字符数
            resourcesPath = Path.ChangeExtension(resourcesPath, null);
            return resourcesPath;
        }

        public static string FullName2AssetPath(string fullName)
        {
            int assetIndx = fullName.IndexOf("Assets");
            string assetPath = fullName.Substring(assetIndx);
            return assetPath.Replace('\\', '/');
        }

    }
}

