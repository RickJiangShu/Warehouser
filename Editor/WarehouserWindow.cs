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
        public void AddItemsToMenu(GenericMenu menu)
	    {
            GUIContent reloadMenu = new GUIContent("Reload");
            GUIContent saveMenu = new GUIContent("Save");
            menu.AddItem(reloadMenu, false, LoadSettings);
		    menu.AddItem(saveMenu, false, SaveSettings);
        }

        void Awake()
        {
            LoadSettings();  
        }
        void OnDestroy()
        {
            SaveSettings();
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
            settings.batchExtension = EditorGUILayout.TextField("Batch Extension:", settings.batchExtension);

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

            if(GUILayout.Button("Clear"))
            {
                AssetBundlePackager.Clear(settings.assetBundlePackages);
            }

            if (GUILayout.Button("Pack"))
            {
                AssetBundlePackager.Pack(settings.assetBundlePackages, settings.batchExtension);
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
        /// 加载Setting
        /// </summary>
        private void LoadSettings()
        {
            settings = AssetDatabase.LoadAssetAtPath<Setting>(SETTINGS_PATH);
            if (settings == null)
            {
                settings = new Setting();
                SaveSettings();
            }
        }

        /// <summary>
        /// 保存Setting
        /// </summary>
        private void SaveSettings()
        {
            UnityEngine.Object old = AssetDatabase.LoadMainAssetAtPath(SETTINGS_PATH);
            if (old != null)
            {
                EditorUtility.CopySerialized(settings, old);
            }
            else
            {
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



