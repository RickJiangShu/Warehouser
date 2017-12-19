/*
 * Author:  Rick
 * Create:  2017/12/18 15:27:00
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AssetRequest
/// </summary>
public class AssetRequest<T> : IEnumerator where T : UnityEngine.Object
{
    public event Action<AssetBundle> onLoadBundleCompleted;

    private Phase _phase = Phase.None;

    private int _loadedDependencies = 0;
    private string[] _dependencies;//依赖包
    
    private string _bundleName;
    private string _assetName;

    //Bundle
    private AssetBundleCreateRequest _loadBundleRequest;
    private AssetBundle _mainBundle;

    //Asset
    private AssetBundleRequest _loadAssetRequest;

    public AssetRequest(string[] dependencies, string bundleName, string assetName)
    {
        _dependencies = dependencies;
        _bundleName = bundleName;
        _assetName = assetName;
        LoadDependencies();
    }

    public AssetRequest(string bundleName, string assetName)
    {
        _bundleName = bundleName;
        _assetName = assetName;
        LoadMainBundle();
    }

    public AssetRequest(AssetBundle bundle, string assetName)
    {
        _mainBundle = bundle;
        _assetName = assetName;
        LoadAsset();
    }

    public AssetRequest()
    {
    }

    private void LoadDependencies()
    {
        _phase = Phase.LoadDependencies;
        _loadBundleRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + _dependencies[_loadedDependencies]);
    }

    private void LoadMainBundle()
    {
        _phase = Phase.LoadMainBundle;
        _loadBundleRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + _bundleName);
    }

    private void LoadAsset()
    {
        _phase = Phase.LoadAsset;
        _loadAssetRequest = _mainBundle.LoadAssetAsync(_assetName);
    }

    public virtual bool MoveNext()
    {
        switch (_phase)
        {
            case Phase.LoadDependencies:
                if (_loadBundleRequest.isDone)
                {
                    onLoadBundleCompleted(_loadBundleRequest.assetBundle);
                    if (++_loadedDependencies == _dependencies.Length)
                    {
                        LoadMainBundle();
                    }
                    else
                    {
                        LoadDependencies();
                    }
                }
                return true;
            case Phase.LoadMainBundle:
                if (_loadBundleRequest.isDone)
                {
                    onLoadBundleCompleted(_loadBundleRequest.assetBundle);
                    _mainBundle = _loadBundleRequest.assetBundle;
                    LoadAsset();
                }
                return true;
            case Phase.LoadAsset:
                return !_loadAssetRequest.isDone;
        }
        return false;
    }

    public virtual float progress
    {
        get {
            switch (_phase)
            {
                case Phase.LoadDependencies:
                case Phase.LoadMainBundle:
                    return (_loadedDependencies + _loadBundleRequest.progress + 1) / (2 + _dependencies.Length);
                case Phase.LoadAsset:
                    return (_dependencies.Length + _loadAssetRequest.progress) / (2 + _dependencies.Length);
            }
            return 1f;
        }
    }

    public T asset
    {
        get {
            return (T)_loadAssetRequest.asset;
        }
    }

    public object Current
    {
        get { return null; }
    }
    public void Reset()
    {
        throw new NotSupportedException();
    }
    public void Dispose()
    {
        throw new NotSupportedException();
    }

    private enum Phase
    {
        None,
        LoadDependencies,
        LoadMainBundle,
        LoadAsset
    }
}
