/*
 * Author:  Rick
 * Create:  2017/8/3 16:05:50
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */

namespace Plugins.Warehouser
{
    /// <summary>
    /// RemSettings
    /// </summary>
    [System.Serializable]
    public class WarehouserSetting
    {
        /// <summary>
        /// Setting 路径
        /// </summary>
        public const string PATH = "Assets/Resources/WarehouserSetting.json";


        /// <summary>
        /// 路径映射对名字
        /// </summary>
        public const string PATH_PAIRS_NAME = "PathPairs.asset";

        /// <summary>
        /// 路径映射对输出目录
        /// </summary>
        public string pathPairsDirectory = "Assets/Resources";

        public string pathPairsPath
        {
            get { return pathPairsDirectory + "/" + PATH_PAIRS_NAME; }
        }

    }
}
