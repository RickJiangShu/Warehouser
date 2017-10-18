/*
 * Author:  Rick
 * Create:  2017/9/28 11:21:19
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
namespace Plugins.Warehouser.Observer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 观察者窗口
    /// </summary>
    public class ObserverWindow : MonoBehaviour
    {
        /// <summary>
        /// 是否显示
        /// </summary>
        private bool isShow = false;

        public void OnGUI()
        {
            if (GUILayout.Button("Observer"))
            {
                isShow = !isShow;
            }

            if (isShow)
            {
                GUILayout.TextField(
                    "all: " + Observer.allObjects.Count
                    );

                //计算对象池中对象数量
                /*
                int objectCountOfPool = 0;
                foreach (List<Object> objs in ObjectPool.objectsOfPool.Values)
                {
                    objectCountOfPool += objs.Count;
                }

                GUILayout.TextField(
                "in scene: " + instanceNumber +
                "\nin pool:" + objectCountOfPool +
                "\n\nget instance count: " + getInstanceCount +
                "\nget asset count: " + getAssetCount +
                "\nrecycle count: " + recycleCount +
                "\ndestroy count: " + destroyCount +
                "\nunload asset count: " + unloadAssetCount
                );
                 */
            }
        }
    }
}