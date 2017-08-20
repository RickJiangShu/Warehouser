﻿/*
 * Author:  Rick
 * Create:  2017/8/1 10:29:42
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 通用对象池
/// </summary>
public class ObjectPool
{
    private static Dictionary<string, List<object>> objectsOfPool = new Dictionary<string,List<object>>();

    public static void Push(string poolKey, object obj)
    {
        if(objectsOfPool.ContainsKey(poolKey))
        {
            objectsOfPool[poolKey].Add(obj);
        }
        else
        {
            objectsOfPool.Add(poolKey, new List<object>() { obj });
        }
    }

    public static object Pull(string poolKey)
    {
        List<object> objects;
        if (objectsOfPool.TryGetValue(poolKey, out objects))
        {
            object obj = objects[0];
            objects.RemoveAt(0);
            return obj;
        }
        return null;
    }
}