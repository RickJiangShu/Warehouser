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
    using UnityEngine.Profiling;

    /// <summary>
    /// 观察者窗口
    /// </summary>
    public class ObserverWindow : MonoBehaviour
    {
        /// <summary>
        /// 是否显示
        /// </summary>
        private bool detailVisible = false;

        /// <summary>
        /// > 多少显示
        /// </summary>
        private int limitCount = 0;

        /// <summary>
        /// 名字正则检测
        /// </summary>
        private string regex = "";

        /// <summary>
        /// 对象计数器显示格式
        /// </summary>
        private const string CounterFormat = "{0}/{1}/{2}\t{3}";

        /// <summary>
        /// fps下一次刷新时间
        /// </summary>
        private float fpsNextUpdate = 0.0f;

        /// <summary>
        /// 帧频
        /// </summary>
        private float fps = 0.0f;

        /// <summary>
        /// 内存峰值
        /// </summary>
        private long memoryMax = 0;

        void Start()
        {
            fpsNextUpdate = Time.time;
        }
        void OnDestory()
        {
        }
        void Update()
        {
            if (Time.time > fpsNextUpdate)
            {
                fps = 1.0f / Time.deltaTime;
                fpsNextUpdate += 1.0f;
            }
        }

        public void OnGUI()
        {
            string baseInfo = "";
            string detailInfo = "";

            //FPS
            baseInfo = "FPS:\t" + fps.ToString("0.0");

            //Memory
            long memory = Profiler.GetTotalAllocatedMemoryLong();
            if (memory > memoryMax)
                memoryMax = memory;
            string unit = "";
            baseInfo += "\nMemory:\t" + MemoryOutputFormat(memory, out unit) + "/" + MemoryOutputFormat(memoryMax, out unit);

            //计数
            List<Counter> counters = CalcCounter();
            int totalCount = 0;
            int poolCount = 0;
            long totalMemory = 0;
            foreach (Counter c in counters)
            {
                totalCount += c.totalCount;
                poolCount += c.poolCount;

                if (detailVisible)
                {
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
                    detailInfo += string.Format(CounterFormat, c.totalCount - c.poolCount, c.poolCount, c.totalCount, c.name) + "\n";
                }
            }

            baseInfo += "\nObjects:\t" + (totalCount - poolCount) + "/" + poolCount + "/" + totalCount;
            GUILayout.TextField(baseInfo);

            if (GUILayout.Button("Detail"))
            {
                detailVisible = !detailVisible;
            }

            if (detailVisible)
            {
                limitCount = (int)GUILayout.HorizontalSlider(limitCount, 0, 10);
                regex = GUILayout.TextField(regex);
                GUILayout.TextArea(detailInfo);
            }
        }

        private List<Counter> CalcCounter()
        {
            List<Counter> counterList = new List<Counter>();
            Dictionary<string, List<GameObject>> all = global::Warehouser.allObjects;
            Dictionary<string, List<GameObject>> pool = global::Warehouser.pool;
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

                if (counter.totalCount > 0)
                    counterList.Add(counter);
            }

            counterList.Sort(SortFun);
            return counterList;
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

        /// <summary>
        /// 内存输出格式化
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        private string MemoryOutputFormat(long memory, out string unit)
        {
            if (memory < 1024)
            {
                unit = "B";
                return memory.ToString();
            }

            if (memory < 1048576)
            {
                unit = "KB";
                return (memory / 1024f).ToString("0.0");
            }

            if (memory < 1073741824)
            {
                unit = "MB";
                return (memory / 1048576f).ToString("0.0");
            }

            unit = "GB";
            return (memory / 1073741824f).ToString("0.0");
        }
    }
}
#endif