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
using UnityEngine.U2D;
using Object = UnityEngine.Object;
using Plugins.Warehouser;

#if UNITY_EDITOR
using Plugins.Warehouser.Editor;
#endif

/// <summary>
/// 资源管理器 
/// </summary>
public class Warehouser
{
    /// <summary>
    /// AssetBundle 依赖文件
    /// </summary>
    private static AssetBundleManifest manifest;

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
        manifest = manifesetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        manifesetBundle.Unload(false);

        //加载PathPairs
        string path = WarehouserUtils.ConvertUnixPath(Constants.PATH_PAIRS_PATH, "Resources", false, false);
        Pairs pairs = Resources.Load<Pairs>(path);
        Mapper.Initialize(pairs);

        //初始化字典
        assetBundles = new Dictionary<string, AssetBundle>();

        //侦听图集引用请求
        SpriteAtlasManager.atlasRequested += AtlasRequest;

#if UNITY_EDITOR
        GameObject observer = new GameObject("Observer");
        observer.AddComponent<Observer>();
        Object.DontDestroyOnLoad(observer);
#endif
    }

    /// <summary>
    /// 处理图集加载请求
    /// </summary>
    private static void AtlasRequest(string name, Action<SpriteAtlas> callback)
    {
        SpriteAtlas atlas = GetAsset<SpriteAtlas>(name);
        callback(atlas);
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
    public static T GetInstance<T>(string name, params object[] args) where T : Object
    {
        T instance = (T)ObjectPool.Pull(name);

        //从对象池取
        if (instance != null)
        {
            if (instance is GameObject)
            {
                IRecycler recycler = ((GameObject)(Object)instance).GetComponent<IRecycler>();
                if(recycler != null)
                    recycler.OnPullFromPool(args);
            }
#if UNITY_EDITOR
            Observer.recycleNumber--;
#endif
            return instance;
        }

        //实例化
        T asset = GetAsset<T>(name);

        instance = UnityEngine.Object.Instantiate<T>(asset);
        
        if (instance is GameObject)
        {
            IInitializer initializer = ((GameObject)(Object)instance).GetComponent<IInitializer>();

            //如果有初始化组件，则初始化
            if (initializer != null)
                initializer.Initialize(args);

#if UNITY_EDITOR
            Observer.gameObjectNumber++;
#endif
        }

#if UNITY_EDITOR
        Observer.getInstanceCount++;
        Observer.instanceNumber++;
#endif

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

#if UNITY_EDITOR
        Observer.recycleNumber++;
        Observer.recycleCount++;
#endif
    }

    /// <summary>
    /// 销毁实例
    /// </summary>
    /// <param name="instance"></param>
    public static void Destroy(Object instance, float delay = 0.0f)
    {
        Object.Destroy(instance, delay);

#if UNITY_EDITOR
        Observer.destroyCount++;
        Observer.instanceNumber--;

        if (instance is GameObject)
        {
            Observer.gameObjectNumber--;
        }
#endif
    }

    
    /// <summary>
    /// 获取Asset
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T GetAsset<T>(string name) where T : Object
    {
        Object asset;

        //获取路径
        Pair pair;
        if (!Mapper.TryGetPath(name, out pair))
        {
            Debug.LogError("找不到路径：" + name);
            return null;
        }

        //加载
        switch (pair.tagType)
        {
            case PairTagType.RESOURCES_PATH:
                asset = Resources.Load<T>(pair.tag);
                break;
            case PairTagType.ASSETBUNDLE_NAME:
                AssetBundle bundle;
                if (assetBundles.TryGetValue(pair.tag, out bundle))
                {
                    asset = bundle.LoadAsset<T>(name);
                }
                else
                {
                    LoadDependencies(pair.tag);//加载依赖
                    bundle = LoadAndCacheAssetBundle(pair.tag);
                    asset = bundle.LoadAsset<T>(name);
                }
                break;
            case PairTagType.ATLAS_NAME:
                SpriteAtlas atlas = GetAsset<SpriteAtlas>(pair.tag);
                asset = atlas.GetSprite(name);
                break;
            default:
                asset = null;
                break;
        }

        if (asset == null)
        {
            Debug.LogError("Asset获取失败：" + name);
            return null;
        }

        //处理在Andorid和IOS平台时编辑器下的Shader丢失问题
#if UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS) 
        Type t = typeof(T);
        if (t == typeof(GameObject))
        {
            GameObject go = asset as GameObject;
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                string shaderName = r.sharedMaterial.shader.name;
                r.sharedMaterial.shader = Shader.Find(shaderName);
            }
            Projector[] projecters = go.GetComponentsInChildren<Projector>();
            foreach (Projector p in projecters)
            {
                string shaderName = p.material.shader.name;
                p.material.shader = Shader.Find(shaderName);
            }
        }
        else if (t == typeof(Material))
        {
            Material mat = asset as Material;
            string shaderName = mat.shader.name;
            mat.shader = Shader.Find(shaderName);
        }
#endif

#if UNITY_EDITOR
        Observer.getAssetCount++;
#endif
        return (T)asset;
    }

    /// <summary>
    /// 加载依赖包
    /// </summary>
    /// <param name="assetBundleName"></param>
    /// <returns></returns>
    private static void LoadDependencies(string assetBundleName)
    {
        //加载所有依赖包
        string[] dependencies = manifest.GetAllDependencies(assetBundleName);
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

#if UNITY_EDITOR
        Observer.unloadAssetCount++;
#endif
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