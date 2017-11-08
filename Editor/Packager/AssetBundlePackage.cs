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
        public PackMode packMode;
        public string assetBundleName;
        public List<string> paths;

        /*
        public PackMode packMode
        {
            get
            {
                int i = assetBundleName.Length - 1;
                if (assetBundleName[i] == '/')
                {
                    if (assetBundleName[i - 1] == '/')
                        return PackMode.Children;

                    return PackMode.Files;
                }
                return PackMode.One;
            }
        }
         */
    }

    public enum PackMode
    {
        One,//标准
        Files,//递归
        Children,//盒子
    }
}
