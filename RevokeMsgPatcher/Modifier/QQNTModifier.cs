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
                    if (editor.FileName == "wrapper.node")
                    {
                        return editor.FileVersion;
                    }
                }
            }
            return "";
        }

        public override void AfterPatchSuccess()
        {

        }

        public override void AfterPatchFail()
        {

        }

        public new bool Restore()
        {
            AfterPatchFail();
            return base.Restore();
        }
    }
}