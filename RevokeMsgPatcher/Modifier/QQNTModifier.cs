using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;
using System;
using System.IO;
using System.Linq;

namespace RevokeMsgPatcher.Modifier
{
    class QQNTModifier : AppModifier
    {
        public QQNTModifier(App config) { this.config = config; }

        public override string FindInstallPath()
        {
            foreach (string key in new[] { "QQ", "QQNT" })
                foreach (string candidate in PathUtil.FindModernInstallPaths(key, @"Tencent\QQNT", "QQ"))
                    if (IsAllFilesExist(candidate)) return candidate;
            return null;
        }

        public override string GetVersion()
        {
            var editor = editors?.FirstOrDefault(e => e.FileName == "wrapper.node");
            if (editor == null) return "";
            // wrapper.node has an internal version unrelated to the QQ.exe launcher version.
            // Display both while still matching patches against the actual module version.
            var appDirectory = new DirectoryInfo(Path.GetDirectoryName(editor.FilePath));
            var versionDirectory = appDirectory.Parent?.Parent;
            Version version;
            if (versionDirectory != null && Version.TryParse(versionDirectory.Name.Replace('-', '.'), out version))
                return versionDirectory.Name + "（模块 " + editor.FileVersion + "）";
            return editor.FileVersion;
        }

        public override void AfterPatchSuccess() { }
        public override void AfterPatchFail() { }
    }
}
