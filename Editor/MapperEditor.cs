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

namespace Plugins.Warehouser
{
    /// <summary>
    /// MapperEditor
    /// </summary>
    public class MapperEditor
    {
        /// <summary>
        /// 忽略的拓展名
        /// </summary>
        private static readonly string[] IGNORE_EXTENSIONS = new string[1] { ".meta" };

        public static void MapPaths(string pathPairsPath)
        {
            //映射Resource
            List<PathPair> pairsOfResources = GetPairsOfResources();

            //映射AssetBundle
            List<PathPair> pairsOfAssetBundles = GetPairsOfAssetBundles();

            //所有对
            List<PathPair> allPairs = new List<PathPair>();
            allPairs.AddRange(pairsOfResources);
            allPairs.AddRange(pairsOfAssetBundles);


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
            if (File.Exists(pathPairsPath))
            {
                UnityEngine.Object old = AssetDatabase.LoadMainAssetAtPath(pathPairsPath);
                EditorUtility.CopySerialized(pathMap, old);
            }
            else
                AssetDatabase.CreateAsset(pathMap, pathPairsPath);
        }

        /// <summary>
        /// 是否忽略
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static bool IsIgnore(string fileName)
        {
            for (int i = 0, j = IGNORE_EXTENSIONS.Length; i < j; i++)
            {
                if (IGNORE_EXTENSIONS[i] == Path.GetExtension(fileName))
                    return true;
            }

            if (fileName == Path.GetFileName(WarehouserSetting.PATH))
                return true;

            if (fileName == WarehouserSetting.PATH_PAIRS_NAME)
                return true;

            return false;
        }

        /// <summary>
        /// 获取Resources目录下所有的路径对
        /// </summary>
        /// <returns></returns>
        private static List<PathPair> GetPairsOfResources()
        {
            List<PathPair> pairs = new List<PathPair>();

            DirectoryInfo resourceDir = new DirectoryInfo(Application.dataPath + Path.DirectorySeparatorChar + "Resources");
            if (resourceDir.Exists)
            {
                FileInfo[] filesInResources = resourceDir.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (FileInfo file in filesInResources)
                {
                    if (IsIgnore(file.Name))
                        continue;

                    string name = file.Name.Replace(file.Extension, "");
                    string path = WarehouserUtils.Convert2ResourcesPath(file.FullName);
                    pairs.Add(new PathPair(name, path));
                }
            }
            return pairs;
        }

        /// <summary>
        /// 获取所有AssetBundle的路径对
        /// </summary>
        /// <returns></returns>
        private static List<PathPair> GetPairsOfAssetBundles()
        {
            List<PathPair> pairs = new List<PathPair>();

            DirectoryInfo assetDir = new DirectoryInfo(Application.dataPath);
            FileInfo[] allFiles = assetDir.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (FileInfo file in allFiles)
            {
                if (IsIgnore(file.Name))
                    continue;

                string assetPath = WarehouserUtils.FullName2AssetPath(file.FullName);
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer == null || string.IsNullOrEmpty(importer.assetBundleName))
                    continue;

                string name = Path.GetFileNameWithoutExtension(file.Name);
                string path = importer.assetBundleName;
                pairs.Add(new PathPair(name, path));
            }

            return pairs;
        }
    }
}
