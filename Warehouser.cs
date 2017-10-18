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

#if OBSERVER
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
    /// 对象池中的所有对象
    /// </summary>
    private static Dictionary<string, List<GameObject>> objectsOfPool;

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

#if OBSERVER
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


    /// <summary>
    /// 获取资源的实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static GameObject GetInstance(string name)
    {
        GameObject instance;
        //从对象池取
        if (objectsOfPool.ContainsKey(name) && objectsOfPool[name].Count > 0)
        {
            //考虑到对象池中的对象已被销毁的情况
            do
            {
                instance = objectsOfPool[name][0];
                objectsOfPool[name].RemoveAt(0);
            }
            while (instance == null);

            if(instance != null)
                return instance;
        }

        //实例化
        GameObject asset = GetAsset<GameObject>(name);
        instance = GameObject.Instantiate(asset);
        instance.name = name;//name对于Warehouser是有意义的
        return instance;
    }
        
    /// <summary>
    /// 回收实例
    /// </summary>
    /// <param name="instance"></param>
    public static void Recycle(GameObject instance)
    {
        instance.SetActive(false);

        if(objectsOfPool.ContainsKey(instance.name))
        {
            objectsOfPool[instance.name].Add(instance);
        }
        else
        {
            objectsOfPool.Add(instance.name, new List<GameObject>() { instance });
        }
#if OBSERVER
        Observer.recycleCount++;
#endif
    }

    /// <summary>
    /// 清除掉对象池中的对象
    /// </summary>
    public static void Clear()
    {
        foreach (List<GameObject> objects in objectsOfPool.Values)
        {
            foreach (GameObject obj in objects)
            {
                GameObject.Destroy(obj);
            }
            objects.Clear();
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

#if OBSERVER
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

#if OBSERVER
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

    /// <summary>
    /// GameObject对象池
    /// </summary>
    private class ObjectPool
    {
        private static Dictionary<string, Dictionary<string, List<GameObject>>> objects = new Dictionary<string,Dictionary<string,List<GameObject>>>();

        public static void Push(GameObject go)
        {
            string tag = go.tag;
            string name = go.name;
            if (!objects.ContainsKey(tag))
                objects.Add(tag, new Dictionary<string, List<GameObject>>());

            if (!objects[tag].ContainsKey(name))
                objects[tag].Add(name, new List<GameObject>());

            go.SetActive(false);
            objects[tag][name].Add(go);
        }

        public static void Pull(string name)
        {
        }

        /*
        public static Dictionary<string, List<Object>> objectsOfPool = new Dictionary<string, List<Object>>();

        public static void Push(string poolKey, Object obj)
        {
            if (objectsOfPool.ContainsKey(poolKey))
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
         */
    }
}

