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
    /// 全局对象池
    /// </summary>
    public static ObjectPool global
    {
        get;
        private set;
    }

    /// <summary>
    /// 容器
    /// </summary>
    private static GameObject _container;

    static ObjectPool()
    {
        global = new ObjectPool();
        _container = new GameObject("ObjectPool");
        GameObject.DontDestroyOnLoad(_container);
    }

    /// <summary>
    /// 当前所有对象
    /// </summary>
    private Dictionary<string, Queue<GameObject>> _objects;

    public ObjectPool()
    {
        _objects = new Dictionary<string, Queue<GameObject>>();
    }

    public void Push(GameObject obj, bool worldPositionStays, float t)
    {
        obj.transform.SetParent(_container.transform, worldPositionStays);
        Push(obj, t);
    }

    /// <summary>
    /// 延迟回收
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="t"></param>
    public void Push(GameObject obj, float t)
    {
        TempRecycler recycler = obj.AddComponent<TempRecycler>();
        recycler.duration = t;
    }

    public void Push(GameObject obj, bool worldPositionStays)
    {
        obj.transform.SetParent(_container.transform, worldPositionStays);
        Push(obj);
    }

    /// <summary>
    /// 添加对象到池
    /// </summary>
    public void Push(GameObject obj)
    {
        if (_objects.ContainsKey(obj.name))
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_objects[obj.name].Contains(obj))
            {
                Warehouser.Log("重复添加-" + obj.name, LogType.Error);
            }
#endif
            _objects[obj.name].Enqueue(obj);
        }
        else
        {
            Queue<GameObject> newQueue = new Queue<GameObject>();
            newQueue.Enqueue(obj);
            _objects.Add(obj.name, newQueue);
        }

        obj.SetActive(false);
    }

    

    /// <summary>
    /// 从对象池获取一个对象
    /// </summary>
    public GameObject Pull(string name)
    {
        GameObject obj;
        if (_objects.ContainsKey(name))
        {
            while (_objects[name].Count > 0)
            {
                obj = _objects[name].Dequeue();

                if (!obj.Equals(null))
                {
                    obj.SetActive(true);
                    return obj;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 清空
    /// </summary>
    public void Clear()
    {
        foreach (Queue<GameObject> queue in _objects.Values)
        {
            foreach (GameObject obj in queue)
            {
                GameObject.Destroy(obj);
            }
        }
        _objects.Clear();
    }
    
    public Queue<GameObject> this[string name]
    {
        get
        {
            return _objects[name];
        }
    }

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    public Dictionary<string, Queue<GameObject>> objects
    {
        get { return _objects; }
    }
    #endif
}
