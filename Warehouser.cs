/*
 * Author:  Rick
 * Create:  2017/8/1 10:15:06
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Plugins.Warehouser;

/// <summary>
/// 资源管理器 
/// </summary>
public class Warehouser
{
    /// <summary>
    /// AssetBundle 依赖文件
    /// </summary>
    private static AssetBundleManifest manifeset;

    /// <summary>
    /// 获取PoolKey通过InstanceID
    /// </summary>
    private static Dictionary<int, string> poolKeysOfInstances;

    /// <summary>
    /// 目前缓存的Bundle
    /// </summary>
    private static Dictionary<string, AssetBundle> assetBundles;

    /// <summary>
    /// 启动（运行时必先调用）
    /// </summary>
    public static void Setup()
    {
        //加载Manifeset
        AssetBundle manifesetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "StreamingAssets");
        manifeset = manifesetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        manifesetBundle.Unload(false);

        //加载PathPairs
        PathPairs pairs = Resources.Load<PathPairs>(Constants.PATH_PAIRS_PATH);
        Mapper.Initialize(pairs);

        //静态变量赋值
        poolKeysOfInstances = new Dictionary<int, string>();
    }

    public static GameObject GetInstance(string name)
    {
        return GetInstance<GameObject>(name);
    }

    public static T GetInstance<T>(string name) where T : Object
    {
        return GetInstance<T>(name, name);
    }
    /// <summary>
    /// 获取资源的实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T GetInstance<T>(string name, string poolKey, params object[] initArgs) where T : Object
    {
        T instance;
        GameObject go;

        //从对象池取
        if (ObjectPool.TryPull<T>(poolKey, out instance))
        {
            IRecycler recycler;
            go = instance as GameObject;
            if (go != null && TryGetComponent<IRecycler>(go, out recycler))
            {
                recycler.OnPullFromPool();
            }
            return instance;
        }

        //实例化
        T resource = GetAsset<T>(name);
        instance = UnityEngine.Object.Instantiate<T>(resource);
        
        //建立索引
        int id = instance.GetInstanceID();
        poolKeysOfInstances.Add(id, poolKey);

        //GameObject
        go = instance as GameObject;
        if (instance is GameObject)
        {
            //如果有初始化组件，则初始化
            IInitializer initializer;
            if (TryGetComponent<IInitializer>(go, out initializer))
            {
                initializer.Initlize(initArgs);
            }

            //如果没有回收组件，添加默认
            IRecycler recycler;
            if (!TryGetComponent<IRecycler>(go, out recycler))
            {
                go.AddComponent<Recycler>();
            }
        }

        return instance;
    }

    /// <summary>
    /// 回收实例
    /// </summary>
    /// <param name="instance"></param>
    public static void Recycle(Object instance)
    {
        int id = instance.GetInstanceID();
        string poolKey;
        if (!poolKeysOfInstances.TryGetValue(id,out poolKey))
        {
            Debug.LogError("找不到实例：" + instance.name + " 的PoolKey，将直接销毁！");
            UnityEngine.Object.Destroy(instance);
            return;
        }

        ObjectPool.Push(poolKey, instance);

        if (instance is GameObject)
        {
            //回收处理
            IRecycler recycler;
            if (TryGetComponent<IRecycler>((GameObject)instance, out recycler))
            {
                recycler.OnPushToPool();
            }
        }
    }
    
    /// <summary>
    /// 获取Asset
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T GetAsset<T>(string name) where T : Object
    {
        T asset;

        //获取路径
        string path;
        if (!Mapper.TryGetPath(name, out path))
        {
            return null;
        }

        //加载
        if (WarehouserUtils.IsResource(path))
        {
            asset = Resources.Load<T>(path);
        }
        else
        {
            AssetBundle bundle;
            if (assetBundles.TryGetValue(path, out bundle))
            {
                asset = bundle.LoadAsset<T>(name);
            }
            else
            {
                LoadDependencies(path);//加载依赖
                bundle = LoadAndCacheAssetBundle(path);
                asset = bundle.LoadAsset<T>(name);
            }
        }
        return asset;
    }

    /// <summary>
    /// 加载依赖包
    /// </summary>
    /// <param name="assetBundleName"></param>
    /// <returns></returns>
    private static void LoadDependencies(string assetBundleName)
    {
        //加载所有依赖包
        string[] dependencies = manifeset.GetAllDependencies(assetBundleName);
        for (int i = 0, l = dependencies.Length; i < l; i++)
        {
            string dependency = dependencies[i];
            if (assetBundles.ContainsKey(dependency))
                continue;

            LoadAndCacheAssetBundle(dependency);
        }
    }

    /// <summary>
    /// 加载并缓存AssetBundle
    /// </summary>
    /// <param name="assetBundleName"></param>
    private static AssetBundle LoadAndCacheAssetBundle(string assetBundleName)
    {
        AssetBundle bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + assetBundleName);
        assetBundles.Add(assetBundleName, bundle);
        return bundle;
    }


    /// <summary>
    /// 卸载Asset
    /// </summary>
    /// <param name="asset"></param>
    public static void UnloadAsset(Object asset)
    {
        if (asset is GameObject)
        {
            GameObject.DestroyImmediate(asset, true);
        }
        else
        {
            Resources.UnloadAsset(asset);
        }
    }

    /// <summary>
    /// 卸载AssetBundle
    /// </summary>
    /// <param name="assetBundleName"></param>
    /// <param name="unloadAllLoadedObjects"></param>
    public static void UnloadAssetBundle(string assetBundleName,bool unloadAllLoadedObjects)
    {
        AssetBundle bundle;
        if (assetBundles.TryGetValue(assetBundleName, out bundle))
        {
            bundle.Unload(unloadAllLoadedObjects);
        }
    }

    /// <summary>
    /// 判断实例是否是 GameObject 并且 有对应的组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private static bool TryGetComponent<T>(GameObject go, out T result) where T : class
    {
        if (go != null)
        {
            result = go.GetComponent<T>();
            return result != null;
        }
        result = null;
        return false;
    }
}