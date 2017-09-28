/*
 * Author:  Rick
 * Create:  2017/8/1 15:47:14
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 回收器
/// </summary>
public interface IRecycler
{
    void OnPushToPool();
    void OnPullFromPool(params object[] args);
}
