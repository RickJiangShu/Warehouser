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
        private bool visible = true;

        public void OnGUI()
        {
            if (GUILayout.Button("Observer"))
            {
                visible = !visible;
            }

            if (visible)
            {
                string format = "{0}/{1}/{2}\t{3}\n";
                Dictionary<string, Counter> counters = new Dictionary<string, Counter>();

                for (int i = 0, l = Observer.allObjects.Count; i < l; i++)
                {
                    GameObject go = Observer.allObjects[i];
                    string name = go.name;
                    Counter counter;

                    if (!counters.TryGetValue(name, out counter))
                    {
                        counter = new Counter();
                        counter.name = name;
                        counters.Add(name, counter);
                    }
                    counter.total++;

                    if (
                        global::Warehouser.objectsOfPool.ContainsKey(go.name) &&
                        global::Warehouser.objectsOfPool[go.name].Contains(go)
                        )
                    {
                        counter.pool++;
                    }

                }

                List<Counter> counterList = new List<Counter>(counters.Values);
                counterList.Sort(SortFun);

                string info = "";
                int allTotal = 0;
                int allPool = 0;
                foreach (Counter c in counterList)
                {
                    if (c.total < 2)
                        continue;

                    allTotal += c.total;
                    allPool += c.pool;

                    info += string.Format(format, c.alive, c.pool, c.total, c.name);
                }

                info = string.Format(format, allTotal - allPool, allPool, allTotal, "Total") + info;
                info = info.Remove(info.Length - 1, 1);

                GUILayout.TextField(info);
            }
        }

        private int SortFun(Counter a, Counter b)
        {
            if (a.total > b.total)
                return -1;
            else if (b.total > a.total)
                return 1;
            else
                return 0;
        }

        private class Counter
        {
            public string name;
            public int total;
            public int pool;
            public int alive
            {
                get { return total - pool; }
            }
        }
    }
}