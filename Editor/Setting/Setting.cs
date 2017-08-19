/*
 * Author:  Rick
 * Create:  2017/8/3 16:05:50
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
namespace Plugins.Warehouser.Editor
{
    using System.Collections.Generic;

    /// <summary>
    /// RemSettings
    /// </summary>
    [System.Serializable]
    public class Setting
    {
        /// <summary>
        /// 需要映射的路径
        /// </summary>
        public List<string> mapPaths;

        /// <summary>
        /// 包
        /// </summary>
        public List<Package> packages;
    }
}
