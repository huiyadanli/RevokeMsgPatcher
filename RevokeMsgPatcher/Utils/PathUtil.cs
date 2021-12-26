using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Utils
{
    public class PathUtil
    {
        public static void DisplayAllProgram()
        {
            RegistryKey uninstallKey, programKey;
            uninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            string[] programKeys = uninstallKey.GetSubKeyNames();
            foreach (string keyName in programKeys)
            {
                programKey = uninstallKey.OpenSubKey(keyName);
                Console.WriteLine(keyName + " , " + programKey.GetValue("DisplayName") + " , " + programKey.GetValue("InstallLocation"));
                programKey.Close();
            }
            uninstallKey.Close();
        }

        /// <summary>
        /// 从注册表中寻找安装路径
        /// </summary>
        /// <param name="uninstallKeyName">
        /// 安装信息的注册表键名
        /// 微信：WeChat
        /// QQ：{052CFB79-9D62-42E3-8A15-DE66C2C97C3E} 
        /// TIM：TIM
        /// </param>
        /// <returns>安装路径</returns>
        public static string FindInstallPathFromRegistry(string uninstallKeyName)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{uninstallKeyName}");
                if (key == null)
                {
                    return null;
                }
                object installLocation = key.GetValue("InstallLocation");
                key.Close();
                if (installLocation != null && !string.IsNullOrEmpty(installLocation.ToString()))
                {
                    return installLocation.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        /// <summary>
        /// 获取所有可能的默认安装路径
        /// </summary>
        /// <param name="relativePath">Tencent\*</param>
        /// <returns></returns>
        public static List<string> GetDefaultInstallPaths(string relativePath)
        {
            List<string> list = new List<string>();
            // 从默认安装目录查找
            string[] drives = Environment.GetLogicalDrives(); //获取当前计算机逻辑磁盘名称列表
            foreach (string d in drives)
            {
                string path = Path.Combine(d, $@"Program Files (x86)\{relativePath}");
                if (Directory.Exists(path))
                {
                    list.Add(path);
                }
            }
            return list;
        }

        /// <summary>
        /// 按文件夹修改时间倒序
        /// </summary>
        /// <param name="dirs"></param>
        public static void SortByLastWriteTimeDesc(ref DirectoryInfo[] dirs)
        {
            Array.Sort(dirs, delegate (DirectoryInfo x, DirectoryInfo y) { return y.LastWriteTime.CompareTo(x.LastWriteTime); });
        }

    }
}
