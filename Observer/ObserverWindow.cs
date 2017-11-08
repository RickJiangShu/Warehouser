/*
 * Author:  Rick
 * Create:  2017/9/28 11:21:19
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
        /// 默认分辨率
        /// </summary>
        private Vector2 designResolution = new Vector2(499f, 888f);

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

        /// <summary>
        /// 内存警告错误
        /// </summary>
        public static int memoryWarningCount = 0;
        
        /// <summary>
        /// 滑动位置
        /// </summary>
        private Vector2[] scrollPositions = new Vector2[3];

        /// <summary>
        /// 开始时间
        /// </summary>
        private static float startTime = 0;

        void Start()
        {
            startTime = Time.realtimeSinceStartup;
            fpsNextUpdate = Time.time;
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
            //分辨率
            float resX = Screen.width / designResolution.x;
            float resY = Screen.height / designResolution.y;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(resX, resY, 1));

            if (memoryWarningCount > 0)
            {
                GUIStyle warningStyle = new GUIStyle();
                warningStyle.fontStyle = FontStyle.Bold;
                warningStyle.fontSize = 16;
                warningStyle.normal.textColor = Color.red;
                GUILayout.Label("Memory Warrning: " + memoryWarningCount, warningStyle);
            }

            string baseInfo = "";
            
            //Time
            int seconds = (int)(Time.realtimeSinceStartup - startTime);
            baseInfo += "Time:\t" + string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);

            //FPS
            baseInfo += "\nFPS:\t" + fps.ToString("0.0");

            //Memory
            long memory = Profiler.GetTotalAllocatedMemoryLong();
            if (memory > memoryMax)
                memoryMax = memory;
            baseInfo += "\nMemory:\t" + (memory / 1048576f).ToString("N1") + " / " + (memoryMax / 1048576f).ToString("N1") + " M";

            //Objects
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

            //Bundles
            Dictionary<string, AssetBundle> bundles = global::Warehouser.assetBundles;
            long bundlesMemory = 0;
            foreach (AssetBundle bundle in bundles.Values)
            {
                bundlesMemory += Profiler.GetRuntimeMemorySizeLong(bundle);
            }
            baseInfo += "\nBundles:\t" + bundles.Keys.Count + " (" + MemoryOutputFormat(bundlesMemory) + ")";

           
            //显示baseInfo
            GUILayout.TextField(baseInfo);

            if (GUILayout.Button("Detail"))
            {
                detailVisible = !detailVisible;
            }

            if (detailVisible)
            {
                string detailInfo = "";

                regex = GUILayout.TextField(regex);

                #region Count Objects
                List<ObjectCounter> objectCounters = new List<ObjectCounter>();

                //填入Counter
                foreach (string name in all.Keys)
                {
                    ObjectCounter counter = new ObjectCounter();
                    counter.name = name;
                    counter.count = 0;

                    foreach (GameObject obj in all[name])
                    {
                        //过滤已被销毁的
                        if (obj.Equals(null))
                            continue;

                        //过滤名字正则
                        try
                        {
                            if (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(name, regex))
                                continue;
                        }
                        catch { };

                        counter.count++;
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

                    if (counter.count > 0)
                        objectCounters.Add(counter);
                }

                //排序
                objectCounters.Sort(SortMemory);

                detailInfo = "Objects:\n";

                //写入文本
                foreach (ObjectCounter counter in objectCounters)
                {
                    detailInfo += MemoryOutputFormat(counter.memory) + "\t" +　(counter.count - counter.poolCount) + "/" + counter.poolCount + "/" + counter.count + "\t" + counter.name + "\n";
                }

                scrollPositions[0] = GUILayout.BeginScrollView(scrollPositions[0], GUILayout.Width(300), GUILayout.Height(100));
                GUILayout.TextField(detailInfo.Remove(detailInfo.Length - 1, 1));
                GUILayout.EndScrollView();
                #endregion

                #region Count Bundles
                List<Counter> bundleCounters = new List<Counter>();

                foreach (string name in bundles.Keys)
                {
                    AssetBundle bundle = bundles[name];

                    //过滤名字正则
                    try
                    {
                        if (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(name, regex))
                            continue;
                    }
                    catch { };

                    Counter counter = new Counter();
                    counter.name = name;
                    counter.memory = Profiler.GetRuntimeMemorySizeLong(bundle);
                    bundleCounters.Add(counter);
                }

                //排序
                bundleCounters.Sort(SortMemory);

                detailInfo = "Bundles:\n";

                //写入文本
                foreach (Counter counter in bundleCounters)
                {
                    detailInfo += MemoryOutputFormat(counter.memory) + "\t" + counter.name + "\n";
                }

                GUILayout.Space(4f);
                scrollPositions[2] = GUILayout.BeginScrollView(scrollPositions[2], GUILayout.Width(300), GUILayout.Height(100));
                GUILayout.TextField(detailInfo.Remove(detailInfo.Length - 1, 1));
                GUILayout.EndScrollView();
                #endregion
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
        /// 计数器
        /// </summary>
        private class Counter
        {
            public string name;
            public int count;
            public long memory;
        }

        /// <summary>
        /// 对象计数器
        /// </summary>
        private class ObjectCounter : Counter
        {
            public int poolCount;
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