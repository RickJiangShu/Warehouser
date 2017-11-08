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
        /// 打包
        /// </summary>
        /// <param name="packages"></param>
        public static void Pack(List<AssetBundlePackage> packages, string extension)
        {
            foreach (AssetBundlePackage package in packages)
            {
                switch (package.packMode)
                {
                    case PackMode.One:
                        PackOne(package, extension);
                        break;
                    case PackMode.Deep:
                        PackDeep(package, extension);
                        break;
                    case PackMode.Top:
                        PackTop(package, extension);
                        break;
                }
            }
            Debug.Log("Pack Complete !");
        }

        private static void PackOne(AssetBundlePackage package, string extension)
        {
            foreach (string path in package.paths)
            {
                if (WarehouserUtils.IsDirectory(path))
                {
                    FileInfo[] files = GetFiles(path, SearchOption.AllDirectories);
                    foreach (FileInfo file in files)
                    {
                        PackFile(file, package.name, extension);
                    }
                }
                else
                {
                    FileInfo file = new FileInfo(path);
                    PackFile(file, package.name, extension);
                }
            }
        }

        private static void PackDeep(AssetBundlePackage package, string extension)
        {
            foreach (string path in package.paths)
            {
                if (WarehouserUtils.IsDirectory(path))
                {
                    FileInfo[] files = GetFiles(path, SearchOption.AllDirectories);
                    foreach (FileInfo file in files)
                    {
                        string filePath = Path.ChangeExtension(file.FullName, null);
#if UNITY_EDITOR_WIN
                        filePath = WarehouserUtils.ConverSeparator(filePath);
#endif
                        int length = path.Length;
                        int startIndex = filePath.IndexOf(path) + (path[length - 1] == '/' ? length : length + 1);
                        string relativePath = filePath.Substring(startIndex);

                        string name = package.name + relativePath;
                        PackFile(file, name, extension);
                    }
                }
                else
                {
                    FileInfo file = new FileInfo(path);
                    string name = package.name + Path.GetFileNameWithoutExtension(file.FullName);
                    PackFile(file, name, extension);
                }
            }
        }

        private static void PackTop(AssetBundlePackage package, string extension)
        {
            foreach (string path in package.paths)
            {
                string relativePath = package.name.Remove(package.name.Length - 1, 1);
                if (WarehouserUtils.IsDirectory(path))
                {
                    DirectoryInfo directory = new DirectoryInfo(path);
                    FileInfo[] childFiles = GetFiles(directory, SearchOption.TopDirectoryOnly);
                    DirectoryInfo[] childDirectories = directory.GetDirectories();

                    //打包子文件夹中的所有文件
                    foreach (DirectoryInfo childDirectory in childDirectories)
                    {
                        FileInfo[] files = GetFiles(childDirectory, SearchOption.AllDirectories);
                        foreach (FileInfo file in files)
                        {
                            string name = relativePath + childDirectory.Name;
                            PackFile(file, name, extension);
                        }
                    }

                    //打包文件夹下的子文件
                    foreach (FileInfo file in childFiles)
                    {
                        string name = relativePath + Path.GetFileNameWithoutExtension(file.Name);
                        PackFile(file, name, extension);
                    }
                }
                else
                {
                    FileInfo file = new FileInfo(path);
                    string name = relativePath + Path.GetFileNameWithoutExtension(file.Name);
                    PackFile(file, name, extension);
                }
            }
        }

        /// <summary>
        /// 清理不用的AssetBundle
        /// </summary>
        /// <param name="packages"></param>
        public static void Clear(List<AssetBundlePackage> packages, string extension)
        {
            //删除不在包中的BundleName
            string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
            for (int i = 0, length = bundleNames.Length; i < length; i++)
            {
                string bundleName = bundleNames[i];

                //1. 对packageName进行对比
                AssetBundlePackage inPackage = null;
                foreach (AssetBundlePackage package in packages)
                {
                    if (package.packMode == PackMode.One)
                    {
                        string fullName = package.name + extension;
                        if (bundleName == fullName)
                        {
                            inPackage = package;
                            break;
                        }
                    }
                    else
                    {
                        string prefix = package.packMode == PackMode.Deep ? package.name : package.name.Remove(package.name.Length - 1);
                        if (bundleName.StartsWith(prefix))
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

                //2. 找到Bundle中每个AssetPath，看是否包含在paths中

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
                FileInfo[] manifests = directory.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (FileInfo file in manifests)
                {
                    string fileEx = Path.GetExtension(file.FullName);
                    if (fileEx == ".meta" || fileEx == ".manifest")
                        continue;
                       

                    string bundleName = WarehouserUtils.ConvertUnixPath(file.FullName, "StreamingAssets", false, true);
                    if (bundleName == "StreamingAssets")
                        continue;

                    //
                    if (!userdNames.Contains(bundleName))
                    {
                        string bundlePath = Path.ChangeExtension(file.FullName, null);
                       // File.Delete(bundlePath);
                        File.Delete(file.FullName);
                        File.Delete(file.FullName + ".manifest");
                        File.Delete(file.FullName + ".meta");
                        File.Delete(file.FullName + ".manifest.meta");

                        Debug.Log("Clear StreamingAsset:" + bundleName);
                    }
                }

                //清除空文件夹
                ClearEmptyDirectories(Application.streamingAssetsPath);
            }

            AssetDatabase.Refresh();

            Debug.Log("Clear Complete !");
        }

        /// <summary>
        /// 清理空的文件夹
        /// </summary>
        /// <param name="path"></param>
        private static void ClearEmptyDirectories(string path)
        {
            string[] subDirectories = Directory.GetDirectories(path);
            foreach (string directory in subDirectories)
            {
                ClearEmptyDirectories(directory);
                if (Directory.GetFileSystemEntries(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                    File.Delete(directory + ".meta");
                    Debug.Log("Clear Empty Directory:" + directory);
                }
            }
        }

        private static void PackFile(FileInfo file, string name, string extension)
        {
            string assetPath = WarehouserUtils.ConvertUnixPath(file.FullName, "Assets", true, true);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
                return;

            string bundleName = GetBundleName(name, extension);
            if (importer.assetBundleName == bundleName)
                return;

            importer.assetBundleName = bundleName;
            Debug.Log("Pack: " + file.Name + " to " + bundleName);
        }

        private static string GetBundleName(string name, string extension)
        {
            return (name + extension).ToLower();
        }

        /// <summary>
        /// 通过路径获取文件
        /// </summary>
        /// <returns></returns>
        private static FileInfo[] GetFiles(string directoryPath, SearchOption option)
        {
            //文件夹中所有文件
            DirectoryInfo directory = new DirectoryInfo(directoryPath);
            return GetFiles(directory, option);
        }

        private static FileInfo[] GetFiles(DirectoryInfo directory, SearchOption option)
        {
            List<FileInfo> files = new List<FileInfo>();
            FileInfo[] allFiles = directory.GetFiles("*.*", option);
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
    }
}
