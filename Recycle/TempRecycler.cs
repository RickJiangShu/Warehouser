/*
 * Author:  Rick
 * Create:  2017/12/11 16:45:46
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 临时回收器（用完就删除，通过脚本添加）
/// </summary>
public class TempRecycler : MonoBehaviour
{
    public float duration;
    // Use this for initialization
    void Start()
    {
        Invoke("Recycle", duration);
    }

    void Recycle()
    {
        ObjectPool.global.Push(gameObject);
        Destroy(this);
    }
}
