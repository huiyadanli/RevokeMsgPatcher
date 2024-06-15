using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RevokeMsgPatcher.Modifier
{
    class QQNTModifier : AppModifier
    {
        public QQNTModifier(App config)
        {
            this.config = config;
        }

        /// <summary>
        /// 自动寻找获取微信安装路径
        /// </summary>
        /// <returns></returns>
        public override string FindInstallPath()
        {
            try
            {
                string installPath = PathUtil.FindInstallPathFromRegistryWOW6432Node("QQ");
                if (!string.IsNullOrEmpty(installPath))
                {
                    installPath = Path.GetDirectoryName(installPath);
                    if (IsAllFilesExist(installPath))
                    {
                        return installPath;
                    }
                }

                installPath = PathUtil.FindInstallPathFromRegistry("QQNT");
                if (!IsAllFilesExist(installPath))
                {
                    List<string> defaultPathList = PathUtil.GetDefaultInstallPaths(@"Tencent\QQNT");
                    foreach (string defaultPath in defaultPathList)
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        /// <summary>
        /// 获取整个APP的当前版本
        /// </summary>
        /// <returns></returns>
        public override string GetVersion()
        {
            if (editors != null && editors.Count > 0)
            {
                foreach (FileHexEditor editor in editors)
                {
                    if (editor.FileName == "QQ.exe")
                    {
                        return editor.FileVersion;
                    }
                }
            }

            return "";
        }

        public string GetIndexJsPath()
        {
            if (string.IsNullOrEmpty(InstallPath))
            {
                throw new Exception("未获取到QQNT安装路径或者QQNT安装路径不合法");
            }

            string indexPath = Path.Combine(InstallPath, @"resources\app\app_launcher\index.js");
            if (!File.Exists(indexPath))
            {
                throw new Exception("未找到index.js文件");
            }

            return indexPath;
        }

        public string GetLiteLoaderPath()
        {
            return Path.Combine(Application.StartupPath, @"LiteLoaderQQNT");
        }

        public override void AfterPatchSuccess()
        {
            string indexPath = GetIndexJsPath();
            string content = File.ReadAllText(indexPath);
            // 正则 require\(String.raw`.*`\);
            string pattern = @"require\(String.raw`.*`\);";
            string liteLoaderPath = GetLiteLoaderPath();
            if (!Directory.Exists(liteLoaderPath))
            {
                MessageBox.Show("LiteLoaderQQNT文件夹不存在，仅安装QQNT去验证补丁", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string replacement = $"require(String.raw`{liteLoaderPath}`);";
            if (Regex.IsMatch(content, pattern))
            {
                content = Regex.Replace(content, pattern, replacement);
            }
            else
            {
                content = replacement + "\n" + content;
            }

            File.WriteAllText(indexPath, content);
        }

        public override void AfterPatchFail()
        {
            try
            {
                string indexPath = GetIndexJsPath();
                string content = File.ReadAllText(indexPath);
                string pattern = @"require\(String.raw`.*`\);\n";
                if (Regex.IsMatch(content, pattern))
                {
                    content = Regex.Replace(content, pattern, "");
                    File.WriteAllText(indexPath, content);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public new bool Restore()
        {
            AfterPatchFail();
            return base.Restore();
        }
    }
}