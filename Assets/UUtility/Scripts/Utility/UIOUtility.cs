using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UTool.Utility
{
    public static partial class UUtility
    {
        /// <summary>
        /// Creates File and Directory If it does not exist on the Path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Returns true if a New File was Created</returns>
        public static bool CheckAndCreateFile(string path)
        {
            bool fileCreated = false;

            string filename = path.Split('\\').Last();
            string folder = path.Replace($@"\{filename}", "");
            CheckAndCreateDirectory(folder);

            path = PlatformSpecificPath(path);

            if (!File.Exists(path))
            {
                File.Create(path).Close();
                fileCreated = true;
            }

            return fileCreated;
        }

        /// <summary>
        /// Creates Directory If it does not exist on the Path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Returns true if a New Directory was Created</returns>
        public static bool CheckAndCreateDirectory(string path)
        {
            bool DirCreated = false;

            path = PlatformSpecificPath(path);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                DirCreated = true;
            }

            return DirCreated;
        }

        public static string GetFileExtension(string fileName)
        {
            string[] paths = fileName.Split('.');

            if (paths.Count() == 1)
                return "";

            return $".{paths.Last()}";
        }

        public static string GetVaildFilename(string folder, string filename)
        {
            string[] split = filename.Split('.');

            bool hasExtention = split.Length > 1;

            string extention = hasExtention ? split.Last() : "";
            string orginalName = hasExtention ? filename.Replace($".{extention}", "") : filename;
            string name = orginalName;

            string GetPath() => hasExtention ? $@"{folder}\{name}.{extention}" : $@"{folder}\{name}";
            string GetFilename() => hasExtention ? $@"{name}.{extention}" : $@"{name}";

            int count = 0;
            while (File.Exists(GetPath()))
            {
                count++;
                name = $"{orginalName}({count})";
            }

            return GetFilename();
        }

        public static bool IsFileAccessible(string path)
        {
            try
            {
                File.Open(path, FileMode.Open, FileAccess.ReadWrite).Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void WriteAllText(string path, string text)
        {
            File.WriteAllText(PlatformSpecificPath(path), text);
        }

        public static string ReadAllText(string path)
        {
            return File.ReadAllText(PlatformSpecificPath(path));
        }

        public static void DeleteDirectory(string path, bool recursive)
        {
            Directory.Delete(PlatformSpecificPath(path), recursive);
        }

        public static void DeleteFile(string path)
        {
            File.Delete(PlatformSpecificPath(path));
        }

        public static string[] GetDirectoryFiles(string path)
        {
            return Directory.GetFiles(PlatformSpecificPath(path));
        }

        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(PlatformSpecificPath(path));
        }

        public static bool FileExists(string path)
        {
            return File.Exists(PlatformSpecificPath(path));
        }

        public static DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(PlatformSpecificPath(path));
        }

        public static FileStream CreateFile(string path)
        {
            return File.Create(PlatformSpecificPath(path));
        }

        public static string PlatformSpecificPath(string path)
        {
#if PLATFORM_ANDROID && !UNITY_EDITOR
            return path.Replace(@"\", "/");
#else
            return path;
#endif
        }
    }
}