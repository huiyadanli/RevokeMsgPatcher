using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Modifier
{
    public class FileHexEditor
    {
        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string FileBakPath { get; set; }

        private string version;
        public string FileVersion
        {
            get
            {
                if (version == null)
                {
                    version = FileUtil.GetFileVersion(FilePath);
                }
                return version;
            }
        }

        public string sha1;
        public string FileSHA1
        {
            get
            {
                if (sha1 == null)
                {
                    sha1 = FileUtil.ComputeFileSHA1(FilePath);
                }
                return sha1;
            }
        }

        public TargetInfo FileTargetInfo { get; set; }

        public ModifyInfo FileModifyInfo { get; set; }

        public FileHexEditor(string installPath, TargetInfo target)
        {
            FileTargetInfo = target.Clone();
            FileName = FileTargetInfo.Name;
            FilePath = Path.Combine(installPath, FileTargetInfo.RelativePath);
            FileBakPath = FilePath + ".h.bak";
        }

        public void Backup()
        {
            File.Copy(FilePath, FileBakPath, true);
        }

        public bool Patch()
        {
            FileUtil.EditMultiHex(FilePath, FileModifyInfo.Changes);
            return true;
        }

        public void Restore()
        {
            File.Copy(FileBakPath, FilePath, true);
        }
    }
}
