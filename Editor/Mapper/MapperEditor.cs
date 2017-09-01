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
using UnityEngine.U2D;

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

        public static void Map(string[] mapPaths, string pairsOutput)
        {
            //定义所有Pairs
            List<Pair> allPairs = new List<Pair>();

            //遍历需要映射的路径
            foreach (string path in mapPaths)
            {
                List<Pair> pairs;
                if (WarehouserUtils.IsDirectory(path))
                {
                    pairs = GetPairsByDirectoryPath(path);
                }
                else
                {
                    pairs = GetPairsByFilePath(path);
                }
                allPairs.AddRange(pairs);
            }

            //检查是否有重名
            Dictionary<string, int> recordCount = new Dictionary<string, int>();
            foreach (Pair pair in allPairs)
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
            Pairs pathMap = new Pairs();
            pathMap.pairs = allPairs.ToArray();

            //创建PathMap
            if (File.Exists(pairsOutput))
            {
                UnityEngine.Object old = AssetDatabase.LoadMainAssetAtPath(pairsOutput);
                EditorUtility.CopySerialized(pathMap, old);
            }
            else
            {
                string directoryPath = Path.GetDirectoryName(pairsOutput);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                AssetDatabase.CreateAsset(pathMap, pairsOutput);
            }
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
        private static List<Pair> GetPairsByDirectoryPath(string path)
        {
            List<Pair> pairs = new List<Pair>();
            if (Directory.Exists(path))
            {
                bool inResources = WarehouserUtils.IsResource(path);
                DirectoryInfo directory = new DirectoryInfo(path);
                FileInfo[] files = directory.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (FileInfo file in files)
                {
                    pairs.AddRange(GetPairsByFile(file));
                }
            }
            return pairs;
        }

        /// <summary>
        /// 通过文件路径获取对
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static List<Pair> GetPairsByFilePath(string path)
        {
            if (File.Exists(path))
            {
                FileInfo file = new FileInfo(path);
                return GetPairsByFile(file);
            }
            return null;
        }

        /// <summary>
        /// 从文件获取对列表
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static List<Pair> GetPairsByFile(FileInfo file)
        {
            List<Pair> pairs = new List<Pair>();

            if (IsIgnore(file.Extension))
                return pairs;

            string path = WarehouserUtils.WithAssetsPath(file.FullName);
            bool inResources = WarehouserUtils.IsResource(path);

            Pair pair;
            if (inResources)
            {
                pair = GetPairByResourceFile(file);
            }
            else
            {
                pair = GetPairByAssetBundleFile(file);
            }

            if (pair != null)
                pairs.Add(pair);

            //如果是图集
            if (file.Extension == ".spriteatlas")
            {
                SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                Sprite[] sprites = new Sprite[atlas.spriteCount];
                atlas.GetSprites(sprites);

                foreach (Sprite sp in sprites)
                {
                    string name = sp.texture.name;
                    pairs.Add(new Pair(name, atlas.tag, PairTagType.ATLAS_NAME));
                }
            }

            return pairs;
        }

        /// <summary>
        /// 通过Reousrce文件获取路径对
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static Pair GetPairByResourceFile(FileInfo file)
        {
            string name = file.Name.Replace(file.Extension, "");
            string path = WarehouserUtils.WithoutResourcesPath(file.FullName);
            return new Pair(name, path, PairTagType.RESOURCES_PATH);
        }

        /// <summary>
        /// 从AssetBundle文件得出路径对
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static Pair GetPairByAssetBundleFile(FileInfo file)
        {
            string assetPath = WarehouserUtils.WithAssetsPath(file.FullName);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null || string.IsNullOrEmpty(importer.assetBundleName))
                return null;

            string name = Path.GetFileNameWithoutExtension(file.Name);
            string path = importer.assetBundleName;
            return new Pair(name, path, PairTagType.ASSETBUNDLE_NAME);
        }
    }
}
