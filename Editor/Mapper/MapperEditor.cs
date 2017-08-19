/*
 * Author:  Rick
 * Create:  2017/8/4 16:59:38
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Plugins.Warehouser.Editor
{
    /// <summary>
    /// 映射器
    /// </summary>
    public class MapperEditor
    {
        /// <summary>
        /// 忽略的拓展名
        /// </summary>
        private static readonly string[] IGNORE_EXTENSIONS = new string[1] { ".meta" };

        public static void Map(string[] mapPaths, string pathPairsOutput)
        {
            //定义所有Pairs
            List<PathPair> allPairs = new List<PathPair>();

            //遍历需要映射的路径
            foreach (string path in mapPaths)
            {
                if (WarehouserUtils.IsDirectory(path))
                {
                    List<PathPair> pairs = GetPairsByDirectoryPath(path);
                    allPairs.AddRange(pairs);
                }
                else
                {
                    PathPair pair = GetPairByFilePath(path);
                    if (pair != null)
                        allPairs.Add(pair);
                }
            }

            //检查是否有重名
            Dictionary<string, int> recordCount = new Dictionary<string, int>();
            foreach (PathPair pair in allPairs)
            {
                if (recordCount.ContainsKey(pair.name))
                    recordCount[pair.name]++;
                else
                    recordCount.Add(pair.name, 1);
            }

            //找出同名的Id
            List<string> sameNames = new List<string>();
            foreach (string key in recordCount.Keys)
            {
                if (recordCount[key] > 1)
                    sameNames.Add(key);
            }


            //如果有重名
            if (sameNames.Count > 0)
            {
                foreach (string sameName in sameNames)
                {
                    Debug.LogError(string.Format(Tips.SAME_NAME, sameName));
                }
                return;
            }

            //生成PathMap
            PathPairs pathMap = new PathPairs();
            pathMap.pairs = allPairs.ToArray();

            //创建PathMap
            if (File.Exists(pathPairsOutput))
            {
                UnityEngine.Object old = AssetDatabase.LoadMainAssetAtPath(pathPairsOutput);
                EditorUtility.CopySerialized(pathMap, old);
            }
            else
                AssetDatabase.CreateAsset(pathMap, pathPairsOutput);
        }

        /// <summary>
        /// 是否忽略
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        private static bool IsIgnore(string extension)
        {
            for (int i = 0, j = IGNORE_EXTENSIONS.Length; i < j; i++)
            {
                if (IGNORE_EXTENSIONS[i] == extension)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 通过目录获取其中的路径对
        /// </summary>
        /// <returns></returns>
        private static List<PathPair> GetPairsByDirectoryPath(string path)
        {
            List<PathPair> pairs = new List<PathPair>();
            if (Directory.Exists(path))
            {
                bool inResources = WarehouserUtils.IsResource(path);
                DirectoryInfo directory = new DirectoryInfo(path);
                FileInfo[] files = directory.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (FileInfo file in files)
                {
                    if (IsIgnore(file.Extension))
                        continue;

                    PathPair pair;
                    if (inResources)
                    {
                        pairs.Add(GetPairByResourceFile(file));
                    }
                    else
                    {
                        pair = GetPairByAssetBundleFile(file);
                        if(pair != null)
                            pairs.Add(pair);
                    }
                }
            }
            return pairs;
        }

        /// <summary>
        /// 通过文件路径获取路径对
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static PathPair GetPairByFilePath(string path)
        {
            if (File.Exists(path))
            {
                FileInfo file = new FileInfo(path);
                if (IsIgnore(path))
                    return null;

                bool inResources = WarehouserUtils.IsResource(path);
                PathPair pair;
                if (inResources)
                {
                    pair = GetPairByResourceFile(file);
                }
                else
                {
                    pair = GetPairByAssetBundleFile(file);
                }
                return pair;
            }
            return null;
        }

        /// <summary>
        /// 通过Reousrce文件获取路径对
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static PathPair GetPairByResourceFile(FileInfo file)
        {
            string name = file.Name.Replace(file.Extension, "");
            string path = WarehouserUtils.Convert2ResourcesPath(file.FullName);
            return new PathPair(name, path);
        }

        /// <summary>
        /// 从AssetBundle文件得出路径对
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static PathPair GetPairByAssetBundleFile(FileInfo file)
        {
            string assetPath = WarehouserUtils.FullName2AssetPath(file.FullName);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null || string.IsNullOrEmpty(importer.assetBundleName))
                return null;

            string name = Path.GetFileNameWithoutExtension(file.Name);
            string path = importer.assetBundleName;
            return new PathPair(name, path);
        }
    }
}
