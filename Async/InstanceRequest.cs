/*
 * Author:  Rick
 * Create:  2017/12/18 16:13:45
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ObjectRequest
/// </summary>

public class InstanceRequest : AssetRequest<GameObject>
{
    private GameObject _instance;


    public InstanceRequest(string[] dependencies, string bundleName, string assetName) : base(dependencies, bundleName, assetName) { }
    public InstanceRequest(AssetBundle bundle, string assetName) : base(bundle, assetName) { }
    public InstanceRequest(string bundleName, string assetName) : base(bundleName, assetName) { }
    

    public InstanceRequest(GameObject obj)
    {
        this._instance = obj;
    }

    public override bool MoveNext()
    {
        if (_instance == null)
        {
            if (!base.MoveNext())
            {
                _instance = Warehouser.Instantiate(asset);
                return false;
            }
            return true;
        }
        return false;
    }

    public override float progress
    {
        get
        {
            return base.progress;
        }
    }

    public GameObject instance
    {
        get { return _instance; }
    }
}
