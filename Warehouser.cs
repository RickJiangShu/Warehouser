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
    /// 路径映射
    /// </summary>
    private static Dictionary<string, string> paths = new Dictionary<string, string>();

    /// <summary>
    /// 所有加载的Bundles
    /// </summary>
    internal static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();

    /// <summary>
    /// 对象池中的所有对象
    /// </summary>
    internal static Dictionary<string, List<GameObject>> pool = new Dictionary<string, List<GameObject>>();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
        for (int i = 0, l = pairs.Length; i < l; i++)
        {
            Pair pair = pairs[i];
            paths.Add(pair.name, pair.path);
        }
        Resources.UnloadAsset(pairs);

        //侦听图集引用请求
        SpriteAtlasManager.atlasRequested += OnAtlasRequest;

        //侦听内存不足事件
        Application.lowMemory += OnLowMemory;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameObject observer = new GameObject("Observer");
        observer.AddComponent<Observer>();
        Object.DontDestroyOnLoad(observer);
#endif
    }

    /// <summary>
    /// 内存不足事件
    /// </summary>
    private static void OnLowMemory()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Observer.memoryWarningCount++;
#endif

        //清空对象池
        Clear();

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

        Debug.Log("Call:" + name);
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
            dynamicObject = NewObject(name, components);
        }
        return dynamicObject;
    }

    /// <summary>
    /// 新建一个GameObject
    /// </summary>
    /// <returns></returns>
    public static GameObject NewObject(string name, params Type[] components)
    {
        GameObject dynamicObject = new GameObject(name, components);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
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

    /// <summary>
    /// 获取图集上的精灵
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Sprite GetSprite(string name)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!paths.ContainsKey(name))
        {
            Debug.LogError("找不到引用的图集：" + name);
        }
#endif
        string atlasName = paths[name];
        SpriteAtlas atlas = GetAsset<SpriteAtlas>(atlasName);
        return atlas.GetSprite(name);
    }

    /// <summary>
    /// 异步获取Asset
    /// </summary>
    /// <returns></returns>
    public static object GetAssetAsync<T>(string name, Action<T> callback)
    {
        /*
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!paths.ContainsKey(name))
        {
            Log("找不到映射的路径：" + name);
        }
#endif
        string path = paths[name];
        */

        return null;
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!paths.ContainsKey(name))
        {
            Debug.LogError("找不到映射的路径：" + name);
        }
#endif

        //获取路径
        string path = paths[name];

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
    /// 卸载AssetBundle
    /// </summary>
    public static void Unload(string assetBundleName, bool unloadAllLoadedObjects)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!assetBundles.ContainsKey(assetBundleName))
        {
            Debug.LogError("Unload: no found " + assetBundleName);
        }
#endif
        AssetBundle bundle = assetBundles[assetBundleName];
        bundle.Unload(unloadAllLoadedObjects);
        assetBundles.Remove(assetBundleName);
    }

    public static void Log(string content, LogType type = LogType.Log)
    {
        string output = "[Warehouser] " + content;
        switch (type)
        {
            case LogType.Warning:
                Debug.LogWarning(output);
                break;
            case LogType.Error:
                Debug.LogError(output);
                break;
            default:
                Debug.Log(output);
                break;
        }
    }
}
