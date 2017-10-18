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
        public void OnGUI()
        {
            int allCount = Observer.allObjects.Count;
            int poolCount = 0;
            for(int i = 0,l = allCount;i<l;i++)
            {
                GameObject go = Observer.allObjects[i];
                if (
                    global::Warehouser.objectsOfPool.ContainsKey(go.name) &&
                    global::Warehouser.objectsOfPool[go.name].Contains(go)
                    )
                {
                    poolCount++;
                }
            }

            GUILayout.TextField(
                    string.Format("objects: {0} / {1} / {2}", allCount - poolCount, poolCount, allCount)
                    );
            
        }
    }
}