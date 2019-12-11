using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;

namespace RevokeMsgPatcher.Modifier
{
    class WechatModifier : AppModifier
    {

        public WechatModifier(App config)
        {
            this.config = config;
        }

        /// <summary>
        /// 自动寻找获取微信安装路径
        /// </summary>
        /// <returns></returns>
        public override string FindInstallPath()
        {
            string installPath = PathUtil.FindInstallPathFromRegistry("Wechat");
            if (!IsAllFilesExist(installPath))
            {
                foreach (string defaultPath in PathUtil.GetDefaultInstallPaths(@"Tencent\Wechat"))
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
