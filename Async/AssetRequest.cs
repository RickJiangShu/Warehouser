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
public class AssetRequest<T> : IEnumerator<T> where T : UnityEngine.Object
{
    private string _bundlePath;
    private string _assetName;
    private Action<T> _completeCallback;
    private Action<float> _progressCallback;

    private AssetBundleCreateRequest _loadBundleRequest;
    private AssetBundleRequest _loadAssetRequest;

    public AssetRequest(string bundlePath, string assetName, Action<float> progressCallback, Action<T> completeCallback)
        : this(bundlePath, assetName, completeCallback)
    {
        _progressCallback = progressCallback;
    }

    public AssetRequest(string bundlePath, string assetName, Action<T> completeCallback)
    {
        _bundlePath = bundlePath;
        _assetName = assetName;
        _loadBundleRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + _bundlePath);
        _completeCallback = completeCallback;
    }

    public bool MoveNext()
    {
        if (!_loadBundleRequest.isDone)
        {
            if (_progressCallback != null)
                _progressCallback(_loadBundleRequest.progress / 2f);
            return true;
        }
        else
        {
            if (_loadAssetRequest == null)
            {
                _loadAssetRequest = _loadBundleRequest.assetBundle.LoadAssetAsync(_assetName);
                Warehouser.assetBundles.Add(_bundlePath, _loadBundleRequest.assetBundle);
            }

            if (!_loadAssetRequest.isDone)
            {
                if (_progressCallback != null)
                    _progressCallback((1f + _loadAssetRequest.progress) / 2f);
                return true;
            }
            else
            {
                _completeCallback((T)_loadAssetRequest.asset);
                return false;
            }
        }
    }

    public object Current
    {
        get { return null; }
    }
    T IEnumerator<T>.Current
    {
        get { throw new NotSupportedException(); }
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