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
    public class WarehouserWindow : EditorWindow, IHasCustomMenu
    {
        /// <summary>
        /// AssetBundle 后缀名
        /// </summary>
        public const string EXTENSION = ".ab";

        /// <summary>
        /// 配置文件路径
        /// </summary>
        public const string SETTING_PATH = "Assets/WarehouserSetting.json";

        /// <summary>
        /// Pairs路径
        /// </summary>
        public const string PAIRS_PATH = "Assets/Resources/WarehouserPairs.asset";

        /// <summary>
        /// 配置文件
        /// </summary>
        public Setting setting;

        /// <summary>
        /// 滚动条坐标
        /// </summary>
        private Vector2 scrollPos;

        [MenuItem("Window/Warehouser")]
        public static WarehouserWindow Get()
        {
            return EditorWindow.GetWindow<WarehouserWindow>("Warehouser");
        }
        public void AddItemsToMenu(GenericMenu menu)
	    {
            GUIContent saveMenu = new GUIContent("Save");
		    menu.AddItem(saveMenu, false, SaveSetting);
        }

        void Awake()
        {
            LoadSetting();  
        }
        void OnDestroy()
        {
            SaveSetting();
        }

        public void OnGUI()
        {
            SerializedObject so = new SerializedObject(this);

            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            //AB Packager
            GUILayout.Label("Asset Bundle Packager", EditorStyles.boldLabel);

            SerializedProperty abPackages = so.FindProperty("setting.assetBundlePackages");
            EditorGUILayout.PropertyField(abPackages, true);

            EditorGUILayout.Space();

            //Mapper
            GUILayout.Label("Mapper", EditorStyles.boldLabel);

            //显示MapPaths
            SerializedProperty mapPathsProp = so.FindProperty("setting.mapPaths");
            EditorGUILayout.PropertyField(mapPathsProp, true);
            
            EditorGUILayout.Space();

            GUILayout.Label("Operation", EditorStyles.boldLabel);

            if(GUILayout.Button("Clear"))
            {
                AssetBundlePackager.Clear(setting.assetBundlePackages);
            }

            if (GUILayout.Button("Pack"))
            {
                AssetBundlePackager.Pack(setting.assetBundlePackages);
            }

            if (GUILayout.Button("Map"))
            {
                MapperEditor.Map(setting.mapPaths.ToArray(), PAIRS_PATH);
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
        /// 加载Setting
        /// </summary>
        private void LoadSetting()
        {
            if (File.Exists(SETTING_PATH))
            {
                string content = File.ReadAllText(SETTING_PATH);
                setting = JsonUtility.FromJson<Setting>(content);
            }
            else
            {
                setting = new Setting();
                SaveSetting();
            }
        }

        /// <summary>
        /// 保存Setting
        /// </summary>
        private void SaveSetting()
        {
            string json = JsonUtility.ToJson(setting, true);
            FileStream fileStream;
            if (!File.Exists(SETTING_PATH))
            {
                fileStream = File.Create(SETTING_PATH);
                fileStream.Close();
            }
            File.WriteAllText(SETTING_PATH, json);
            AssetDatabase.Refresh();
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



