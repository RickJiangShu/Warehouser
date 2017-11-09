/*
 * Author:  Rick
 * Create:  2017/8/2 13:45:24
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace Plugins.Warehouser.Editor
{
    /// <summary>
    /// 映射器编辑器
    /// </summary>
    public class WarehouserWindow : EditorWindow
    {
        /// <summary>
        /// 配置文件路径
        /// </summary>
        public const string SETTINGS_PATH = "Assets/WarehouserSettings.asset";

        /// <summary>
        /// Pairs路径
        /// </summary>
        public const string PAIRS_PATH = "Assets/Resources/WarehouserPairs.asset";

        /// <summary>
        /// 配置文件
        /// </summary>
        public Setting settings;

        /// <summary>
        /// 滚动条坐标
        /// </summary>
        private Vector2 scrollPos;

        [MenuItem("Window/Warehouser")]
        public static WarehouserWindow Get()
        {
            return EditorWindow.GetWindow<WarehouserWindow>("Warehouser");
        }

        void Awake()
        {
            LoadSettings();  
        }

        public void OnGUI()
        {
            SerializedObject so = new SerializedObject(settings);

            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            //AB Packager
            GUILayout.Label("Asset Bundle Packager", EditorStyles.boldLabel);
            
            //extension
        //    GUILayout.Label("Default Extension:");
            settings.extension = EditorGUILayout.TextField("Extension:", settings.extension);

            SerializedProperty abPackages = so.FindProperty("assetBundlePackages");
            EditorGUILayout.PropertyField(abPackages, true);

            EditorGUILayout.Space();

            //Mapper
            GUILayout.Label("Mapper", EditorStyles.boldLabel);

            //显示MapPaths
            SerializedProperty mapPathsProp = so.FindProperty("mapPaths");
            EditorGUILayout.PropertyField(mapPathsProp, true);
            
            EditorGUILayout.Space();

            GUILayout.Label("Operation", EditorStyles.boldLabel);

            if (GUILayout.Button("Print"))
            {
                Print(settings.extension);
            }

            if(GUILayout.Button("Clear"))
            {
                AssetBundlePackager.Clear(settings.assetBundlePackages, settings.extension);
            }

            if (GUILayout.Button("Pack"))
            {
                AssetBundlePackager.Pack(settings.assetBundlePackages, settings.extension);
            }

            if (GUILayout.Button("Map"))
            {
                MapperEditor.Map(settings.mapPaths.ToArray(), PAIRS_PATH);
            }

            if (GUILayout.Button("Build Asset Bundles"))
            {
                BuildAssetBundles();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            if (GUI.changed)
            {
                so.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// 打印StreamingAsset体积
        /// </summary>
        /// <param name="extension"></param>
        private void Print(string extension)
        {
            DirectoryInfo directory = new DirectoryInfo(Application.streamingAssetsPath);
            if (directory.Exists)
            {
                List<FileInfo> bundleFiles = new List<FileInfo>(directory.GetFiles("*" + extension, SearchOption.AllDirectories));
                bundleFiles.Sort(SortBytes);

                string output = "";
                long total = 0;
                foreach (FileInfo file in bundleFiles)
                {
                    long size = file.Length;
                    total += size;
                    output += Observer.ConvertBytes(size) + "\t" + file.Name + "\n";
                }

                global::Warehouser.Log("StreamingAssets Files: " + bundleFiles.Count + " Size: " + Observer.ConvertBytes(total) + "\n" + output);
             //   Debug.Log(Observer.ConvertBytes(total) + "\tTotal");
            }
        }

        /// <summary>
        /// 根据文件大小排序
        /// </summary>
        /// <returns></returns>
        private int SortBytes(FileInfo a, FileInfo b)
        {
            if (a.Length > b.Length)
                return -1;
            else if (a.Length < b.Length)
                return 1;
            else
                return 0;
        }


        /// <summary>
        /// 加载Setting
        /// </summary>
        private void LoadSettings()
        {
            settings = AssetDatabase.LoadAssetAtPath<Setting>(SETTINGS_PATH);
            if (settings == null)
            {
                settings = new Setting();
                AssetDatabase.CreateAsset(settings, SETTINGS_PATH);
            }
        }

        /// <summary>
        /// 构建AssetBundles
        /// </summary>
        private void BuildAssetBundles()
        {
            BuildTarget platform = BuildTarget.iOS;
#if UNITY_ANDROID
            platform = BuildTarget.Android;
#elif UNITY_IPHONE
            platform = BuildTarget.iOS;
#elif UNITY_STANDALONE_WIN
            platform = BuildTarget.StandaloneWindows;
#endif
            if (!Directory.Exists(Application.streamingAssetsPath))
                Directory.CreateDirectory(Application.streamingAssetsPath);

            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.None, platform);
            AssetDatabase.Refresh();

            Debug.Log("AssetBunelds Build Complete !");
        }

    }
}



