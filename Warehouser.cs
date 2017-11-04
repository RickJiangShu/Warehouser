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

#if TEST
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
    /// 所有加载的Bundles
    /// </summary>
    internal static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();

    /// <summary>
    /// Bundle中尚未加载的Asset数量
    /// </summary>
    internal static Dictionary<string, int> leftAssetCount = new Dictionary<string, int>(); 

    /// <summary>
    /// 所有加载的Assets
    /// </summary>
    internal static Dictionary<string, Object> assets = new Dictionary<string, Object>();

    /// <summary>
    /// 对象池中的所有对象
    /// </summary>
    internal static Dictionary<string, List<GameObject>> pool = new Dictionary<string, List<GameObject>>();

#if TEST
    /// <summary>
    /// 所有对象（包括instance和newObject）
    /// </summary>
    internal static Dictionary<string, List<GameObject>> allObjects = new Dictionary<string, List<GameObject>>();
#endif

    /// <summary>
    /// 启动（运行时必先调用）
    /// </summary>
    public static void Start()
    {
        //加载Manifeset
        AssetBundle manifesetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/StreamingAssets");
        manifest = manifesetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        manifesetBundle.Unload(false);
        
        //加载PathPairs
        Pairs pairs = Resources.Load<Pairs>("WarehouserPairs");
        Mapper.Initialize(pairs);
        Resources.UnloadAsset(pairs);

        //侦听图集引用请求
        SpriteAtlasManager.atlasRequested += OnAtlasRequest;

        //侦听内存不足事件
        Application.lowMemory += OnLowMemory;

#if TEST
        GameObject observer = new GameObject("Observer");
        observer.AddComponent<ObserverWindow>();
        Object.DontDestroyOnLoad(observer);
#endif
    }

    /// <summary>
    /// 内存不足事件
    /// </summary>
    private static void OnLowMemory()
    {
        Debug.LogError("LowMemory");

#if TEST
        ObserverWindow.memoryWarningCount++;
#endif

        //清空对象池
        Clear();

        //清空AssetBundles

        //防止程序中使用其他创建Asset的操作，比如：new Texture()
        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// 处理图集加载请求
    /// </summary>
    private static void OnAtlasRequest(string name, Action<SpriteAtlas> callback)
    {
        SpriteAtlas atlas = GetAsset<SpriteAtlas>(name);
        callback(atlas);
    }

    /// <summary>
    /// 如果对象池有便取，否则新建一个新的对象
    /// </summary>
    /// <returns></returns>
    public static GameObject GetObject(string name, params Type[] components)
    {
        GameObject dynamicObject = Pull(name);
        if (dynamicObject == null)
        {
            dynamicObject = New(name, components);
        }
        return dynamicObject;
    }

    /// <summary>
    /// 新建一个GameObject
    /// </summary>
    /// <returns></returns>
    public static GameObject New(string name, params Type[] components)
    {
        GameObject dynamicObject = new GameObject(name, components);

#if TEST
        if (allObjects.ContainsKey(name))
        {
            allObjects[name].Add(dynamicObject);
        }
        else
        {
            allObjects.Add(name, new List<GameObject>() { dynamicObject });
        }
#endif
        return dynamicObject;
    }

    /// <summary>
    /// 如果对象池有便取，否则实例化一个新的Prefab
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static GameObject GetInstance(string name)
    {
        GameObject instance = Pull(name);
        if (instance == null)
        {
            instance = Instantiate(name);
        }
        return instance;
    }

    /// <summary>
    /// 实例化一个Prefab
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static GameObject Instantiate(string name)
    {
        GameObject original = GetAsset<GameObject>(name);
        GameObject instance = GameObject.Instantiate(original);
        instance.name = name;//name对于Warehouser是有意义的

#if TEST
        if (allObjects.ContainsKey(name))
        {
            allObjects[name].Add(instance);
        }
        else
        {
            allObjects.Add(name, new List<GameObject>() { instance });
        }
#endif
        return instance;
    }

    /// <summary>
    /// 从对象池取
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static GameObject Pull(string name)
    {
        if (pool.ContainsKey(name))
        {
            //考虑到对象池中的对象已被销毁的情况
            GameObject objOfPool = null;
            while (pool[name].Count > 0)
            {
                objOfPool = pool[name][0];
                pool[name].RemoveAt(0);

                if (!objOfPool.Equals(null))
                {
                    objOfPool.SetActive(true);
                    return objOfPool;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 回收实例
    /// </summary>
    /// <param name="instance"></param>
    public static void Push(GameObject instance)
    {
        if(pool.ContainsKey(instance.name))
        {
            if (pool[instance.name].Contains(instance))//防止重复添加
                return;

            pool[instance.name].Add(instance);
        }
        else
        {
            pool.Add(instance.name, new List<GameObject>() { instance });
        }
        instance.SetActive(false);
    }

    /// <summary>
    /// 清除掉对象池中的对象
    /// </summary>
    public static void Clear()
    {
        foreach (List<GameObject> objects in pool.Values)
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
        if (pool.ContainsKey(name))
        {
            foreach (GameObject obj in pool[name])
            {
                GameObject.Destroy(obj);
            }
            pool[name].Clear();
        }
    }

    public static void ClearWithTag(string tag)
    {
        foreach (List<GameObject> objects in pool.Values)
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
    /// 获取图集上的精灵
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Sprite GetSprite(string name)
    {
        string atlasName;
        if (!Mapper.TryGetPath(name, out atlasName))
        {
            Debug.LogError("找不到引用的图集：" + name);
            return null;
        }

        SpriteAtlas atlas = GetAsset<SpriteAtlas>(atlasName);
        return atlas.GetSprite(name);
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

        //获取缓存
        if (assets.TryGetValue(name, out asset))
            return (T)asset;

        //获取路径
        string path;
        if (!Mapper.TryGetPath(name, out path))
        {
            Debug.LogError("找不到映射的路径：" + name);
            return null;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!Path.HasExtension(path))
        {
            Debug.LogError("获取图集上的精灵，使用Warehouser.GetSprite");
            return null;
        }
#endif

        //AssetBundle 加载
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
        //检测包中是否还有没加载的Asset，如果没有则销毁Bundle
        int leftCount = --leftAssetCount[path];
        if (leftCount == 0)
        {
            UnloadAssetBundle(bundle);
        }

        assets.Add(name, asset);

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

        string[] assetNames = bundle.GetAllAssetNames();
        leftAssetCount.Add(assetBundleName, assetNames.Length);
        return bundle;
    }

    /// <summary>
    /// 卸载AssetBundle
    /// </summary>
    /// <param name="assetBundleName"></param>
    /// <param name="unloadAllLoadedObjects"></param>
    public static void UnloadAssetBundle(AssetBundle bundle)
    {
        assetBundles.Remove(bundle.name);
        leftAssetCount.Remove(bundle.name);
        bundle.Unload(false);
    }

    public static void UnloadAsset(GameObject asset)
    {
        assets.Remove(asset.name);
        GameObject.DestroyImmediate(asset, true);
    }
    /// <summary>
    /// 卸载Asset
    /// </summary>
    /// <param name="asset"></param>
    public static void UnloadAsset(Object asset)
    {
        assets.Remove(asset.name);
        Resources.UnloadAsset(asset);
    }
}