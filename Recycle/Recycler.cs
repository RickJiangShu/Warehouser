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
/// 延迟回收器（不支持动态挂载，直接挂在Prefab上）
/// </summary>
public class Recycler : MonoBehaviour
{
    public float delay;

    public void OnEnable()
    {
        CancelInvoke();
        Invoke("Push", delay);
        
    }

    public void Push()
    {
        ObjectPool.global.Push(gameObject);
    }
}
