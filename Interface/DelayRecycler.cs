/*
 * Author:  Rick
 * Create:  2017/8/20 14:23:59
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DelayRecycler
/// </summary>
public class DelayRecycler : MonoBehaviour, IRecycler
{
    public string name;
    public float delay;

    public void OnPushToPool()
    {
        gameObject.SetActive(false);
    }
    public void OnPullFromPool(params object[] args)
    {
        gameObject.SetActive(true);
    }

    public void OnEnable()
    {
        Invoke("Recycle", delay);
    }

    public void Recycle()
    {
        Warehouser.Recycle(gameObject);
    }
}
