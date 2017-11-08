/*
 * Author:  Rick
 * Create:  2017/8/3 16:05:50
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
namespace Plugins.Warehouser.Editor
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// RemSettings
    /// </summary>
    [System.Serializable]
    public class Setting : ScriptableObject
    {
        /// <summary>
        /// 批处理时的后缀名
        /// </summary>
        public string extension = ".ab";

        /// <summary>
        /// AB包
        /// </summary>
        public List<AssetBundlePackage> assetBundlePackages;

        /// <summary>
        /// 需要映射的路径
        /// </summary>
        public List<string> mapPaths;
    }
}
