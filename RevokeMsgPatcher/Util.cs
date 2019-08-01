using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RevokeMsgPatcher
{
    public class Util
    {

        /// <summary>
        /// 自动查找安装路径
        /// </summary>
        /// <returns>安装路径</returns>
        public static string AutoFindInstallPath()
        {
            // 微信的注册表路径
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\WeChat");
                object installLocation = key.GetValue("InstallLocation");
                key.Close();
                if (installLocation == null || string.IsNullOrEmpty(installLocation.ToString()) || !IsWechatInstallPath(installLocation.ToString()))
                {
                    // 从默认安装目录查找
                    string[] drives = Environment.GetLogicalDrives(); //获取当前计算机逻辑磁盘名称列表
                    foreach (string d in drives)
                    {
                        string assertPath = Path.Combine(d, @"Program Files (x86)\Tencent\WeChat");
                        if (IsWechatInstallPath(assertPath))
                        {
                            return assertPath;
                        }
                    }
                }
                else
                {
                    return installLocation.ToString();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        /// <summary>
        /// 通过文件是否存在判断是否是安装目录
        /// </summary>
        /// <param name="path">安装目录</param>
        /// <returns></returns>
        public static bool IsWechatInstallPath(string path)
        {
            return File.Exists(Path.Combine(path, "WeChatWin.dll")) && File.Exists(Path.Combine(path, "WeChat.exe"));
        }

        /// <summary>
        /// 获取文件版本
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileVersion(string path)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(path);
            return fileVersionInfo.FileVersion;
        }

        /// <summary>
        /// 计算文件SHA1
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ComputeFileSHA1(string s)
        {
            FileStream file = new FileStream(s, FileMode.Open);
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] retval = sha1.ComputeHash(file);
            file.Close();

            StringBuilder sc = new StringBuilder();
            for (int i = 0; i < retval.Length; i++)
            {
                sc.Append(retval[i].ToString("x2"));
            }
            return sc.ToString();
        }

        /// <summary>
        /// 修改文件指定位置的字节
        /// </summary>
        /// <param name="path">WeChatWin.dll 的路径</param>
        /// <param name="position">偏移位置</param>
        /// <param name="after">修改后的值</param>
        /// <returns></returns>
        public static bool EditHex(string path, long position, byte after)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                stream.Position = position;
                stream.WriteByte(after);
            }
            return true;

        }
    }
}
