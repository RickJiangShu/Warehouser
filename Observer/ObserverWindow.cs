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
    using System.Text.RegularExpressions;
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

        /// <summary>
        /// 名字正则检测
        /// </summary>
        private string regex = "";

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
                Dictionary<string,List<GameObject>> pool = global::Warehouser.pool;
                foreach (string name in all.Keys)
                {
                    Counter counter = new Counter();

                    counter.name = name;
                    counter.totalCount = 0;

                    foreach (GameObject go in all[name])
                    {
                        //过滤已被销毁的
                        if (go.Equals(null))
                            continue;

                        counter.totalCount++;
                        if (pool.ContainsKey(name))
                        {
                            counter.poolCount = pool[name].Count;
                        }
                        else
                        {
                            counter.poolCount = 0;
                        }
                    }

                    if(counter.totalCount > 0)
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

                    //数量过滤
                    if (c.totalCount <= limitCount)
                        continue;

                    //名字过滤
                    try
                    {
                        if (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(c.name, regex))
                            continue;
                    }
                    catch
                    {
                        //防止正则输入出错
                    }

                    info += string.Format(format, c.totalCount - c.poolCount, c.poolCount, c.totalCount, c.name);
                }

                info = string.Format(format, totalCount - poolCount, poolCount, totalCount, "Total") + info;
                info = info.Remove(info.Length - 1, 1);

                limitCount = (int)GUILayout.HorizontalSlider(limitCount, 0, 10);

                regex = GUILayout.TextField(regex);
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