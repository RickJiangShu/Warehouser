/*
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
        if(Contains(poolKey))
        {
            objectsOfPool[poolKey].Add(obj);
        }
        else
        {
            objectsOfPool.Add(poolKey, new List<object>() { obj });
        }
    }

    public static T Pull<T>(string poolKey) where T : class
    {
        List<object> objects = objectsOfPool[poolKey];
        object obj = objects[0];
        objects.RemoveAt(0);
        return (T)obj;
    }

    public static object Pull(string poolKey)
    {
        return Pull<object>(poolKey);
    }

    public static bool TryPull<T>(string poolKey, out T obj) where T : class
    {
        if (Contains(poolKey))
        {
            obj = (T)Pull(poolKey);
            return true;
        }
        obj = null;
        return false;
    }
    public static bool TryPull(string poolKey,out object obj)
    {
        return TryPull<object>(poolKey, out obj);
    }

    public static bool Contains(string poolKey)
    {
        return objectsOfPool.ContainsKey(poolKey);
    }
}