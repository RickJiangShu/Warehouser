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
/// 延迟回收器（不支持动态挂载）
/// </summary>
public class DelayPusher : MonoBehaviour
{
    public string name;
    public float delay;

    public void OnEnable()
    {
        Invoke("Recycle", delay);
    }

    public void Push()
    {
        Warehouser.Push(gameObject);
    }
}
