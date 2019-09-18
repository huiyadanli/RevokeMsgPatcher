using RevokeMsgPatcher.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Modifier
{
    class WechatModifier : AppModifier
    {
        public override string FindInstallPath()
        {
            string installPath = PathUtil.FindInstallPathFromRegistry("Wecaht");
            if(!IsAllBinaryFilesExist(installPath))
            {
                foreach(string defaultPath in PathUtil.GetDefaultInstallPaths(@"Tencent\Wechat"))
                {
                    if(IsAllBinaryFilesExist(defaultPath))
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

        public override bool GetVersion()
        {
            throw new NotImplementedException();
        }
    }
}
