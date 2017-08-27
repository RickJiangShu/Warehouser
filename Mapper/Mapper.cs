/*
 * Author:  Rick
 * Create:  2017/8/1 10:26:21
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
namespace Plugins.Warehouser
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 映射器 Mapper
    /// </summary>
    public class Mapper
    {
        private static Dictionary<string, Pair> mapOfPair;

        /// <summary>
        /// 初始化键值对
        /// </summary>
        /// <param name="pairs"></param>
        public static void Initialize(Pairs pairs)
        {
            mapOfPair = new Dictionary<string, Pair>();
            for (int i = 0, c = pairs.Length; i < c; i++)
            {
                Pair pair = pairs[i];
                mapOfPair.Add(pair.name, pair);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">路径</param>
        /// <param name="path">完整路径</param>
        /// <returns></returns>
        public static bool TryGetPath(string name, out Pair pair)
        {
            return mapOfPair.TryGetValue(name, out pair);
        }
    }
}
