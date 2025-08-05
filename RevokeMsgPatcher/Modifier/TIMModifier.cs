using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;

namespace RevokeMsgPatcher.Modifier
{
    class TIMModifier : BaseAppModifier
    {
        public override string VersionFileName => "IM.dll";

        public TIMModifier(App config) : base(config) { }

        /// <summary>
        /// 自动寻找获取微信安装路径
        /// </summary>
        /// <returns></returns>
        public override string FindInstallPath()
        {
            string installPath = PathUtil.FindInstallPathFromRegistry("TIM");
            if (!IsAllFilesExist(installPath))
            {
                foreach (string defaultPath in PathUtil.GetDefaultInstallPaths(@"Tencent\TIM"))
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
    }
}
