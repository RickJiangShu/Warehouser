/*
 * Author:  Rick
 * Create:  2017/8/1 10:15:06
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;
using Plugins.Warehouser;

#if OBSERVER
using Plugins.Warehouser.Observer;
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
    public static Dictionary<string, List<GameObject>> objectsOfPool;

    /// <summary>
    /// 启动（运行时必先调用）
    /// </summary>
    public static void Start()
    {
        //初始化静态变量
        assetBundles = new Dictionary<string, AssetBundle>();
        objectsOfPool = new Dictionary<string,List<GameObject>>();

        //加载Manifeset
        AssetBundle manifesetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/StreamingAssets");
        manifest = manifesetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        manifesetBundle.Unload(false);

        //加载PathPairs
        AssetBundle pairsBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/pairs.ab");
        Pairs pairs = pairsBundle.LoadAsset<Pairs>("Pairs");
        Mapper.Initialize(pairs);
        pairsBundle.Unload(true);

        //侦听图集引用请求
        SpriteAtlasManager.atlasRequested += AtlasRequest;

#if OBSERVER
        GameObject observer = new GameObject("Observer");
        observer.AddComponent<ObserverWindow>();
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
                instance.SetActive(true);
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

#if OBSERVER
        instance.AddComponent<Observer>();
#endif
        return instance;
    }
        
    /// <summary>
    /// 回收实例
    /// </summary>
    /// <param name="instance"></param>
    public static void Recycle(GameObject instance)
    {
        if(objectsOfPool.ContainsKey(instance.name))
        {
            if (objectsOfPool[instance.name].Contains(instance))//添加已经存在于对象池中的对象
                return;

            objectsOfPool[instance.name].Add(instance);
        }
        else
        {
            objectsOfPool.Add(instance.name, new List<GameObject>() { instance });
        }
        instance.SetActive(false);
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

    public static void Clear(string name)
    {
        if (objectsOfPool.ContainsKey(name))
        {
            foreach (GameObject obj in objectsOfPool[name])
            {
                GameObject.Destroy(obj);
            }
            objectsOfPool[name].Clear();
        }
    }

    public static void ClearWithTag(string tag)
    {
        foreach (List<GameObject> objects in objectsOfPool.Values)
        {
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                if (objects[i].tag == tag)
                {
                    GameObject.Destroy(objects[i]);
                    objects.RemoveAt(i);
                }
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
        Object asset;

        //获取路径
        string path;
        if (!Mapper.TryGetPath(name, out path))
        {
            Debug.LogError("找不到路径：" + name);
            return null;
        }

        //AssetBundle 加载
        if (Path.HasExtension(path))
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
        //图集加载
        else
        {
            SpriteAtlas atlas = GetAsset<SpriteAtlas>(path);
            asset = atlas.GetSprite(name);
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