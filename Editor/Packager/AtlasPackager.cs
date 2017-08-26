/*
 * Author:  Rick
 * Create:  2017/8/26 10:33:11
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
namespace Plugins.Warehouser.Editor
{
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    /// <summary>
    /// AtlasPackager
    /// </summary>
    public class AtlasPackager : ScriptableObject
    {
        /// <summary>
        /// 图片拓展名
        /// </summary>
        private static readonly List<string> SPRITE_EXTENSIONS = new List<string>() { ".jpg", ".png" };

        /// <summary>
        /// 打包
        /// </summary>
        /// <param name="packages"></param>
        public static void Pack(List<AtlasPackage> packages)
        {
            foreach(AtlasPackage package in packages)
            {
                foreach(string path in package.paths)
                {
                    if(WarehouserUtils.IsDirectory(path))
                    {
                        PackDirectory(path, package.tag);
                    }
                    else
                    {
                        PackFile(path, package.tag);
                    }
                }
            }
        }

        /// <summary>
        /// 清理
        /// </summary>
        /// <param name="packages"></param>
        public static void Clear(List<AtlasPackage> packages)
        {
            DirectoryInfo assetDirectory = new DirectoryInfo("Assets");
            FileInfo[] allFiles = assetDirectory.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (FileInfo file in allFiles)
            {
                if (!SPRITE_EXTENSIONS.Contains(file.Extension))
                    continue;

                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(WarehouserUtils.FullName2AssetPath(file.FullName));
                if (importer.textureType == TextureImporterType.Sprite && !string.IsNullOrEmpty(importer.spritePackingTag))
                {
                    bool inPackage = false;
                    foreach (AtlasPackage package in packages)
                    {
                        if (importer.spritePackingTag == package.tag)
                        {
                            inPackage = true;
                            break;
                        }
                    }
                    if (!inPackage)
                    {
                        importer.spritePackingTag = string.Empty;
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        /// <summary>
        /// 打包整个文件夹
        /// </summary>
        private static void PackDirectory(string path, string tag)
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                FileInfo[] allFiles = directory.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (FileInfo file in allFiles)
                {
                    if (!SPRITE_EXTENSIONS.Contains(file.Extension))
                        continue;

                    TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(WarehouserUtils.FullName2AssetPath(file.FullName));
                    SetTag(importer, tag, true);
                }
            }
        }

        /// <summary>
        /// 打包单个文件
        /// </summary>
        private static void PackFile(string path, string tag)
        {
            if (File.Exists(path))
            {
                FileInfo file = new FileInfo(path);
                if (!SPRITE_EXTENSIONS.Contains(file.Extension))
                    return;

                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(WarehouserUtils.FullName2AssetPath(file.FullName));
                SetTag(importer, tag, true);
            }
        }

        /// <summary>
        /// 设置tag
        /// </summary>
        private static void SetTag(TextureImporter importer, string tag, bool forceSetSprite)
        {
            if (forceSetSprite)
            {
                importer.textureType = TextureImporterType.Sprite;
            }

            if (importer.textureType == TextureImporterType.Sprite && importer.spritePackingTag != tag)
            {
                importer.spritePackingTag = tag;
                importer.SaveAndReimport();
            }
        }
    }

}
