using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Model
{
    public class TargetInfo
    {
        public string Name { get; set; }

        public string RelativePath { get; set; }

        public string Memo { get; set; }

        public string StartVersion { get; set; }

        public string EndVersion { get; set; }

        /// <summary>
        /// 使用其他文件的版本作为版本号
        /// </summary>
        public string RelativePathForVersion { get; set; }

        public TargetInfo Clone()
        {
            TargetInfo o = new TargetInfo();
            o.Name = Name;
            o.RelativePath = RelativePath;
            o.Memo = Memo;
            o.StartVersion = StartVersion;
            o.EndVersion = EndVersion;
            o.RelativePathForVersion = RelativePathForVersion;
            return o;
        }

        public string GetAbsolutePath(string installPath)
        {
            if (string.IsNullOrWhiteSpace(installPath)) return "";
            if (RelativePath.Contains("{version}"))
            {
                // 获取{version}之前的路径部分
                var versionIndex = RelativePath.IndexOf("{version}");
                var prefixPath = RelativePath.Substring(0, versionIndex);
                var suffixPath = RelativePath.Substring(versionIndex + "{version}".Length);

                // Prefer versioned modules over files left behind by older QQ installations.
                var searchDirectory = Path.Combine(installPath, prefixPath);

                // 获取所有子目录
                var directories = Directory.Exists(searchDirectory) ? Directory.GetDirectories(searchDirectory) : new string[0];

                // 筛选包含至少三个点的目录名，并解析为版本号
                var validVersions = new List<Tuple<string, Version>>();

                foreach (var directory in directories)
                {
                    var dirName = Path.GetFileName(directory);

                    // 检查是否包含至少两个点
                    if (dirName.Count(c => c == '.') >= 2)
                    {
                        // 尝试解析为版本号
                        if (Version.TryParse(dirName.Replace("-","."), out Version version))
                        {
                            string candidate = Path.Combine(installPath, RelativePath.Replace("{version}", dirName));
                            if (File.Exists(candidate)) validVersions.Add(new Tuple<string, Version>(dirName, version));
                        }
                    }
                }

                if (validVersions.Count == 0)
                {
                    // Also accept a version directory or the directory containing the target.
                    foreach (string relative in new[] { suffixPath.TrimStart('\\', '/'), Name })
                    {
                        string directPath = Path.Combine(installPath, relative);
                        if (File.Exists(directPath)) return directPath;
                    }
                    return "";
                }

                // 找到版本号最大的目录
                var maxVersionDir = validVersions.OrderByDescending(v => v.Item2).First().Item1;

                // 构建完整路径
                var fullRelativePath = RelativePath.Replace("{version}", maxVersionDir);
                return Path.Combine(installPath, fullRelativePath);
            }
            else
            {
                return Path.Combine(installPath, RelativePath);
            }
        }
    }
}
