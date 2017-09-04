﻿/*
 * Author:  Rick
 * Create:  2017/8/19 11:40:55
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
namespace Plugins.Warehouser.Editor
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// 打包器
    /// </summary>
    public class AssetBundlePackager : ScriptableObject
    {
        /// <summary>
        /// 包扩展名
        /// </summary>
        private const string EXTENSION = ".ab";

        /// <summary>
        /// 打包
        /// </summary>
        /// <param name="packages"></param>
        public static void Pack(List<AssetBundlePackage> packages)
        {
            foreach (AssetBundlePackage package in packages)
            {
                foreach (string path in package.paths)
                {
                    if (WarehouserUtils.IsDirectory(path))
                    {
                        PackDirectory(path, package.assetBundleName);
                    }
                    else
                    {
                        PackFile(path, package.assetBundleName);
                    }
                }
            }
        }

        /// <summary>
        /// 清理掉非Package指定的Asset Bundle
        /// </summary>
        public static void Clear(List<AssetBundlePackage> packages)
        {
            string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
            for (int i = 0, length = bundleNames.Length; i < length; i++)
            {
                string bundleName = bundleNames[i];
                bool inPackage = false;
                foreach (AssetBundlePackage package in packages)
                {
                    if (!IsPrefix(package.assetBundleName))
                    {
                        inPackage = bundleName == package.assetBundleName;
                    }
                    else
                    {
                        inPackage = bundleName.StartsWith(package.assetBundleName);
                    }
                    if (inPackage)
                        break;
                }

                //如果不包含在包中
                if (!inPackage)
                {
                    AssetDatabase.RemoveAssetBundleName(bundleName, true);
                }
            }

            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 打包整个文件夹
        /// </summary>
        /// <param name="path"></param>
        private static void PackDirectory(string path, string name)
        {
            if (Directory.Exists(path))
            {
                bool isPrefix = IsPrefix(name);
                DirectoryInfo directory = new DirectoryInfo(path);
                FileInfo[] files = directory.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (FileInfo file in files)
                {
                    if (IsIgnore(file.Extension))
                        continue;

                    if (!isPrefix)
                    {
                        PackFile(file, name);
                    }
                    else
                    {
                  //      string fullDirectoryPath = directory.FullName.
                        string relativePath = file.FullName.Substring(directory.FullName.Length + 1);
                        string bundleName = name + relativePath;
                        bundleName = Path.ChangeExtension(bundleName, EXTENSION);
                        PackFile(file, bundleName);
                    }
                }
            }
        }

        /// <summary>
        /// 打包单个文件
        /// </summary>
        /// <param name="path"></param>
        private static void PackFile(string path, string name)
        {
            if (File.Exists(path))
            {
                FileInfo file = new FileInfo(path);
                bool isPrefix = IsPrefix(name);

                if (!isPrefix)
                {
                    PackFile(file, name);
                }
                else
                {
                    string bundleName = name + file.Name + EXTENSION;
                    PackFile(file, bundleName);
                }
            }
        }
        private static void PackFile(FileInfo file, string name)
        {
            string assetPath = WarehouserUtils.ConvertPath(file.FullName, "Assets/", false, true);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
                return;

            importer.assetBundleName = name;
        }

        /// <summary>
        /// 是否忽略
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        private static bool IsIgnore(string extension)
        {
            if (extension == ".mate")
                return true;
            return false;
        }

        /// <summary>
        /// 是否是前缀模式名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool IsPrefix(string name)
        {
            return name[name.Length - 1] == '/';
        }

    }
}
