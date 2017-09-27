/*
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
                        //PackDirectory
                        if (Directory.Exists(path))
                        {
                            FileInfo[] files = GetFiles(path);
                            foreach (FileInfo file in files)
                            {
                                PackFile(file, package.assetBundleName, path);
                            }
                        }
                    }
                    else
                    {
                        //PackFile
                        if (File.Exists(path))
                        {
                            FileInfo file = new FileInfo(path);
                            PackFile(file, package.assetBundleName);
                        }
                    }
                }
            }
            Debug.Log("Pack Complete !");
        }

        /// <summary>
        /// 清理不用的AssetBundle
        /// </summary>
        /// <param name="packages"></param>
        public static void Clear(List<AssetBundlePackage> packages)
        {
            //删除不在包中的BundleName
            string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
            for (int i = 0, length = bundleNames.Length; i < length; i++)
            {
                string bundleName = bundleNames[i];
                //如果不包含在包中
                if (!Contains(packages, bundleName))
                {
                    AssetDatabase.RemoveAssetBundleName(bundleName, true);
                    Debug.Log("Clear Package:" + bundleName);
                }
            }

            //删除Streaming Assets
            List<string> unusedNames = new List<string>(AssetDatabase.GetUnusedAssetBundleNames());
            DirectoryInfo directory = new DirectoryInfo(Application.streamingAssetsPath);
            if (directory.Exists)
            {
                FileInfo[] files = directory.GetFiles("*.manifest", SearchOption.AllDirectories);
                foreach (FileInfo file in files)
                {
                    string bundleName = WarehouserUtils.ConvertUnixPath(file.FullName, "StreamingAssets", false, false);
                    if (bundleName == "StreamingAssets")
                        continue;

                    //不用的Name里包含 || 不包含包里
                    if (unusedNames.Contains(bundleName) || !Contains(packages, bundleName))
                    {
                        string bundlePath = Path.ChangeExtension(file.FullName, null);
                        File.Delete(bundlePath);
                        File.Delete(file.FullName);
                        Debug.Log("Clear StreamingAsset:" + bundlePath);
                    }
                }
            }
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();

            Debug.Log("Clear Complete !");
        }

        /// <summary>
        /// 判断一个BundleName是否包含在Packages里
        /// </summary>
        /// <returns></returns>
        public static bool Contains(List<AssetBundlePackage> packages, string bundleName)
        {
            foreach (AssetBundlePackage package in packages)
            {
                string lower = package.assetBundleName.ToLower();
                if (!IsPrefix(lower))
                {
                    if (bundleName == lower)
                    {
                        return true;
                    }
                }
                else
                {
                    if (bundleName.StartsWith(lower))
                    {
                        //判断是否还在该Package包含的目录中
                        string fileBundleName;
                        foreach (string path in package.paths)
                        {
                            if (WarehouserUtils.IsDirectory(path))
                            {
                                //PackDirectory
                                if (Directory.Exists(path))
                                {
                                    FileInfo[] files = GetFiles(path);
                                    foreach (FileInfo file in files)
                                    {
                                        fileBundleName = GetBundleName(file.FullName, package.assetBundleName, path);

                                        if (bundleName == fileBundleName)
                                            return true;
                                    }
                                }
                            }
                            else
                            {
                                //PackFile
                                if (File.Exists(path))
                                {
                                    FileInfo file = new FileInfo(path);
                                    fileBundleName = GetBundleName(file.FullName, package.assetBundleName);

                                    if (bundleName == fileBundleName)
                                        return true;
                                }
                            }
                        }
                        return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 打包单个文件
        /// </summary>
        /// <param name="path"></param>
        private static void PackFile(FileInfo file, string packageName, string directoryPath = null)
        {
            string assetPath = WarehouserUtils.ConvertUnixPath(file.FullName, "Assets", true, true);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
                return;

            string bundleName = GetBundleName(file.FullName, packageName, directoryPath);
            importer.assetBundleName = bundleName;

            Debug.Log("Pack: " + bundleName);
        }


        /// <summary>
        /// 通过路径获取文件
        /// </summary>
        /// <returns></returns>
        private static FileInfo[] GetFiles(string directoryPath)
        {
            List<FileInfo> files = new List<FileInfo>();

            //文件夹中所有文件
            DirectoryInfo directory = new DirectoryInfo(directoryPath);
            FileInfo[] allFiles = directory.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (FileInfo file in allFiles)
            {
                //过滤
                if (IsIgnore(file.Extension))
                    continue;

                files.Add(file);
            }
            return files.ToArray();
        }

        /// <summary>
        /// 获取BundleName
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <param name="packageName"></param>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        private static string GetBundleName(string fileFullName, string packageName, string directoryPath = null)
        {
            string bundleName;
            //无前缀
            if (!IsPrefix(packageName))
            {
                bundleName = packageName.ToLower();
                return bundleName;
            }
            //有前缀
            else
            {
                //目录下的子文件
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    string filePath = fileFullName;
#if UNITY_EDITOR_WIN
                    filePath = WarehouserUtils.ConverSeparator(filePath);
#endif
                    string relativePath = filePath.Substring(directoryPath.Length + 1);
                    bundleName = packageName + Path.ChangeExtension(relativePath, EXTENSION);
                }
                else
                {
                    string fileName = Path.GetFileName(fileFullName);
                    bundleName = packageName + Path.ChangeExtension(fileName, EXTENSION);
                }
                return bundleName;
            }
        }

        /// <summary>
        /// 是否忽略
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        private static bool IsIgnore(string extension)
        {
            if (extension == ".meta")
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
