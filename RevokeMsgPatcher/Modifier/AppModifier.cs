using RevokeMsgPatcher.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace RevokeMsgPatcher.Modifier
{
    /// <summary>
    /// 1. 自动获取安装目录（已验证） 或者手动填写安装目录
    /// 2. 验证安装目录 验证要修改的 dll 是否存在
    /// 3. 判断所有 dll 是否符合防撤回要求
    /// 4. 备份所有 dll  // *.h.bak
    /// 5. 根据每个 dll 匹配的 修改信息 循环修改
    /// </summary>
    public abstract class AppModifier
    {
        protected App config;

        public App Config { set { config = value; } get { return config; } }

        protected List<FileHexEditor> editors;

        public string InstallPath { get; set; }

        /// <summary>
        /// 自动搜索应用安装路径
        /// </summary>
        /// <returns>应用安装路径</returns>
        public abstract string FindInstallPath();

        //public abstract bool ValidateAndInitialize(string installPath);

        /// <summary>
        /// 获取版本号
        /// </summary>
        /// <returns></returns>
        public abstract string GetVersion();

        /// <summary>
        /// 操作版本号显示控件的内容和样式
        /// </summary>
        /// <param name="label">显示版本的控件</param>
        public void SetVersionLabel(System.Windows.Forms.Label label)
        {
            string version = GetVersion();
            // 补丁信息中是否都有对应的版本
            int i = 0, j = 0;
            foreach (FileHexEditor editor in editors) // 多种文件
            {
                // 精确版本匹配
                bool haven = false;
                foreach (ModifyInfo modifyInfo in config.FileModifyInfos[editor.FileName]) // 多个版本信息
                {
                    if (editor.FileVersion == modifyInfo.Version)
                    {
                        haven = true;
                        break;
                    }
                }
                if (haven)
                {
                    i++;
                }

                // 匹配出对应版本是否有可以使用的特征
                if (config.FileCommonModifyInfos != null)
                {
                    bool inRange = false;
                    foreach (CommonModifyInfo commonModifyInfo in config.FileCommonModifyInfos[editor.FileName])
                    {
                        // editor.FileVersion 在 StartVersion 和 EndVersion 之间
                        if (IsInVersionRange(editor.FileVersion, commonModifyInfo.StartVersion, commonModifyInfo.EndVersion))
                        {
                            inRange = true;
                            break;
                        }
                    }
                    if (inRange)
                    {
                        j++;
                    }
                }
            }

            // 全部都有对应匹配的版本
            if (i == editors.Count)
            {
                label.Text = version + "（已支持）";
                label.ForeColor = Color.Green;
            }
            else if (j == editors.Count)
            {
                label.Text = version + "（支持特征码防撤回）";
                label.ForeColor = Color.LimeGreen;
            }
            else
            {
                label.Text = version + "（不支持）";
                label.ForeColor = Color.Red;
            }

        }

        /// <summary>
        /// 判断APP安装路径内是否都存在要修改的文件
        /// </summary>
        /// <param name="installPath">APP安装路径</param>
        /// <returns></returns>
        public bool IsAllFilesExist(string installPath)
        {
            if (string.IsNullOrEmpty(installPath))
            {
                return false;
            }
            int success = 0;
            foreach (TargetInfo info in config.FileTargetInfos.Values)
            {
                string filePath = Path.Combine(installPath, info.RelativePath);
                if (File.Exists(filePath))
                {
                    success++;
                }
            }
            if (success == config.FileTargetInfos.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 判断版本是否处于版本范围，特殊版本的可以重载此方法
        /// </summary>
        /// <param name="version">当前版本</param>
        /// <param name="start">起始版本</param>
        /// <param name="end">结束版本,为空为不限制</param>
        /// <returns></returns>
        public bool IsInVersionRange(string version, string start, string end)
        {
            try
            {
                int v = Convert.ToInt32(version.Replace(".", ""));
                int s = Convert.ToInt32(start.Replace(".", ""));
                int e = 0;
                if (string.IsNullOrEmpty(end))
                {
                    e = int.MaxValue;
                }
                else
                {
                    e = Convert.ToInt32(end.Replace(".", ""));
                }
                if (v >= s && v <= e)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("判断版本范围时出错：" + e.Message);
            }
            return false;
        }

        /// <summary>
        /// a.初始化修改器
        /// </summary>
        /// <param name="installPath">APP安装路径</param>
        public void InitEditors(string installPath)
        {
            // 初始化文件修改器
            editors = new List<FileHexEditor>();
            foreach (TargetInfo info in config.FileTargetInfos.Values)
            {
                FileHexEditor editor = new FileHexEditor(installPath, info);
                editors.Add(editor);
            }

        }

        /// <summary>
        /// b.验证文件完整性，寻找对应的补丁信息
        /// </summary>
        public void ValidateAndFindModifyInfo()
        {
            // 寻找对应文件版本与SHA1的修改信息
            foreach (FileHexEditor editor in editors) // 多种文件
            {
                // 通过SHA1和文件版本判断是否可以打补丁 根据不同结果返回不同的提示
                ModifyInfo matchingSHA1Before = null, matchingSHA1After = null, matchingVersion = null;
                foreach (ModifyInfo modifyInfo in config.FileModifyInfos[editor.FileName]) // 多个版本信息
                {
                    if (modifyInfo.Name == editor.FileName) // 保险用的无用判断
                    {
                        if (editor.FileSHA1 == modifyInfo.SHA1After)
                        {
                            matchingSHA1After = modifyInfo;
                        }
                        else if (editor.FileSHA1 == modifyInfo.SHA1Before)
                        {
                            matchingSHA1Before = modifyInfo;
                        }

                        if (editor.FileVersion == modifyInfo.Version)
                        {
                            matchingVersion = modifyInfo;
                        }
                    }
                }

                // 多个版本范围，匹配出对应版本可以使用的特征
                if (config.FileCommonModifyInfos != null)
                {
                    foreach (CommonModifyInfo commonModifyInfo in config.FileCommonModifyInfos[editor.FileName])
                    {
                        // editor.FileVersion 在 StartVersion 和 EndVersion 之间
                        if (IsInVersionRange(editor.FileVersion, commonModifyInfo.StartVersion, commonModifyInfo.EndVersion))
                        {
                            editor.FileCommonModifyInfo = commonModifyInfo;
                            break;
                        }
                    }
                }

                // 补丁前SHA1匹配上，肯定是正确的dll
                if (matchingSHA1Before != null)
                {
                    editor.FileModifyInfo = matchingSHA1Before;
                    continue;
                }
                // 补丁后SHA1匹配上，肯定已经打过补丁
                if (matchingSHA1After != null)
                {
                    throw new BusinessException("installed", $"你已经安装过此补丁，文件路径：{editor.FilePath}");
                }
                // 全部不匹配，说明不支持
                if (matchingSHA1Before == null && matchingSHA1After == null && matchingVersion == null)
                {
                    throw new BusinessException("not_support", $"不支持此版本：{editor.FileVersion}，文件路径：{editor.FilePath}");
                }
                // SHA1不匹配，版本匹配，可能dll已经被其他补丁程序修改过
                if ((matchingSHA1Before == null && matchingSHA1After == null) && matchingVersion != null)
                {
                    throw new BusinessException("maybe_modified", $"程序支持此版本：{editor.FileVersion}。但是文件校验不通过，请确认是否使用过其他补丁程序。文件路径：{editor.FilePath}");
                }
            }
        }

        /// <summary>
        /// c.根据补丁信息，安装补丁
        /// </summary>
        /// <returns></returns>
        public bool Patch()
        {
            // 首先验证文件修改器是否没问题
            foreach (FileHexEditor editor in editors)
            {
                if (editor.FileModifyInfo == null)
                {
                    throw new Exception("补丁安装失败，原因：文件修改器初始化失败！");
                }
            }
            // 再备份所有文件
            foreach (FileHexEditor editor in editors)
            {
                editor.Backup();
            }
            // 打补丁！
            List<FileHexEditor> done = new List<FileHexEditor>(); // 已经打上补丁的
            try
            {
                foreach (FileHexEditor editor in editors)
                {
                    bool success = editor.Patch();
                    if (!success)
                    {
                        editor.Restore();
                    }
                    done.Add(editor);
                }
            }
            catch (Exception ex)
            {
                // 恢复所有已经打上补丁的文件
                foreach (FileHexEditor editor in done)
                {
                    editor.Restore();
                }
                throw ex;
            }
            return true;
        }

        public bool BackupExists()
        {
            foreach (FileHexEditor editor in editors)
            {
                if (!File.Exists(editor.FileBakPath))
                {
                    return false;
                }
            }
            return true;
        }

        public bool Restore()
        {
            if (BackupExists())
            {
                foreach (FileHexEditor editor in editors)
                {
                    editor.Restore();
                }
                return true;
            }
            else
            {
                throw new Exception("备份文件不存在，还原失败！");
            }
        }
    }
}
