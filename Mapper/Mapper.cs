/*
 * Author:  Rick
 * Create:  2017/8/1 10:26:21
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 映射器 Mapper
/// </summary>
public class Mapper
{
    private static Dictionary<string, string> mapOfPath;

    /// <summary>
    /// 初始化键值对
    /// </summary>
    /// <param name="pairs"></param>
    public static void Initialize(PathPairs pairs)
    {
        mapOfPath = new Dictionary<string,string>();
        for (int i = 0, c = pairs.Length; i < c; i++)
        {
            PathPair pair = pairs[i];
            mapOfPath.Add(pair.name, pair.path);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name">路径</param>
    /// <param name="path">完整路径</param>
    /// <returns></returns>
    public static bool TryGetPath(string name, out string path)
    {
        return mapOfPath.TryGetValue(name, out path);
    }
}