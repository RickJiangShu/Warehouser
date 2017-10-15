/*
 * Author:  Rick
 * Create:  2017/8/1 10:29:42
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用对象池
/// </summary>
public class ObjectPool
{
    public static Dictionary<string, List<Object>> objectsOfPool = new Dictionary<string, List<Object>>();

    public static void Push(string poolKey, Object obj)
    {
        if(objectsOfPool.ContainsKey(poolKey))
        {
            objectsOfPool[poolKey].Add(obj);
        }
        else
        {
            objectsOfPool.Add(poolKey, new List<Object>() { obj });
        }
    }

    public static object Pull(string poolKey)
    {
        List<Object> objects;
        if (objectsOfPool.TryGetValue(poolKey, out objects) && objects.Count > 0)
        {
            object obj = objects[0];
            objects.RemoveAt(0);
            return obj;
        }
        return null;
    }
}