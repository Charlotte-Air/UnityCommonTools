using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace Framework.Utils.Unity
{
    public class FileUtils
    {
        public static bool IsDirectoryExist(string path)
        {
            return Directory.Exists(path);
        }

        public static void CreateDirectory(string path)
        {
            try
            {
                if (!IsDirectoryExist(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void DeleteDirectory(string path, bool keepRoot = false)
        {
            DeleteDirectoryInternal(path);
            if (!keepRoot)
            {
                System.IO.Directory.Delete(path);
            }
        }

        private static void DeleteDirectoryInternal(string path)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);

                if (dir != null)
                {
                    foreach (FileInfo fi in dir.GetFiles())
                    {
                        fi.Delete();
                    }

                    foreach (DirectoryInfo di in dir.GetDirectories())
                    {
                        DeleteDirectoryInternal(di.FullName);
                        di.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static string RemovePathPrefix(string path, string prefix)
        {
            int f = path.IndexOf(prefix);
            if (f != -1)
            {
                path = path.Remove(f, prefix.Length);
            }

            return path;
        }

        public static string RemovePathFileName(string path)
        {
            string f = Path.GetFileName(path);
            return path.Replace(f, "");
        }

        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public static string GetExtension(string path)
        {
            int f = path.LastIndexOf(".");
            if (f != -1)
            {
                int l = path.Length - f;
                return path.Substring(f, l);
            }

            return "";
        }

        public static string RemoveExtension(string path)
        {
            int f = path.LastIndexOf(".");
            if (f != -1)
            {
                int l = path.Length - f;
                path = path.Remove(f, l);
            }

            return path;
        }

        public static string RemoveLastPathSep(string path)
        {
            string last = path.Substring(path.Length - 1);
            if (last == "/" || last == "\\")
            {
                path = path.Substring(0, path.Length - 1);
            }

            return path;
        }

        public static string GetLastDir(string path)
        {
            path = RemoveLastPathSep(path);
            int f = path.LastIndexOfAny(new char[] { '/', '\\' });
            if (f != -1)
            {
                int l = path.Length - f - 1;
                return path.Substring(f + 1, l);
            }

            return "";
        }

        public static void CreateFile(string src, byte[] bytes)
        {
            try
            {
                File.WriteAllBytes(src, bytes);
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat("CreateFile error , path = {0}.", src);
                Debug.LogException(ex);
            }
        }

        public static void CopyFile(string src, string dest, bool encr = false)
        {
            if (File.Exists(dest))
            {
                byte[] bytes = File.ReadAllBytes(src);
                if (encr)
                {
                    for (int i = 0; i < bytes.Length; i++) bytes[i] = (byte)(bytes[i] ^ (byte)i);
                }

                File.WriteAllBytes(dest, bytes);
            }
            else
            {
                string destDir = Path.GetDirectoryName(dest);
                CreateDirectory(destDir);
                try
                {
                    if (encr)
                    {
                        byte[] bytes = File.ReadAllBytes(src);
                        for (int i = 0; i < bytes.Length; i++) bytes[i] = (byte)(bytes[i] ^ (byte)i);
                        File.WriteAllBytes(dest, bytes);
                    }
                    else
                    {
                        File.Copy(src, dest);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public static void CopyFile2(string src, string dest, string fill = "")
        {
            if (File.Exists(dest))
            {
                Debug.LogWarningFormat("目的文件已经存在。{0}->{1}", src, dest);
            }

            byte[] bytes = File.ReadAllBytes(src);
            if (!string.IsNullOrEmpty(fill))
            {
                byte[] oldbytes = bytes;
                bytes = new byte[fill.Length + oldbytes.Length];
                for (int i = 0; i < fill.Length; i++) bytes[i] = (byte)fill[i];
                Array.Copy(oldbytes, 0, bytes, fill.Length, oldbytes.Length);
            }

            File.WriteAllBytes(dest, bytes);
        }

        public static void MoveFile(string src, string dest)
        {
            string destDir = Path.GetDirectoryName(dest);
            CreateDirectory(destDir);
            try
            {
                File.Move(src, dest);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static byte[] GetFileBytes(string src)
        {
            return File.ReadAllBytes(src);
        }

        public static void DeleteFile(string src)
        {
            File.Delete(src);
        }

        public static void DeleteFiles(string path, string ext)
        {
            if (!ext.Contains("."))
            {
                ext = "." + ext;
            }

            try
            {
                string[] files = Directory.GetFiles(path);
                for (int i = 0; i < files.Length; i++)
                {
                    string f = files[i];
                    if (f.Contains(ext))
                    {
                        DeleteFile(f);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static List<string> GetFilesInDirectory(string path, bool withSubDirs, string searchPattern = "")
        {
            List<string> ret = new List<string>();

            GetFilesInDirectoryRecursively(path, searchPattern, withSubDirs, ref ret);

            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = ret[i].Replace("\\", "/");
            }

            return ret;
        }

        private static void GetFilesInDirectoryRecursively(string path, string searchPattern, bool withSubDirs,
            ref List<string> ret)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + path);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            FileInfo[] files = null;

            if (string.IsNullOrEmpty(searchPattern))
            {
                files = dir.GetFiles();
            }
            else
            {
                files = dir.GetFiles(searchPattern);
            }

            foreach (FileInfo file in files)
            {
                if (!file.FullName.Contains(".meta"))
                {
                    ret.Add(file.FullName);
                }
            }

            if (withSubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    GetFilesInDirectoryRecursively(subdir.FullName, searchPattern, withSubDirs, ref ret);
                }
            }
        }

        public static void DirectoryCopy(string srcPath, string destPath, bool withSubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(srcPath);

            if (!dir.Exists)
            {
                Debug.LogWarningFormat("directory copy failed, invalid srcPath {0}", srcPath);
                return;
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            CreateDirectory(destPath);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destPath, file.Name);
                try
                {
                    file.CopyTo(temppath, true);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            if (withSubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destPath, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, withSubDirs);
                }
            }
        }

        public static long GetFileSize(string filePath)
        {
            try
            {
                FileInfo fi = new FileInfo(filePath);
                return fi.Length;
            }
            catch
            {
                Debug.LogWarningFormat("Get FileInfo {0} failed.", filePath);
                return 0;
            }
        }
    }
}