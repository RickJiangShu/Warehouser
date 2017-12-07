/*
 * Author:  Rick
 * Create:  2017/12/1 17:27:20
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池
/// </summary>
sealed public class ObjectPool
{
    /// <summary>
    /// 所有对象
    /// </summary>
    private Dictionary<string, Queue<GameObject>> objects;

    public ObjectPool()
    {
        objects = new Dictionary<string, Queue<GameObject>>();
    }

    ~ObjectPool()
    {
        Clear();
    }

    /// <summary>
    /// 添加对象到池
    /// </summary>
    public void Push(GameObject obj)
    {
        if (objects.ContainsKey(obj.name))
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!objects[obj.name].Contains(obj))
            {
                Warehouser.Log("重复添加-" + obj.name, LogType.Error);
            }
#endif
            objects[obj.name].Enqueue(obj);
        }
        else
        {
            Queue<GameObject> newQueue = new Queue<GameObject>();
            newQueue.Enqueue(obj);
            objects.Add(obj.name, newQueue);
        }

        obj.SetActive(false);
    }

    /// <summary>
    /// 从对象池获取一个对象
    /// </summary>
    public GameObject Pull(string name)
    {
        GameObject obj;
        while (objects[name].Count > 0)
        {
            obj = objects[name].Dequeue();

            if (!obj.Equals(null))
            {
                obj.SetActive(true);
                return obj;
            }
        }
        return null;
    }

    /// <summary>
    /// 清空
    /// </summary>
    public void Clear()
    {
        foreach (Queue<GameObject> queue in objects.Values)
        {
            foreach (GameObject obj in queue)
            {
                GameObject.Destroy(obj);
            }
        }
        objects.Clear();
    }

    /// <summary>
    /// 是否包含对象
    /// </summary>
    public bool Contains(string name)
    {
        return objects.ContainsKey(name) && objects[name].Count > 0;
    }

    public Queue<GameObject> this[string name]
    {
        get
        {
            return objects[name];
        }
    }
}
