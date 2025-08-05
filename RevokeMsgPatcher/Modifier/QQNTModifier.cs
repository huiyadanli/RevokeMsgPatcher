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
    class QQNTModifier : BaseAppModifier
    {
        public override string VersionFileName => "wrapper.node";

        public QQNTModifier(App config) : base(config) { }

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

        public new bool Restore()
        {
            base.AfterPatchFail();
            return base.Restore();
        }
    }
}