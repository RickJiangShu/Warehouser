/*
 * Author:  Rick
 * Create:  2017/8/19 11:38:15
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
namespace Plugins.Warehouser.Editor
{
    using UnityEngine;
    using System.Collections.Generic;
    /// <summary>
    /// Package
    /// </summary>
    [System.Serializable]
    public class AssetBundlePackage
    {
        public string assetBundleName;
        public List<string> paths;
    }
}
