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
    /// 目前缓存的Bundle
    /// </summary>
    private static Dictionary<string, AssetBundle> assetBundles;

    /// <summary>
    /// 启动（运行时必先调用）
    /// </summary>
    public static void Setup()
    {
        //加载Manifeset
        AssetBundle manifesetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/StreamingAssets");
        manifeset = manifesetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        manifesetBundle.Unload(false);

        //加载PathPairs
        string path = WarehouserUtils.Convert2ResourcesPath(Constants.PATH_PAIRS_PATH);
        PathPairs pairs = Resources.Load<PathPairs>(path);
        Mapper.Initialize(pairs);

        //初始化字典
        assetBundles = new Dictionary<string, AssetBundle>();
    }

    public static GameObject GetInstance(string name, params object[] initArags)
    {
        return GetInstance<GameObject>(name, initArags);
    }

    /// <summary>
    /// 获取资源的实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T GetInstance<T>(string name, params object[] initArgs) where T : Object
    {
        T instance;

        //从对象池取
        if (ObjectPool.TryPull<T>(name, out instance))
        {
            if (instance is GameObject)
            {
                IRecycler recycler = ((GameObject)(Object)instance).GetComponent<IRecycler>();
                if(recycler != null)
                    recycler.OnPullFromPool();
            }
            return instance;
        }

        //实例化
        T resource = GetAsset<T>(name);
        instance = UnityEngine.Object.Instantiate<T>(resource);
        
        if (instance is GameObject)
        {
            IInitializer initializer = ((GameObject)(Object)instance).GetComponent<IInitializer>();

            //如果有初始化组件，则初始化
            if (initializer != null)
                initializer.Initlize(initArgs);
        }
        return instance;
    }

    /// <summary>
    /// 回收实例
    /// </summary>
    /// <param name="instance"></param>
    public static void Recycle(string name, Object instance)
    {
        if (instance is GameObject)
        {
            //回收处理
            IRecycler recycler = ((GameObject)instance).GetComponent<IRecycler>();
            if (recycler != null)
                recycler.OnPushToPool();
        }
        ObjectPool.Push(name, instance);
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
        AssetBundle bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + assetBundleName);
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
            assetBundles.Remove(assetBundleName);
        }
    }

}