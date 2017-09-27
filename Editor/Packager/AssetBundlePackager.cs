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

                //找到对应Package
                AssetBundlePackage inPackage = null;
                foreach (AssetBundlePackage package in packages)
                {
                    string packageName = package.assetBundleName.ToLower();
                    if (!IsPrefix(packageName))
                    {
                        if (bundleName == packageName)
                        {
                            inPackage = package;
                            break;
                        }
                    }
                    else
                    {
                        if (bundleName.StartsWith(packageName))
                        {
                            inPackage = package;
                            break;
                        }
                    }
                }

                //如果找不到包
                if (inPackage == null)
                {
                    AssetDatabase.RemoveAssetBundleName(bundleName, true);//该bundleName根本不存在
                    Debug.Log("Clear Package:" + bundleName);
                    continue;
                }

                //找到该AssetBundle中所有Asset的路径
                string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);

                //判断Asset是否依然包含在Package的路径中
                foreach (string asset in assets)
                {
                    bool inPath = false;
                    foreach (string path in inPackage.paths)
                    {
                        if (asset.StartsWith(path))
                        {
                            inPath = true;
                            break;
                        }
                    }

                    if (!inPath)
                    {
                        AssetImporter importer = AssetImporter.GetAtPath(asset);
                        if (importer == null)
                            continue;

                        importer.assetBundleName = null;//bundleName存在于Package中，但该Asset的路径不包含在Paths中
                        Debug.Log("Clear AssetBundleName:" + asset);
                    }
                }
            }

            //删除无引用的BundleNames
            AssetDatabase.RemoveUnusedAssetBundleNames();

            //删除Streaming Assets
            DirectoryInfo directory = new DirectoryInfo(Application.streamingAssetsPath);
            if (directory.Exists)
            {
                List<string> userdNames = new List<string>(AssetDatabase.GetAllAssetBundleNames());
                FileInfo[] manifests = directory.GetFiles("*.manifest", SearchOption.AllDirectories);
                foreach (FileInfo file in manifests)
                {
                    string bundleName = WarehouserUtils.ConvertUnixPath(file.FullName, "StreamingAssets", false, false);
                    if (bundleName == "StreamingAssets")
                        continue;

                    //
                    if (!userdNames.Contains(bundleName))
                    {
                        string bundlePath = Path.ChangeExtension(file.FullName, null);
                        File.Delete(bundlePath);
                        File.Delete(file.FullName);
                        Debug.Log("Clear StreamingAsset:" + bundlePath);
                    }
                }
            }

            AssetDatabase.Refresh();

            Debug.Log("Clear Complete !");
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
            if (importer.assetBundleName == bundleName)
                return;

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
                bundleName = packageName;
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
                    int startIndex = filePath.IndexOf(directoryPath) + directoryPath.Length;
                    string relativePath = filePath.Substring(startIndex);
                    bundleName = packageName + Path.ChangeExtension(relativePath, EXTENSION);
                }
                else
                {
                    string fileName = Path.GetFileName(fileFullName);
                    bundleName = packageName + Path.ChangeExtension(fileName, EXTENSION);
                }
            }
            return bundleName.ToLower();
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
