using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;
using System.Collections.Generic;
using System.IO;
using System;

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
                // version 可能被构造函数初始化，请注意！
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
            FilePath = FileTargetInfo.GetAbsolutePath(installPath);
            FileBakPath = FilePath + ".h.bak";
            fileReplacedPath = FilePath + ".h.process";
            // if (target.RelativePathForVersion != null)
            // {
            //     version = FileUtil.GetFileVersion(Path.Combine(installPath, target.RelativePathForVersion));
            // }
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
            if (TargetChanges == null || TargetChanges.Count == 0)
            {
                throw new BusinessException("change_null", "在安装补丁时，变更的内容为空！");
            }

            if (sha1 == null || FileUtil.ComputeFileSHA1(FilePath) != sha1)
                throw new BusinessException("file_changed", "文件在验证后发生变化，请重新检测后再安装。");

            // Stage changes beside the target so replacement is atomic on the same volume.
            // A locked client file or failed write leaves the original intact.
            string stagedPath = fileReplacedPath + "." + Guid.NewGuid().ToString("N");
            try
            {
                File.Copy(FilePath, stagedPath);
                FileUtil.EditMultiHex(stagedPath, TargetChanges);
                if (FileUtil.ComputeFileSHA1(FilePath) != sha1)
                    throw new BusinessException("file_changed", "文件在安装过程中发生变化，已停止替换。");
                File.Replace(stagedPath, FilePath, null);
            }
            finally
            {
                if (File.Exists(stagedPath)) File.Delete(stagedPath);
            }
            return true;
        }

        /// <summary>
        /// 还原
        /// </summary>
        public void Restore()
        {
            string stagedPath = fileReplacedPath + "." + Guid.NewGuid().ToString("N");
            try
            {
                File.Copy(FileBakPath, stagedPath);
                File.Replace(stagedPath, FilePath, null);
            }
            finally
            {
                if (File.Exists(stagedPath)) File.Delete(stagedPath);
            }
        }
    }
}
