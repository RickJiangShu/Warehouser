/*
 * Author:  Rick
 * Create:  2017/8/2 16:49:24
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
namespace Plugins.Warehouser
{
    /// <summary>
    /// 名字映射
    /// </summary>
    [System.Serializable]
    public class Pair
    {
        public string name;
        public string path;//带后缀的为AssetBundle，不带后缀的为Sprite Atlas名

        public Pair(string name, string path)
        {
            this.name = name;
            this.path = path;
        }
    }
}
