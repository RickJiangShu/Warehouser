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
        public string tag;//tag 通常是path，但如果是sprite则是atlasName
        public byte tagType;

        public Pair(string name, string tag, byte tagType)
        {
            this.name = name;
            this.tag = tag;
            this.tagType = tagType;
        }
    }

    public class PairTagType
    {
        public const byte RESOURCES_PATH = 0;
        public const byte ASSETBUNDLE_NAME = 1;
        public const byte ATLAS_NAME = 2;
    }
}
