using RevokeMsgPatcher.Forms;
using RevokeMsgPatcher.Matcher;
using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

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
        public void SetVersionLabelAndCategoryCategories(Label label, Panel panel)
        {
            string version = GetVersion();
            // 补丁信息中是否都有对应的版本
            int i = 0, j = 0;

            // 精确版本匹配
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
            }

            if (i == editors.Count)
            {
                label.Text = version + "（已支持）";
                label.ForeColor = Color.Green;
                UIController.AddMsgToPanel(panel, "只有基于特征的补丁才能选择功能");
                return;
            }

            // 模糊版本匹配（特征码）
            // 特征码匹配的时候的可选功能项
            SortedSet<string> categories = new SortedSet<string>();
            SortedSet<string> installed = new SortedSet<string>();
            foreach (FileHexEditor editor in editors) // 多种文件
            {
                // 匹配出对应版本是否有可以使用的特征
                if (config.FileCommonModifyInfos != null)
                {
                    bool inRange = false;
                    foreach (CommonModifyInfo commonModifyInfo in config.FileCommonModifyInfos[editor.FileName])
                    {
                        // editor.FileVersion 在 StartVersion 和 EndVersion 之间
                        if (IsInVersionRange(editor.FileVersion, commonModifyInfo.StartVersion, commonModifyInfo.EndVersion))
                        {
                            // 取出特征码的功能类型
                            foreach (string c in commonModifyInfo.GetCategories())
                            {
                                if (c != null)
                                {
                                    categories.Add(c);
                                }
                            }
                            // 获取已经安装过的功能类型
                            SortedSet<string> replaced = ModifyFinder.FindReplacedFunction(editor.FilePath, commonModifyInfo.ReplacePatterns);
                            foreach (string c in replaced)
                            {
                                installed.Add(c);
                            }
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
            if (j == editors.Count)
            {
                label.Text = version + "（支持特征防撤回）";
                label.ForeColor = Color.LimeGreen;
                UIController.AddCategoryCheckBoxToPanel(panel, categories.ToArray(), installed.ToArray());
            }
            else
            {
                label.Text = version + "（不支持）";
                label.ForeColor = Color.Red;
                UIController.AddMsgToPanel(panel, "无功能选项");
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
                if (VersionUtil.Compare(version, start) == 1 && VersionUtil.Compare(version, end) <= 0)
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
        /// 寻找版本对应的特征码信息
        /// </summary>
        /// <param name="editor">文件编辑器</param>
        private CommonModifyInfo FindCommonModifyInfo(FileHexEditor editor)
        {
            foreach (CommonModifyInfo commonModifyInfo in config.FileCommonModifyInfos[editor.FileName])
            {
                // editor.FileVersion 在 StartVersion 和 EndVersion 之间
                if (IsInVersionRange(editor.FileVersion, commonModifyInfo.StartVersion, commonModifyInfo.EndVersion))
                {
                    Console.WriteLine($"{commonModifyInfo.StartVersion}<{editor.FileVersion}<={commonModifyInfo.EndVersion}");
                    return commonModifyInfo;
                }
            }
            return null;
        }

        /// <summary>
        /// 文件修改器是否已经有对应的特征码修改替换信息
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public bool EditorsHasCommonModifyInfos()
        {
            int i = 0;
            for (i = 0; i < editors.Count; i++) // 多种文件
            {
                if (editors[i].FileCommonModifyInfo == null)
                {
                    break;
                }
            }
            if (i == editors.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
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
        /// <param name="categories">操作类型（防撤回或者多开等）,为空则是所有操作</param>
        public void ValidateAndFindModifyInfo(List<string> categories)
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

                // 补丁前SHA1匹配上，肯定是正确的dll
                if (matchingSHA1Before != null)
                {
                    editor.FileModifyInfo = matchingSHA1Before;
                    editor.TargetChanges = matchingSHA1Before.Changes;
                    continue;
                }
                // 补丁后SHA1匹配上，肯定已经打过补丁
                if (matchingSHA1After != null)
                {
                    throw new BusinessException("installed", $"你已经安装过此补丁！");
                }

                // SHA1不匹配说明精准替换肯定不支持
                if (matchingSHA1Before == null && matchingSHA1After == null)
                {
                    // 尝试使用特征码替换
                    // 多个版本范围，匹配出对应版本可以使用的特征
                    if (config.FileCommonModifyInfos != null)
                    {
                        editor.FileCommonModifyInfo = FindCommonModifyInfo(editor);
                    }

                    // 存在对应的特征时不报错
                    if (editor.FileCommonModifyInfo != null && editor.FileCommonModifyInfo.ReplacePatterns != null)
                    {
                        List<ReplacePattern> replacePatterns = editor.FileCommonModifyInfo.ReplacePatterns;
                        // 根据需要操作的功能类型（防撤回或者多开等）筛选特征码
                        if (categories != null && categories.Count > 0)
                        {
                            replacePatterns = editor.FileCommonModifyInfo.ReplacePatterns.Where(info => categories.Contains(info.Category)).ToList();
                        }
                        // 如果能顺利得到 TargetChanges 不报错则可以使用特征替换方式
                        editor.TargetChanges = ModifyFinder.FindChanges(editor.FilePath, replacePatterns);
                        continue;
                    }
                    else
                    {
                        // SHA1不匹配，连版本也不匹配，说明完全不支持
                        if (matchingVersion == null)
                        {
                            throw new BusinessException("not_support", $"不支持此版本：{editor.FileVersion}！");
                        }
                        // SHA1不匹配，但是版本匹配，可能dll已经被其他补丁程序修改过
                        if (matchingVersion != null)
                        {
                            throw new BusinessException("maybe_modified", $"程序支持此版本：{editor.FileVersion}。但是文件校验不通过，请确认是否使用过其他补丁程序！");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// c.根据补丁信息，安装补丁
        /// 两种打补丁的方式：精准（指定位置替换）、通用（特征码替换）
        /// </summary>
        /// <returns></returns>
        public bool Patch()
        {
            // 首先验证文件修改器是否没问题
            foreach (FileHexEditor editor in editors)
            {
                if (editor == null)
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
                        // 此处还原逻辑不可能进入
                        editor.Restore();
                    }
                    else
                    {
                        done.Add(editor);
                    }
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
                if (!File.Exists(editor.FileBakPath) || editor.FileVersion != editor.BackupFileVersion)
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
                    string currVersion = editor.FileVersion;
                    string bakVersion = editor.BackupFileVersion;
                    if (currVersion != bakVersion)
                    {
                        DialogResult dr = MessageBox.Show(
                            $"当前文件：{editor.FilePath}，版本：{currVersion}；" + Environment.NewLine +
                            $"备份文件：{editor.FileBakPath}，版本：{bakVersion}；" + Environment.NewLine +
                            $"两者版本不一致，是否继续还原？",
                            "提示 ", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                        if (dr == DialogResult.OK)
                        {
                            editor.Restore();
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        editor.Restore();
                    }
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
