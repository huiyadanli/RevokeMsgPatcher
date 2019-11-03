using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;

namespace RevokeMsgPatcher.Modifier
{
    class QQLiteModifier : AppModifier
    {
        public QQLiteModifier(App config)
        {
            this.config = config;
        }

        /// <summary>
        /// 自动寻找获取微信安装路径
        /// </summary>
        /// <returns></returns>
        public override string FindInstallPath()
        {
            string installPath = PathUtil.FindInstallPathFromRegistry("QQLite");
            if (!IsAllFilesExist(installPath))
            {
                foreach (string defaultPath in PathUtil.GetDefaultInstallPaths(@"Tencent\QQLite"))
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
                    if (editor.FileName == "IM.dll")
                    {
                        return editor.FileVersion;
                    }
                }
            }
            return "";
        }
    }
}
