/*
 * Author:  Rick
 * Create:  2017/9/28 11:21:19
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Plugins.Warehouser
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.Profiling;

    /// <summary>
    /// 观察者窗口
    /// </summary>
    public class Observer : MonoBehaviour
    {
        /// <summary>
        /// 默认分辨率
        /// </summary>
        //private Vector2 designResolution = new Vector2(499f, 888f);

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
        private float nextUpdateTime = 0.0f;

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
        /// 报错次数
        /// </summary>
        public int errorCount = 0;

        /// <summary>
        /// 错误信息
        /// </summary>
        private string errorInfo = "";

        /// <summary>
        /// FPS警告限制
        /// </summary>
        private float fpsWarningLimit = 1f / 10f;

        /// <summary>
        /// 开始时间
        /// </summary>
        private float startTime = 0;

        /// <summary>
        /// 滑动位置
        /// </summary>
        private Vector2[] scrollPositions = new Vector2[3];

        /// <summary>
        /// 警告样式
        /// </summary>
        private GUIStyle warningStyle;

        /// <summary>
        /// 内存计算结果缓存，防止重复计算
        /// </summary>
        private Dictionary<string, long> resultCache = new Dictionary<string, long>();

        /// <summary>
        /// 基本信息
        /// </summary>
        private string info;
        private StringBuilder infoBuilder = new StringBuilder();

        /// <summary>
        /// 对象详情信息
        /// </summary>
        private string objectsInfo;
        private StringBuilder objectsInfoBuilder = new StringBuilder();

        /// <summary>
        /// AB 详情信息
        /// </summary>
        private string bundlesInfo;
        private StringBuilder bundlesInfoBuilder = new StringBuilder();


        void Awake()
        {
            warningStyle = new GUIStyle();
            warningStyle.fontStyle = FontStyle.Bold;
            warningStyle.fontSize = 16;
            warningStyle.normal.textColor = Color.red;

            if (Application.isMobilePlatform)
            {
                Application.logMessageReceivedThreaded += OnReceiveLogMessage;
            }
        }


        void Start()
        {
            startTime = Time.realtimeSinceStartup;
            nextUpdateTime = Time.time;
        }

        void OnReceiveLogMessage(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Log || type == LogType.Warning)
                return;

            errorCount++;
            errorInfo += condition + "\n" + stackTrace + "\n";
        }

        void Update()
        {
            if (Time.deltaTime > fpsWarningLimit)
            {
         //       Debug.LogError("FPS Warrning");
            }

            if (Time.time > nextUpdateTime)
            {
                fps = 1.0f / Time.deltaTime;
                CalcInfo();

                nextUpdateTime += 1.0f;
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


        private void CalcInfo()
        {
            //Clear
            infoBuilder.Length = 0;

            //Version
            infoBuilder.Append("Version:\t").Append(Application.version);

            //Time
            int seconds = (int)(Time.realtimeSinceStartup - startTime);
            int minutes = seconds / 60;
            infoBuilder.Append("\nTime:\t").AppendFormat("{0:00}:{1:00}", seconds / 60, seconds % 60);

            //FPS
            infoBuilder.Append("\nFPS:\t").Append((int)fps);

            //Memory
            long memory = Profiler.GetTotalAllocatedMemoryLong();
            if (memory > memoryMax)
                memoryMax = memory;

            infoBuilder.Append("\nMemory:\t")
                .Append((memory / 1048576f).ToString("N1"))
                .Append(" / ")
                .Append((memoryMax / 1048576f).ToString("N1"))
                .Append(" M");

            //Objects
            int objectCount = 0;
            int poolCount = 0;
            Dictionary<string, List<GameObject>> all = global::Warehouser.allObjects;
            Dictionary<string, Queue<GameObject>> pool = ObjectPool.global.objects;
            foreach (string name in all.Keys)
            {
                foreach (GameObject obj in all[name])
                {
                    if (obj.Equals(null))
                        continue;

                    objectCount++;
                    if (pool.ContainsKey(name) && pool[name].Contains(obj))
                        poolCount++;
                }
            }

            infoBuilder.Append("\nObjects:\t")
                .Append((objectCount - poolCount))
                .Append(" / ")
                .Append(poolCount)
                .Append(" / ")
                .Append(objectCount);

            //Bundles
            Dictionary<string, AssetBundle> bundles = global::Warehouser.assetBundles;
            long bundlesMemory = 0;

            foreach (AssetBundle bundle in bundles.Values)
            {
                long m;
                if(!resultCache.TryGetValue(bundle.name, out m))
                    m = Profiler.GetRuntimeMemorySizeLong(bundle);

                bundlesMemory += m;
            }

            infoBuilder.Append("\nBundles:\t")
                .Append(bundles.Keys.Count)
                .Append(" (")
                .Append(ConvertBytes(bundlesMemory))
                .Append(")");

            info = infoBuilder.ToString();

            //计算详情
            if (detailVisible)
            {
                #region Count Objects
                objectsInfoBuilder.Length = 0;

                List<Counter> objectCounters = new List<Counter>();

                //填入Counter
                foreach (string name in all.Keys)
                {
                    Counter counter = new Counter();
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
                objectCounters.Sort(SortCount);

                objectsInfoBuilder.Append("Objects:\n");

                //写入文本
                foreach (Counter counter in objectCounters)
                {
                    objectsInfoBuilder.Append((counter.count - counter.poolCount))
                        .Append("/")
                        .Append(counter.poolCount)
                        .Append("/")
                        .Append(counter.count)
                        .Append("\t")
                        .Append(counter.name)
                        .Append("\n");
                }

                objectsInfo = objectsInfoBuilder.Remove(objectsInfoBuilder.Length - 1, 1).ToString();
                #endregion

                #region Count Bundles
                bundlesInfoBuilder.Length = 0;

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
                    long m;
                    if (!resultCache.TryGetValue(bundle.name, out m))
                        m = Profiler.GetRuntimeMemorySizeLong(bundle);

                    counter.memory = m;
                    bundleCounters.Add(counter);
                }

                //排序
                bundleCounters.Sort(SortMemory);

                bundlesInfoBuilder.Append("Asset Bundles:\n");

                //写入文本
                foreach (Counter counter in bundleCounters)
                {
                    bundlesInfoBuilder.Append(ConvertBytes(counter.memory))
                        .Append("\t")
                        .Append(counter.name)
                        .Append("\n");
                }

                bundlesInfo = bundlesInfoBuilder.Remove(bundlesInfoBuilder.Length - 1, 1).ToString();
                #endregion
            }
        }

        public void OnGUI()
        {
            //分辨率（会影响布局）
        //    float resX = Screen.width / designResolution.x;
        //    float resY = Screen.height / designResolution.y;
        //    GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(resX, resY, 1));

            if (errorCount > 0)
            {
                GUILayout.Label("Error: " + errorCount, warningStyle);
            }

            if (memoryWarningCount > 0)
            {
                GUILayout.Label("Memory Warrning: " + memoryWarningCount, warningStyle);
            }
           
            //显示baseInfo
            GUILayout.TextField(info);

            if (GUILayout.Button("Detail"))
            {
                detailVisible = !detailVisible;

                if (detailVisible)
                    CalcInfo();
            }

            if (detailVisible)
            {
                regex = GUILayout.TextField(regex);

                //Objects
                scrollPositions[0] = GUILayout.BeginScrollView(scrollPositions[0], GUILayout.Width(300), GUILayout.Height(100));
                GUILayout.TextField(objectsInfo);
                GUILayout.EndScrollView();

                //Asset Bundles
                GUILayout.Space(4f);
                scrollPositions[1] = GUILayout.BeginScrollView(scrollPositions[1], GUILayout.Width(300), GUILayout.Height(100));
                GUILayout.TextField(bundlesInfo);
                GUILayout.EndScrollView();
                
                //Errors
                if (errorCount > 0)
                {
                    scrollPositions[2] = GUILayout.BeginScrollView(scrollPositions[2]);
                    GUILayout.TextArea(errorInfo.Remove(errorInfo.Length - 2));
                    GUILayout.EndScrollView();
                }
            }
        }

        private int SortCount(Counter a, Counter b)
        {
            if (a.count > b.count)
                return -1;
            else if (b.count > a.count)
                return 1;
            else
                return 0;
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
        private struct Counter
        {
            public string name;
            public int count;
            public int poolCount;
            public long memory;
        }


        /// <summary>
        /// 内存输出格式化
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ConvertBytes(long bytes)
        {
            if (bytes < 1024)
            {
                return bytes.ToString() + " B";
            }

            if (bytes < 1048576)
            {
                return (bytes / 1024f).ToString("0.0") + " K";
            }

            if (bytes < 1073741824)
            {
                return (bytes / 1048576f).ToString("0.0") + " M";
            }

            return (bytes / 1073741824f).ToString("0.0") + " G";
        }
    }
}
#endif