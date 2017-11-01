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
                string format = "{0}\t{1}\n";
                
                List<Counter> counterList = new List<Counter>();
                Dictionary<string,List<GameObject>> pool = global::Warehouser.objectsOfPool;
                foreach(string name in pool.Keys)
                {
                    Counter counter = new Counter();
                    counter.name = name;
                    counter.count = pool[name].Count;
                    counterList.Add(counter);
                }

                counterList.Sort(SortFun);

                string info = "";
                int total = 0;
                foreach (Counter c in counterList)
                {
                    total += c.count;
                    if (c.count <= limitCount)
                        continue;

                    info += string.Format(format, c.count, c.name);
                }

                info = string.Format(format, total, "Total") + info;
                info = info.Remove(info.Length - 1, 1);

                limitCount = (int)GUILayout.HorizontalSlider(limitCount, 0, 10);

                GUILayout.TextArea(info);
            }
        }

        private int SortFun(Counter a, Counter b)
        {
            if (a.count > b.count)
                return -1;
            else if (b.count > a.count)
                return 1;
            else
                return 0;
        }

        private class Counter
        {
            public string name;
            public int count;
        }
    }
}
#endif