using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Model
{
    public class App
    {
        public string Name { get; set; }

        /// <summary>
        /// 主程序相对路径, 非必填, 用于主程序版本号的使用
        /// </summary>
        public string MainAppRelativePath { get; set; }

        public Dictionary<string, TargetInfo> FileTargetInfos { get; set; }

        public Dictionary<string, List<ModifyInfo>> FileModifyInfos { get; set; }

        /// <summary>
        /// 通用的修改特征
        /// </summary>
        public Dictionary<string, List<CommonModifyInfo>> FileCommonModifyInfos { get; set; }

        public HashSet<string> GetSupportVersions()
        {
            // 使用 HashSet 防重
            HashSet<string> versions = new HashSet<string>();
            foreach (List<ModifyInfo> modifyInfos in FileModifyInfos.Values)
            {
                foreach (ModifyInfo modifyInfo in modifyInfos)
                {
                    versions.Add(modifyInfo.Version);
                }
            }
            return versions;
        }

        public string GetSupportVersionStr()
        {
            string str = "";
            foreach (string v in GetSupportVersions())
            {
                str += v + "、";
            }
            if (str.Length > 1)
            {
                str = str.Substring(0, str.Length - 1);
            }
            return str;
        }
    }
}
