using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;
using System.Collections.Generic;
using System.IO;

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

        public string BackupFileVersion
        {
            get
            {
                return FileUtil.GetFileVersion(FileBakPath);
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
        /// 通过比对版本范围得到的通用查找替换的修改信息（特征码替换信息）
        /// </summary>
        public CommonModifyInfo FileCommonModifyInfo { get; set; }

        /// <summary>
        /// 将要执行的修改
        /// </summary>
        public List<Change> TargetChanges { get; set; }

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
            // 不覆盖同版本的备份文件
            if (File.Exists(FileBakPath))
            {
                if (FileVersion != BackupFileVersion)
                {
                    File.Copy(FilePath, FileBakPath, true);
                }
            }
            else
            {
                File.Copy(FilePath, FileBakPath, true);
            }

        }

        /// <summary>
        /// 打补丁
        /// </summary>
        /// <returns></returns>
        public bool Patch()
        {
            if (TargetChanges == null)
            {
                throw new BusinessException("change_null", "在安装补丁时，变更的内容为空！");
            }

            FileUtil.EditMultiHex(FilePath, TargetChanges);
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
