using RevokeMsgPatcher.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web.Script.Serialization;
using File = System.IO.File;

namespace RevokeMsgPatcher
{
    public class Patcher
    {
        private string intallPath;
        public string IntallPath
        {
            get { return intallPath; }
            set
            {
                intallPath = value;
                if (!string.IsNullOrEmpty(intallPath) && Util.IsWechatInstallPath(intallPath))
                {
                    dllPath = Path.Combine(intallPath, "WeChatWin.dll");
                    bakPath = Path.Combine(intallPath, "WeChatWin.dll.h.bak");
                }
                else
                {
                    intallPath = null;
                    dllPath = null;
                    bakPath = null;
                }
            }
        }

        private string dllPath;
        private string bakPath;

        public string DllPath { get => dllPath; }
        public string BakPath { get => bakPath; }

        private List<TargetFile> targetFiles;
        private TargetFile currentFile;

        public List<TargetFile> TargetFiles { get => targetFiles; }

        public Patcher()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            targetFiles = serializer.Deserialize<List<TargetFile>>(Properties.Resources.PatchJson);
        }

        public void SetNewPatchJson(string json)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            targetFiles = serializer.Deserialize<List<TargetFile>>(json);
        }

        public string JudgeVersion()
        {
            string sha1 = Util.ComputeFileSHA1(dllPath);
            Debug.WriteLine(sha1);

            foreach (TargetFile t in targetFiles)
            {
                if (sha1 == t.SHA1After)
                {
                    return "done";
                }
                if (sha1 == t.SHA1Before)
                {
                    currentFile = t;
                    return t.Version;
                }
            }
            return null;
        }

        public bool Patch()
        {
            if (currentFile != null)
            {
                File.Copy(dllPath, bakPath, true);
                bool done = Util.EditHex(dllPath, currentFile.Position, currentFile.Content);
                if (!done)
                {
                    File.Copy(bakPath, dllPath, true);
                }
                return done;
            }
            return false;
        }

    }
}
