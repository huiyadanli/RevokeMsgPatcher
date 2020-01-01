using RevokeMsgPatcher.Matcher;
using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Model.Enum;
using RevokeMsgPatcher.Utils;
using System;
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
                return  FileUtil.GetFileVersion(FileBakPath);
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
        /// </summary>
        /// <param name="type">两种打补丁的方式：精准（指定位置替换）、通用（特征码替换）</param>
        /// <returns></returns>
        public bool Patch(PatchType type)
        {
            if (type == PatchType.Accurate)
            {
                AccuratePatch();

            }
            else
            {
                CommonPatch();
            }
            return true;
        }

        /// <summary>
        /// 精准（指定位置替换）
        /// </summary>
        public void AccuratePatch()
        {
            FileUtil.EditMultiHex(FilePath, FileModifyInfo.Changes);
        }

        /// <summary>
        /// 通用（特征码替换）
        /// </summary>
        public void CommonPatch()
        {
            if (FileCommonModifyInfo == null)
            {
                throw new Exception("特征码替换：缺失对应特征码信息");
            }
            // 1. 拷贝一份临时文件
            File.Copy(FilePath, fileReplacedPath, true);
            // 2. 读取整个临时文件
            byte[] fileByteArray = File.ReadAllBytes(fileReplacedPath);
            // 3. 循环查找所有未替换的替换点
            int needReplaceNum = 0;
            List<Change> changes = new List<Change>();
            foreach (ReplacePattern pattern in FileCommonModifyInfo.ReplacePatterns)
            {
                int[] indexs = FuzzyMatcher.MatchNotReplaced(fileByteArray, pattern.Search, pattern.Replace);
                if (indexs.Length == 1)
                {
                    needReplaceNum++;
                    changes.Add(new Change(indexs[0], pattern.Replace));
                }
            }
            // 判断是否可以使用特征码替换的方式
            if (needReplaceNum == 0)
            {
                // 查找所有替换点
                int matchNum = 0;
                foreach (ReplacePattern pattern in FileCommonModifyInfo.ReplacePatterns)
                {
                    int[] indexs = FuzzyMatcher.MatchAll(fileByteArray, pattern.Search);
                    if (indexs.Length == 1)
                    {
                        matchNum++;
                    }
                }
                if (matchNum == FileCommonModifyInfo.ReplacePatterns.Count)
                {
                    throw new BusinessException("already_replace", "特征码替换：当前应用已经防撤回");
                }
                else
                {
                    throw new BusinessException("not_found_to_replace", "特征码替换：没有搜索到撤回的相关特征");
                }
            }
            else if (needReplaceNum == FileCommonModifyInfo.ReplacePatterns.Count)
            {
                // 正常情况下每个替换点都能找到
                // 3. 替换所有未替换的替换点
                FileUtil.EditMultiHex(fileReplacedPath, changes);
                // 4. 覆盖特征码替换后的文件
                File.Copy(fileReplacedPath, FilePath, true);
            }
            else
            {
                throw new BusinessException("found_num_err", "特征码替换：可替换的特征数不正确");
            }
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
