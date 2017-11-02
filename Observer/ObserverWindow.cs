/*
 * Author:  Rick
 * Create:  2017/9/28 11:21:19
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
#if TEST
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
        private bool visible = false;

        /// <summary>
        /// > 多少显示
        /// </summary>
        private int limitCount = 0; 

        public void OnGUI()
        {
            if (GUILayout.Button("Observer"))
            {
                visible = !visible;
            }

            if (visible)
            {
                string format = "{0}/{1}/{2}\t{3}\n";
                
                List<Counter> counterList = new List<Counter>();
                Dictionary<string, List<GameObject>> all = global::Warehouser.allObjects;
                Dictionary<string,List<GameObject>> pool = global::Warehouser.objectsOfPool;
                foreach (string name in all.Keys)
                {
                    Counter counter = new Counter();
                    counter.name = name;
                    counter.totalCount = all[name].Count;
                    if (pool.ContainsKey(name))
                    {
                        counter.poolCount = pool[name].Count;
                    }
                    else
                    {
                        counter.poolCount = 0;
                    }
                    counterList.Add(counter);
                }

                counterList.Sort(SortFun);

                string info = "";
                int totalCount = 0;
                int poolCount = 0;
                foreach (Counter c in counterList)
                {
                    totalCount += c.totalCount;
                    poolCount += c.poolCount;
                    if (c.totalCount <= limitCount)
                        continue;

                    info += string.Format(format, c.totalCount - c.poolCount, c.poolCount, c.totalCount, c.name);
                }

                info = string.Format(format, totalCount - poolCount, poolCount, totalCount, "Total") + info;
                info = info.Remove(info.Length - 1, 1);

                limitCount = (int)GUILayout.HorizontalSlider(limitCount, 0, 10);

                GUILayout.TextArea(info);
            }
        }

        private int SortFun(Counter a, Counter b)
        {
            if (a.totalCount > b.totalCount)
                return -1;
            else if (b.totalCount > a.totalCount)
                return 1;
            else
                return 0;
        }

        private struct Counter
        {
            public string name;
            public int totalCount;
            public int poolCount;
        }
    }
}
#endif