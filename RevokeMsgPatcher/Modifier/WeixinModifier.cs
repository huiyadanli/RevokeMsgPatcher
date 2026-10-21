using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;
using System;
using System.IO;
using System.Linq;

namespace RevokeMsgPatcher.Modifier
{
    class WeixinModifier : AppModifier
    {
        public WeixinModifier(App config) { this.config = config; }
        public override void AfterPatchSuccess() { }
        public override void AfterPatchFail() { }

        public override string FindInstallPath()
        {
            foreach (string candidate in PathUtil.FindModernInstallPaths("Weixin", @"Tencent\Weixin", "Weixin"))
            {
                string path = ResolveInstallPath(candidate);
                if (base.IsAllFilesExist(path)) return path;
            }
            return null;
        }

        public override string ResolveInstallPath(string path)
        {
            path = PathUtil.NormalizeInstallPath(path);
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return null;
            if (File.Exists(Path.Combine(path, "Weixin.dll"))) return path;

            // The launcher version identifies the active version; directory timestamps do not.
            string version = FileUtil.GetFileVersion(Path.Combine(path, "Weixin.exe"));
            if (!string.IsNullOrEmpty(version))
            {
                string active = Path.Combine(path, version);
                return File.Exists(Path.Combine(active, "Weixin.dll")) ? active : null;
            }
            foreach (string candidate in PathUtil.GetVersionDirectories(path))
                if (File.Exists(Path.Combine(candidate, "Weixin.dll"))) return candidate;
            return null;
        }

        public override string GetVersion()
        {
            return editors == null ? "" : editors.FirstOrDefault(e => e.FileName == "Weixin.dll")?.FileVersion ?? "";
        }
    }
}
