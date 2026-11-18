using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;
using RevokeMsgPatcher.Matcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace RevokeMsgPatcher.Modifier
{
    class QQNTModifier : AppModifier
    {
        private const string AdaptiveCategory = "防撤回（自动分析）";

        protected override bool UseAdaptiveAnalysisOnly { get { return true; } }

        public QQNTModifier(App config)
        {
            this.config = config;
        }

        /// <summary>
        /// 自动寻找QQ安装路径
        /// </summary>
        /// <returns></returns>
        public override string FindInstallPath()
        {
            try
            {
                string installPath = PathUtil.FindInstallPathFromRegistryWOW6432Node("QQ");
                if (!string.IsNullOrEmpty(installPath))
                {
                    installPath = Path.GetDirectoryName(installPath);
                    if (IsAllFilesExist(installPath))
                    {
                        return installPath;
                    }
                }
            
                installPath = PathUtil.FindInstallPathFromRegistry("QQNT");
                if (!IsAllFilesExist(installPath))
                {
                    List<string> defaultPathList = PathUtil.GetDefaultInstallPaths(@"Tencent\QQNT");
                    foreach (string defaultPath in defaultPathList)
                    {
                        if (IsAllFilesExist(defaultPath))
                        {
                            return defaultPath;
                        }
                    }
                }
                else
                {
                    return installPath;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            return null;
        }

        /// <summary>
        /// 获取QQNT版本
        /// </summary>
        /// <returns></returns>
        public override string GetVersion()
        {
            if (editors != null && editors.Count > 0)
            {
                foreach (FileHexEditor editor in editors)
                {
                    if (editor.FileName == "wrapper.node")
                    {
                        return editor.FileVersion;
                    }
                }
            }
            return "";
        }

        protected override bool TryGetAdaptiveCategories(FileHexEditor editor, out string[] categories, out string[] installed, out string status)
        {
            categories = new[] { AdaptiveCategory };
            installed = new string[0];
            status = null;
            if (editor.FileName != "wrapper.node") return false;
            try
            {
                QQNTAdaptiveResult result = QQNTAdaptiveLocator.Analyze(editor.FilePath);
                if (result.IsPatched) installed = new[] { AdaptiveCategory };
                return true;
            }
            catch (BusinessException error)
            {
                status = error.Message;
                return false;
            }
        }

        protected override bool TryFindAdaptiveChanges(FileHexEditor editor, List<string> categories, out List<Change> changes)
        {
            changes = null;
            if (editor.FileName != "wrapper.node") return false;
            if (categories != null && categories.Count > 0 && !categories.Contains(AdaptiveCategory)) return false;
            QQNTAdaptiveResult result = QQNTAdaptiveLocator.Analyze(editor.FilePath);
            if (result.IsPatched)
                throw new BusinessException("match_already_replace", "自动分析：当前 QQNT 已安装防撤回补丁。");
            changes = result.Changes;
            return changes != null && changes.Count == 2;
        }

        public bool IsQQRunning()
        {
            return Process.GetProcessesByName("QQ").Length > 0;
        }

        public void StopQQProcesses()
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(15);
            while (DateTime.UtcNow < deadline)
            {
                Process[] processes = Process.GetProcessesByName("QQ");
                if (processes.Length == 0) return;

                foreach (Process process in processes)
                {
                    try
                    {
                        if (!process.HasExited) process.CloseMainWindow();
                    }
                    catch { }
                }
                Thread.Sleep(500);
                foreach (Process process in Process.GetProcessesByName("QQ"))
                {
                    try
                    {
                        if (!process.HasExited) process.Kill();
                    }
                    catch { }
                }
                Thread.Sleep(350);
            }

            if (Process.GetProcessesByName("QQ").Length > 0)
                throw new BusinessException("qq_running", "无法关闭 QQ，请手动退出后重试！");
        }

        public override void AfterPatchSuccess()
        {

        }

        public override void AfterPatchFail()
        {

        }

        public new bool Restore()
        {
            AfterPatchFail();
            return base.Restore();
        }
    }
}
