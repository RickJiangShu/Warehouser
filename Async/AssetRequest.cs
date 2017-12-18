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
    private string _bundlePath;
    private string _assetName;

    private AssetBundleCreateRequest _loadBundleRequest;
    private AssetBundleRequest _loadAssetRequest;
    private AssetBundle _loadedAssetBundle;

    public AssetRequest(AssetBundle assetBundle, string assetName)
    {
        _loadedAssetBundle = assetBundle;
        _assetName = assetName;
        LoadAsset();
    }

    public AssetRequest(string bundlePath, string assetName)
    {
        _bundlePath = bundlePath;
        _assetName = assetName;
        LoadAssetBundle();
    }

    private void LoadAssetBundle()
    {
        _loadBundleRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + _bundlePath);
    }
    private void LoadAsset()
    {
        _loadAssetRequest = _loadedAssetBundle.LoadAssetAsync(_assetName);
    }

    public virtual bool MoveNext()
    {
        if (_loadedAssetBundle == null)
        {
            if (_loadBundleRequest.isDone)
            {
                _loadedAssetBundle = _loadBundleRequest.assetBundle;//引用加载的Bundle
                Warehouser.assetBundles.Add(_bundlePath, _loadedAssetBundle);
                LoadAsset();
            }
            return true;
        }
        else
        {
            return !_loadAssetRequest.isDone;
        }
    }

    public float progress
    {
        get {
            if (_loadedAssetBundle == null)
            {
                return _loadBundleRequest.progress / 2f;
            }
            else
            {
                return (1f + _loadAssetRequest.progress) / 2f;
            }
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
}