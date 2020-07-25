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
            // 精准
            if (FileModifyInfos != null)
            {
                foreach (List<ModifyInfo> modifyInfos in FileModifyInfos.Values)
                {
                    foreach (ModifyInfo modifyInfo in modifyInfos)
                    {
                        versions.Add(modifyInfo.Version);
                    }
                }
            }
            // 模糊 范围
            if (FileCommonModifyInfos != null)
            {
                foreach (List<CommonModifyInfo> commonModifyInfos in FileCommonModifyInfos.Values)
                {
                    foreach (CommonModifyInfo commonModifyInfo in commonModifyInfos)
                    {
                        string end = string.IsNullOrEmpty(commonModifyInfo.EndVersion) ? "最新版" : commonModifyInfo.EndVersion;
                        versions.Add(commonModifyInfo.StartVersion + "~" + end);
                    }
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
