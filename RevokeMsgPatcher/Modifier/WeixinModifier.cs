using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace RevokeMsgPatcher.Modifier
{
    class WeixinModifier : BaseAppModifier
    {
        public override string VersionFileName => "Weixin.dll";

        public WeixinModifier(App config) : base(config) { }

        /// <summary>
        /// 自动寻找获取微信安装路径
        /// </summary>
        /// <returns></returns>
        public override string FindInstallPath()
        {
            try
            {
                string installPath = PathUtil.FindInstallPathFromRegistryWOW6432Node("Weixin");
                string realPath = null;
                if (!string.IsNullOrEmpty(installPath))
                {
                    installPath = Path.GetDirectoryName(installPath);
                    realPath = GetRealInstallPath(installPath);
                }
                if (string.IsNullOrEmpty(realPath))
                {
                    List<string> defaultPathList = PathUtil.GetDefaultInstallPaths(@"Tencent\Weixin");
                    foreach (string defaultPath in defaultPathList)
                    {
                        realPath = GetRealInstallPath(defaultPath);
                        if (!string.IsNullOrEmpty(realPath))
                        {
                            return defaultPath;
                        }
                    }
                }
                else
                {
                    return realPath;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        /// <summary>
        /// 微信目录结构
        /// </summary>
        /// <param name="basePath"></param>
        /// <returns></returns>
        private string GetRealInstallPath(string basePath)
        {
            if (basePath == null)
            {
                return null;
            }
            DirectoryInfo[] directories = new DirectoryInfo(basePath).GetDirectories();
            PathUtil.SortByLastWriteTimeDesc(ref directories); // 按修改时间倒序
            foreach (DirectoryInfo folder in directories)
            {
                if (IsAllFilesExist(folder.FullName))
                {
                    return folder.FullName;
                }
            }
            return null;
        }
    }
}
