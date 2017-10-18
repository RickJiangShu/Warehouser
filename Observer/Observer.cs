/*
 * Author:  Rick
 * Create:  2017/10/18 11:30:20
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
namespace Plugins.Warehouser.Observer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 观察者，挂载于对象身上用于观察对象。
    /// </summary>
    public class Observer : MonoBehaviour
    {
        public static List<GameObject> allObjects = new List<GameObject>();//所有通过Warehouser创建的对象

        void Awake()
        {
            allObjects.Add(gameObject);
        }

        public void OnDestroy()
        {
            allObjects.Remove(gameObject);
        }
    }
}

