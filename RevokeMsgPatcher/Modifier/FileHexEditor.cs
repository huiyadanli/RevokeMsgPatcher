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

        private string fileReplacedPath;

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

        /// <summary>
        /// 通过比对SHA1得到的特定版本的修改信息
        /// </summary>
        public ModifyInfo FileModifyInfo { get; set; }

        /// <summary>
        /// 通过比对版本范围得到的通用查找替换的修改信息
        /// </summary>
        public CommonModifyInfo FileCommonModifyInfo { get; set; }

        public FileHexEditor(string installPath, TargetInfo target)
        {
            FileTargetInfo = target.Clone();
            FileName = FileTargetInfo.Name;
            FilePath = Path.Combine(installPath, FileTargetInfo.RelativePath);
            FileBakPath = FilePath + ".h.bak";
            fileReplacedPath = FilePath + ".h.process";
        }

        /// <summary>
        /// 备份
        /// </summary>
        public void Backup()
        {
            File.Copy(FilePath, FileBakPath, true);
        }

        /// <summary>
        /// 打补丁
        /// 优先使用特定的补丁信息（存在对应SHA1信息）
        /// 不存在补丁信息，使用通用版本替换方法
        /// </summary>
        /// <returns></returns>
        public bool Patch()
        {
            FileUtil.EditMultiHex(FilePath, FileModifyInfo.Changes);
            return true;
        }

        /// <summary>
        /// 还原
        /// </summary>
        public void Restore()
        {
            File.Copy(FileBakPath, FilePath, true);
        }
    }
}
