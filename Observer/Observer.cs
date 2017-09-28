/*
 * Author:  Rick
 * Create:  2017/9/28 11:21:19
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
namespace Plugins.Warehouser.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Observer
    /// </summary>
    public class Observer : MonoBehaviour
    {
        //数量
        public static int instanceNumber = 0;
        public static int gameObjectNumber = 0;
        public static int recycleNumber = 0;//

        //函数调用计数
        public static int getInstanceCount = 0;
        public static int recycleCount = 0;
        public static int getAssetCount = 0;
        public static int destroyCount = 0;//销毁计数
        public static int unloadAssetCount = 0;

        private bool isShow = false;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnGUI()
        {
            if (GUILayout.Button("Observer"))
            {
                isShow = !isShow;
            }

            if (isShow)
            {
                GUILayout.TextField(
                "instance number: " + instanceNumber +
                "\ngame object number: " + gameObjectNumber +
                "\nrecycle number: " + recycleNumber +
                "\n\nget instance count: " + getInstanceCount +
                "\nget asset count: " + getAssetCount +
                "\nrecycle count: " + recycleCount +
                "\ndestroy count: " + destroyCount +
                "\nunload asset count: " + unloadAssetCount
                );
            }
        }
    }
}