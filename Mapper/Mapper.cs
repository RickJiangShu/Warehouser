﻿/*
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
        private static Dictionary<string, string> mapOfPaths;

        /// <summary>
        /// 初始化键值对
        /// </summary>
        /// <param name="pairs"></param>
        public static void Initialize(Pairs pairs)
        {
            mapOfPaths = new Dictionary<string, string>();
            for (int i = 0, c = pairs.Length; i < c; i++)
            {
                Pair pair = pairs[i];
                mapOfPaths.Add(pair.name, pair.path);
            }
        }

        public static string Get(string name)
        {
            return mapOfPaths[name];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">路径</param>
        /// <param name="path">完整路径</param>
        /// <returns></returns>
        public static bool TryGetPath(string name, out string path)
        {
            return mapOfPaths.TryGetValue(name, out path);
        }
    }
}
