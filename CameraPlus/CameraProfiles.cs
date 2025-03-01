﻿using IPA.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using LogLevel = IPA.Logging.Logger.Level;

namespace CameraPlus
{
    public static class CameraProfiles
    {
        public static string pPath = Path.Combine(UnityGame.UserDataPath, "." + Plugin.Name.ToLower());
        public static string mPath = Path.Combine(UnityGame.UserDataPath, Plugin.Name);
        public static string currentlySelected = "None";

        public static void CreateMainDirectory()
        {
            DirectoryInfo di = Directory.CreateDirectory(pPath);
            di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            Directory.CreateDirectory(Path.Combine(pPath, "Profiles"));
            var a = new DirectoryInfo(Path.Combine(pPath, "Profiles")).GetDirectories();
            if (a.Length > 0)
                currentlySelected = a.First().Name;
        }

        public static void SaveCurrent()
        {
            string cPath = mPath;

            if (!Plugin.Instance._rootConfig.ProfileLoadCopyMethod && Plugin.Instance._currentProfile != null)
            {
                cPath = Path.Combine(pPath, "Profiles", Plugin.Instance._currentProfile);
            }
            DirectoryCopy(cPath, Path.Combine(pPath, "Profiles", GetNextProfileName()), false);
        }

        public static void SetNext(string now = null)
        {
            DirectoryInfo[] dis = new DirectoryInfo(Path.Combine(pPath, "Profiles")).GetDirectories();
            if (now == null)
            {
                currentlySelected = "None";
                if (dis.Length > 0)
                    currentlySelected = dis.First().Name;
                return;
            }
            int index = 0;
            var a = dis.Where(x => x.Name == now);
            if (a.Count() > 0)
            {
                index = dis.ToList().IndexOf(a.First());
                if (index < dis.Count() - 1)
                    currentlySelected = dis.ElementAtOrDefault(index + 1).Name;
                else
                    currentlySelected = dis.ElementAtOrDefault(0).Name;
            }
            else
            {
                currentlySelected = "None";
                if (dis.Length > 0)
                    currentlySelected = dis.First().Name;
            }
        }

        public static void TrySetLast(string now = null)
        {
            DirectoryInfo[] dis = new DirectoryInfo(Path.Combine(pPath, "Profiles")).GetDirectories();
            if (now == null)
            {
                currentlySelected = "None";
                if (dis.Length > 0)
                    currentlySelected = dis.First().Name;
                return;
            }
            int index = 0;
            var a = dis.Where(x => x.Name == now);
            if (a.Count() > 0)
            {
                index = dis.ToList().IndexOf(a.First());
                if (index == 0 && dis.Length >= 2)
                    currentlySelected = dis.ElementAtOrDefault(dis.Count() - 1).Name;
                else if (index < dis.Count() && dis.Length >= 2)
                    currentlySelected = dis.ElementAtOrDefault(index - 1).Name;
                else
                    currentlySelected = dis.ElementAtOrDefault(0).Name;
            }
            else
            {
                currentlySelected = "None";
                if (dis.Length > 0)
                    currentlySelected = dis.First().Name;
            }
        }

        public static void DeleteProfile(string name)
        {
            if (Directory.Exists(Path.Combine(pPath, "Profiles", name)))
                Directory.Delete(Path.Combine(pPath, "Profiles", name), true);
        }

        public static string GetNextProfileName(string BaseName = "")
        {
            int index = 1;
            string folName = "CameraPlusProfile";
            string bname;
            if (BaseName == "")
                bname = "CameraPlusProfile";
            else
                bname = BaseName;
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(pPath, "Profiles"));
            DirectoryInfo[] dirs = dir.GetDirectories($"{bname}*");
            foreach (var dire in dirs)
            {
                folName = $"{bname}{index.ToString()}";
                index++;
            }
            return folName;
        }

        public static void SetProfile(string name)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(pPath, "Profiles", name));
            if (!dir.Exists)
                return;
            DirectoryInfo di = new DirectoryInfo(mPath);
            foreach (FileInfo file in di.GetFiles())
                file.Delete();
            //foreach (DirectoryInfo dim in di.GetDirectories())
            //    dim.Delete(true);

            DirectoryCopy(dir.FullName, mPath, false);
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
                return;

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        public static void DirectoryCreate(string sourceDirName)
        {
            if (!Directory.Exists(sourceDirName))
                Directory.CreateDirectory(sourceDirName);
        }
    }
    public class ProfileChanger : MonoBehaviour
    {
        public static string pPath = Path.Combine(UnityGame.UserDataPath, "." + Plugin.Name.ToLower());
        public void ProfileChange(String ProfileName)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(pPath, "Profiles", ProfileName));
            if (!dir.Exists)
                return;
            ClearCameras();
            Plugin.Instance._currentProfile = ProfileName;

            if (Plugin.Instance._rootConfig.ProfileLoadCopyMethod && ProfileName !=null)
                CameraProfiles.SetProfile(ProfileName);
            CameraUtilities.ReloadCameras();
        }

        public void ClearCameras()
        {
            var cs = Resources.FindObjectsOfTypeAll<CameraPlusBehaviour>();

            if (Plugin.Instance._rootConfig.ProfileLoadCopyMethod)
            {
                foreach (var c in cs)
                    CameraUtilities.RemoveCamera(c);
            }
            foreach (var csi in Plugin.Instance.Cameras.Values)
                Destroy(csi.Instance.gameObject);
            Plugin.Instance.Cameras.Clear();
        }
    }
}
