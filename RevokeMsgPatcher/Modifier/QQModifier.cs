using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;
using System;
using System.Collections.Generic;

namespace RevokeMsgPatcher.Modifier
{
    class QQModifier : BaseAppModifier
    {
        public override string VersionFileName => "IM.dll";

        public QQModifier(App config) : base(config) { }

        /// <summary>
        /// 自动寻找获取微信安装路径
        /// </summary>
        /// <returns></returns>
        public override string FindInstallPath()
        {
            try
            {
                string installPath = PathUtil.FindInstallPathFromRegistry("{052CFB79-9D62-42E3-8A15-DE66C2C97C3E}");
                if (!IsAllFilesExist(installPath))
                {
                    List<string> defaultPathList = PathUtil.GetDefaultInstallPaths(@"Tencent\QQ");
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
    }
}
