/*
 * Author:  Rick
 * Create:  2017/8/4 15:49:53
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 默认Recycler
/// </summary>
public sealed class Recycler : MonoBehaviour,IRecycler
{
    public void OnPushToPool()
    {
        gameObject.SetActive(false);
    }
    public void OnPullFromPool()
    {
        gameObject.SetActive(true);
    }
}
