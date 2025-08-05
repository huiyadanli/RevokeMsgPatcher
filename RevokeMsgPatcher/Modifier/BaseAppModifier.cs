using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RevokeMsgPatcher.Modifier
{
    public abstract class BaseAppModifier : AppModifier, IAppModifier
    {
        public abstract string VersionFileName { get; }
        public App Config { get; set; }

        protected BaseAppModifier(App config)
        {
            this.Config = config;
        }

        public override string GetVersion()
        {
            if (editors != null && editors.Count > 0)
            {
                foreach (FileHexEditor editor in editors)
                {
                    if (editor.FileName == VersionFileName)
                    {
                        return editor.FileVersion;
                    }
                }
            }
            return string.Empty;
        }

        public override void AfterPatchSuccess() { }

        public override void AfterPatchFail() { }

        public bool BackupExists()
        {
            if (editors == null || editors.Count == 0)
            {
                return false;
            }
            foreach (FileHexEditor editor in editors)
            {
                if (!System.IO.File.Exists(editor.FileBakPath))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsAllFilesExist(string installPath)
        {
            if (Config.TargetFiles == null)
            {
                return true;
            }
            foreach (TargetInfo target in Config.TargetFiles)
            {
                if (!System.IO.File.Exists(target.GetAbsolutePath(installPath)))
                {
                    return false;
                }
            }
            return true;
        }

        public bool InitEditors(string installPath)
        {
            // 判断是否是安装路径
            if (!IsAllFilesExist(installPath))
            {
                return false;
            }

            // 初始化十六进制文件编辑器
            // 并寻找与之配对的版本修改信息
            editors = new List<FileHexEditor>();
            foreach (TargetInfo target in Config.TargetFiles)
            {
                editors.Add(new FileHexEditor(installPath, target));
            }

            return true;
        }

        public void SetVersionLabelAndCategoryCategories(Label lblVersion, Panel panelCategories)
        {
            lblVersion.Text = GetVersion();
            UIController.AddCategoriesToPanel(panelCategories, Config.Categories);
        }

        public void ValidateAndFindModifyInfo(List<string> categories)
        {
            foreach (FileHexEditor editor in editors)
            {
                // 寻找精确版本修改信息
                ModifyInfo modifyInfo = ModifyFinder.FindModifyInfo(editor.FileSHA1, Config.ModifyInfos);
                if (modifyInfo != null)
                {
                    editor.FileModifyInfo = modifyInfo;
                    editor.TargetChanges = modifyInfo.Changes;
                }
                else
                {
                    // 寻找通用特征码修改信息
                    CommonModifyInfo commonModifyInfo = ModifyFinder.FindCommonModifyInfo(editor.FileVersion, Config.CommonModifyInfos);
                    if (commonModifyInfo != null)
                    {
                        editor.FileCommonModifyInfo = commonModifyInfo;
                        editor.TargetChanges = ModifyFinder.FindChangesByCategories(editor.FilePath, commonModifyInfo.ReplacePatterns, categories);
                    }
                    else
                    {
                        throw new BusinessException("not_support_version", $"当前版本 {editor.FileVersion} 不支持，请等待更新！");
                    }
                }
            }
        }

        public void Patch()
        {
            foreach (FileHexEditor editor in editors)
            {
                editor.Backup();
                editor.Patch();
            }
            AfterPatchSuccess();
        }

        public bool Restore()
        {
            foreach (FileHexEditor editor in editors)
            {
                editor.Restore();
            }
            return true;
        }
    }
}
