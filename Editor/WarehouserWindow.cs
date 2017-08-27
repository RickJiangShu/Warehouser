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
        public void OnEnable()
        {
            
            LoadSetting();  
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

            if(GUILayout.Button("Clear Unused Packages"))
            {
                AssetBundlePackager.Clear(setting.assetBundlePackages);
            }

            if (GUILayout.Button("Pack"))
            {
                AssetBundlePackager.Pack(setting.assetBundlePackages);
            }

            if (GUILayout.Button("Map"))
            {
                MapperEditor.Map(setting.mapPaths.ToArray(), Constants.PATH_PAIRS_PATH);
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
                SaveSetting();
            }
        }


        /// <summary>
        /// 加载Setting
        /// </summary>
        private void LoadSetting()
        {
            if (File.Exists(Constants.SETTING_PATH))
            {
                string content = File.ReadAllText(Constants.SETTING_PATH);
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
            if (!File.Exists(Constants.SETTING_PATH))
            {
                fileStream = File.Create(Constants.SETTING_PATH);
                fileStream.Close();
            }
            File.WriteAllText(Constants.SETTING_PATH, json);
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
        }

    }
}



