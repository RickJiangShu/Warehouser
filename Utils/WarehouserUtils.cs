﻿/*
 * Author:  Rick
 * Create:  2017/8/4 11:12:00
 * Email:   rickjiangshu@gmail.com
 * Follow:  https://github.com/RickJiangShu
 */
using System.IO;

namespace Plugins.Warehouser
{
    /// <summary>
    /// WarehouserUtils
    /// </summary>
    public class WarehouserUtils
    {
        /// <summary>
        /// 是否在Resources目录下
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsResource(string path)
        {
            return path.Contains("Resources");
        }

        public static bool IsDirectory(string path)
        {
            FileAttributes attr = File.GetAttributes(path);
            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }


        #region UNITY_EDITOR_WIN
        /// <summary>
        /// 转换分割符
        /// </summary>
        /// <returns></returns>
        public static string ConverSeparator(string path)
        {
            return path.Replace('\\', '/');
        }
        #endregion

        /// <summary>
        /// 转换Unix路径
        /// </summary>
        /// <param name="path">完整路径</param>
        /// <param name="directoryName">目录名</param>
        /// <param name="withDirectory">包含目录</param>
        /// <param name="withExtension">包含拓展名</param>
        /// <returns></returns>
        public static string ConvertUnixPath(string path,string directoryName,bool withDirectory,bool withExtension)
        {
            path = path.Replace('\\', '/');

            string directory = directoryName + '/'; 
            int start = path.IndexOf(directory);
            if (withDirectory)
            {
                path = path.Substring(start);
            }
            else
            {
                path = path.Substring(start + directory.Length);
            }
            if (!withExtension)
            {
                path = Path.ChangeExtension(path, null);
            }
            return path;
        }
    }
}

