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

            //Packager
            GUILayout.Label("Packager", EditorStyles.boldLabel);

            //显示Package
            SerializedProperty packages = so.FindProperty("setting.packages");
            EditorGUILayout.PropertyField(packages, true);

            if (GUILayout.Button("Pack"))
            {
                Packager.Pack(setting.packages);
            }

            EditorGUILayout.Space();

            //Mapper
            GUILayout.Label("Mapper", EditorStyles.boldLabel);

            //显示MapPaths
            SerializedProperty mapPathsProp = so.FindProperty("setting.mapPaths");
            EditorGUILayout.PropertyField(mapPathsProp, true);
            
            if (GUILayout.Button("Map"))
            {
                MapperEditor.Map(setting.mapPaths.ToArray(), Constants.PATH_PAIRS_PATH);
            }

            EditorGUILayout.Space();

            GUILayout.Label("Asset Bundle", EditorStyles.boldLabel);

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

    }
}



