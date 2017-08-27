/*
 * Author:  Rick
 * Create:  2017/8/27 16:22:03
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using UnityEngine;

namespace Plugins.Warehouser
{
    /// <summary>
    /// 名字映射列表
    /// </summary>
    [System.Serializable]
    public class Pairs : ScriptableObject
    {
        public Pair[] pairs;

        public int Length
        {
            get { return pairs.Length; }
        }

        public Pair this[int i]
        {
            get { return pairs[i]; }
        }
    }
}