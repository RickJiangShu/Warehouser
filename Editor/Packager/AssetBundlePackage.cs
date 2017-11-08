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
        public string name;
        public List<string> paths;

        public PackMode packMode
        {
            get
            {
                int i = name.Length - 1;
                if (name[i] == '/')
                {
                    if (name[i - 1] == '/')
                        return PackMode.Top;

                    return PackMode.Deep;
                }
                return PackMode.One;
            }
        }
    }

    public enum PackMode
    {
        One,
        Deep,
        Top,
    }
}
