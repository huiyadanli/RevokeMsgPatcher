using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;

namespace RevokeMsgPatcher.Modifier
{
    class DingTalkModifier : AppModifier
    {
        public DingTalkModifier(App config)
        {
            this.config = config;
        }

        /// <summary>
        /// 钉钉
        /// x64: HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\钉钉
        /// x86: HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\钉钉
        /// </summary>
        /// <returns></returns>
        public override string FindInstallPath()
        {
            string installPath = FindDingTalkInstallPathFromRegistry();
            if (!IsAllFilesExist(installPath))
            {
                foreach (string defaultPath in PathUtil.GetDefaultInstallPaths(@"DingDing"))
                {
                    if (IsAllFilesExist(defaultPath))
                    {
                        return defaultPath;
                    }
                }
            }
            else
            {
                return installPath;
            }
            return null;
        }

        private string FindDingTalkInstallPathFromRegistry()
        {
            List<string> registryPaths = new List<string>();
            // 获取系统位数
            if (Environment.Is64BitProcess)
            {
                registryPaths.Add(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\钉钉");
            }
            else
            {
                registryPaths.Add(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\钉钉");
            }
            try
            {
                object uninstallExe = null;
                foreach (string registryPath in registryPaths)
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\钉钉");
                    if (key == null)
                    {
                        continue;
                    }
                    uninstallExe = key.GetValue("UninstallString");
                    key.Close();
                    if (uninstallExe != null && !string.IsNullOrEmpty(uninstallExe.ToString()))
                    {
                        break;
                    }
                }
                // 再判断一次
                if (uninstallExe != null && !string.IsNullOrEmpty(uninstallExe.ToString()))
                {
                    return uninstallExe.ToString().Replace("uninst.exe", "");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        public override string GetVersion()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// a.初始化修改器
        /// 使用主程序的版本
        /// </summary>
        /// <param name="installPath">APP安装路径</param>
        public new void InitEditors(string installPath)
        {
            string mainVersion = GetVersion();
            // 初始化文件修改器
            editors = new List<FileHexEditor>();
            foreach (TargetInfo info in config.FileTargetInfos.Values)
            {
                FileHexEditor editor = new FileHexEditor(installPath, info, mainVersion);
                editors.Add(editor);
            }

        }
    }
}
