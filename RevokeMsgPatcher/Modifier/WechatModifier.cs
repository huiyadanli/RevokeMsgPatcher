using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace RevokeMsgPatcher.Modifier
{
    class WechatModifier : AppModifier
    {

        public WechatModifier(App config)
        {
            this.config = config;
        }

        public override void AfterPatchSuccess()
        {
        }

        public override void AfterPatchFail()
        {
        }

        /// <summary>
        /// 自动寻找获取微信安装路径
        /// </summary>
        /// <returns></returns>
        public override string FindInstallPath()
        {
            try
            {
                string installPath = PathUtil.FindInstallPathFromRegistry("Wechat");
                string realPath = GetRealInstallPath(installPath);
                if (string.IsNullOrEmpty(realPath))
                {
                    List<string> defaultPathList = PathUtil.GetDefaultInstallPaths(@"Tencent\QQ");
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
        /// 微信 3.5.0.4 改变了目录结构
        /// </summary>
        /// <param name="basePath"></param>
        /// <returns></returns>
        private string GetRealInstallPath(string basePath)
        {
            if (basePath == null)
            {
                return null;
            }
            if (IsAllFilesExist(basePath))
            {
                return basePath;
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
                    if (editor.FileName == "WeChatWin.dll")
                    {
                        return editor.FileVersion;
                    }
                }
            }
            return "";
        }

        //public override bool ValidateAndInitialize(string installPath)
        //{
        //    // 判断是否是安装路径
        //    if (!IsAllBinaryFilesExist(installPath))
        //    {
        //        return false;
        //    }

        //    // 初始化十六进制文件编辑器
        //    // 并寻找与之配对的版本修改信息
        //    InitEditors(installPath);

        //    return true;
        //}
    }
}
