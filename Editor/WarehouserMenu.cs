/*
 * Author:  Rick
 * Create:  2017/8/19 10:52:57
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
namespace Plugins.Warehouser
{
    using UnityEngine;
    using UnityEditor;

    /// <summary>
    /// WarehouserMenu
    /// </summary>
    public class WarehouserMenu : ScriptableObject
    {
        [MenuItem("Assets/Warehouser/Add To MapPaths")]
        static void AddToMapPaths()
        {
            string path = GetSelectedPath();
            WarehouserWindow.Get().setting.mapPaths.Add(path);
        }

        private static string GetSelectedPath()
        {
            string path = "Assets";
            foreach (Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
            }
            return path;
        }
    }

}
