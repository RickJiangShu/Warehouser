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

                /*
                Debug.Log("GetMonoHeapSizeLong:" + Profiler.GetMonoHeapSizeLong() / 1048576);
                Debug.Log("GetMonoUsedSizeLong:" + Profiler.GetMonoUsedSizeLong() / 1048576);

                Debug.Log("usedHeapSizeLong:" + Profiler.usedHeapSizeLong / 1048576);
                Debug.Log("GetTotalAllocatedMemoryLong:" + Profiler.GetTotalAllocatedMemoryLong() / 1048576);
                Debug.Log("GetTotalReservedMemoryLong:" + Profiler.GetTotalReservedMemoryLong() / 1048576);
                Debug.Log("GetTempAllocatorSize:" + Profiler.GetTempAllocatorSize() / 1048576);
                Debug.Log("GetTotalUnusedReservedMemoryLong:" + Profiler.GetTotalUnusedReservedMemoryLong() / 1048576);
                 */
            }
        }

        public void OnGUI()
        {
            string baseInfo = "";

            //FPS
            baseInfo = "FPS:\t" + fps.ToString("0.0");

            //Memory
            long memory = Profiler.GetTotalAllocatedMemoryLong();
            if (memory > memoryMax)
                memoryMax = memory;
            baseInfo += "\nMemory:\t" + (memory / 1048576f).ToString("N1") + " / " + (memoryMax / 1048576f).ToString("N1") + " M";

            //计数
            int objectCount = 0;
            int poolCount = 0;
            long objectMemory = 0;
            Dictionary<string, List<GameObject>> all = global::Warehouser.allObjects;
            Dictionary<string, List<GameObject>> pool = global::Warehouser.pool;
            foreach (string name in all.Keys)
            {
                foreach (GameObject obj in all[name])
                {
                    if (obj.Equals(null))
                        continue;

                    objectCount++;
                    if (pool.ContainsKey(name) && pool[name].Contains(obj))
                        poolCount++;

                    objectMemory += Profiler.GetRuntimeMemorySizeLong(obj);
                }
            }

            baseInfo += "\nObjects:\t" + (objectCount - poolCount) + " / " + poolCount + " / " + objectCount;
            if (objectMemory > 0f)
                baseInfo += " (" + MemoryOutputFormat(objectMemory) + ")";

           
            GUILayout.TextField(baseInfo);

            if (GUILayout.Button("Detail"))
            {
                detailVisible = !detailVisible;
            }

            if (detailVisible)
            {
                string detailInfo = "";

                regex = GUILayout.TextField(regex);

                List<Counter> counters = new List<Counter>();

                //填入Counter
                foreach (string name in all.Keys)
                {
                    Counter counter = new Counter();
                    counter.name = name;
                    counter.totalCount = 0;

                    foreach (GameObject obj in all[name])
                    {
                        //过滤已被销毁的
                        if (obj.Equals(null))
                            continue;

                        //过滤名字正则
                        try
                        {
                            if (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(counter.name, regex))
                                continue;
                        }
                        catch { };

                        counter.totalCount++;
                        counter.memory += Profiler.GetRuntimeMemorySizeLong(obj);
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
                        counters.Add(counter);
                }

                //排序
                counters.Sort(SortMemory);

                //写入文本
                foreach (Counter counter in counters)
                {
                    detailInfo += (counter.totalCount - counter.poolCount) + "/" + counter.poolCount + "/" + counter.totalCount;

                    if (counter.memory > 0f)
                        detailInfo += "\t" + MemoryOutputFormat(counter.memory);

                    detailInfo += "\t" + counter.name + "\n";
                }

                if (detailInfo.Length > 0)
                {
                    GUILayout.TextArea(detailInfo.Remove(detailInfo.Length - 1, 1));
                }
                else
                {
                    GUILayout.TextArea("Empty");
                }
            }
        }

        private int SortMemory(Counter a, Counter b)
        {
            if (a.memory > b.memory)
                return -1;
            else if (b.memory > a.memory)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// 对象计数
        /// </summary>
        private struct Counter
        {
            public string name;
            public int totalCount;
            public int poolCount;
            public long memory;
        }

        /// <summary>
        /// 内存输出格式化
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        private string MemoryOutputFormat(long memory)
        {
            if (memory < 1024)
            {
                return memory.ToString() + " B";
            }

            if (memory < 1048576)
            {
                return (memory / 1024f).ToString("0.0") + " K";
            }

            if (memory < 1073741824)
            {
                return (memory / 1048576f).ToString("0.0") + " M";
            }

            return (memory / 1073741824f).ToString("0.0") + " G";
        }
    }
}
#endif