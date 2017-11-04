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
            UnityEngine.Object old = AssetDatabase.LoadMainAssetAtPath(pairsOutput);
            if (old != null)
            {
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

            Debug.Log("Map Complete !");
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

            string path = WarehouserUtils.ConvertUnixPath(file.FullName, "Assets", true, true);

            Pair pair = GetPairByAssetBundleFile(file);

            if (pair != null)
                pairs.Add(pair);

            //如果是图集
            if (file.Extension == ".spriteatlas")
            {
                SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                if (atlas == null)
                {
                    Debug.LogError("SpriteAtlas Load Error:" + path);
                    return pairs;
                }

                Sprite[] sprites = new Sprite[atlas.spriteCount];
                atlas.GetSprites(sprites);

                foreach (Sprite sp in sprites)
                {
                    string name = sp.texture.name;
                    pairs.Add(new Pair(name, atlas.tag));
                }
            }

            return pairs;
        }


        /// <summary>
        /// 从AssetBundle文件得出路径对
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static Pair GetPairByAssetBundleFile(FileInfo file)
        {
            string assetPath = WarehouserUtils.ConvertUnixPath(file.FullName, "Assets", true, true);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null || string.IsNullOrEmpty(importer.assetBundleName))
                return null;

            string name = Path.GetFileNameWithoutExtension(file.Name);
            string path = importer.assetBundleName;
            return new Pair(name, path);
        }
    }
}
