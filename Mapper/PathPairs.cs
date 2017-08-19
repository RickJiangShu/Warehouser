/*
 * Author:  Rick
 * Create:  2017/8/2 16:49:24
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 路径队列
/// </summary>
[System.Serializable]
public class PathPairs : ScriptableObject
{
    public PathPair[] pairs;

    public int Length
    {
        get { return pairs.Length; }
    }

    public PathPair this[int i]
    {
        get { return pairs[i]; }
    }
}

/// <summary>
/// ID和路径对
/// </summary>
[System.Serializable]
public class PathPair
{
    public string name;
    public string path;

    public PathPair(string name, string path)
    {
        this.name = name;
        this.path = path;
    }
}