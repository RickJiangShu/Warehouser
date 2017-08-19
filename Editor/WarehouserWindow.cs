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

namespace Plugins.Warehouser
{
    /// <summary>
    /// 映射器编辑器
    /// </summary>
    public class WarehouserWindow : EditorWindow
    {
        /// <summary>
        /// 配置文件
        /// </summary>
        private static WarehouserSetting setting;

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
            //Base Settings
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);

            setting.pathPairsDirectory = EditorGUILayout.TextField("PathPairs Directory", setting.pathPairsDirectory);
            //
            //Opertions
            GUILayout.Label("Opertions", EditorStyles.boldLabel);

            if (GUILayout.Button("Map Paths"))
            {
                MapperEditor.MapPaths(setting.pathPairsPath);
            }

            if (GUI.changed)
            {
                SaveSetting();
            }
        }


        /// <summary>
        /// 加载Setting
        /// </summary>
        private static void LoadSetting()
        {
            if (File.Exists(WarehouserSetting.PATH))
            {
                string content = File.ReadAllText(WarehouserSetting.PATH);
                setting = JsonUtility.FromJson<WarehouserSetting>(content);
            }
            else
            {
                setting = new WarehouserSetting();
                SaveSetting();
            }
        }

        /// <summary>
        /// 保存Setting
        /// </summary>
        private static void SaveSetting()
        {
            string json = JsonUtility.ToJson(setting, true);
            FileStream fileStream;
            if (!File.Exists(WarehouserSetting.PATH))
            {
                fileStream = File.Create(WarehouserSetting.PATH);
                fileStream.Close();
            }
            File.WriteAllText(WarehouserSetting.PATH, json);
            AssetDatabase.Refresh();
        }

    }
}



