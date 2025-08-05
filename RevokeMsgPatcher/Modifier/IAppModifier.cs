using RevokeMsgPatcher.Model;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RevokeMsgPatcher.Modifier
{
    public interface IAppModifier
    {
        App Config { get; set; }
        List<FileHexEditor> editors { get; set; }

        string FindInstallPath();
        string GetVersion();
        bool InitEditors(string installPath);
        void ValidateAndFindModifyInfo(List<string> categories);
        void Patch();
        bool Restore();
        bool BackupExists();
        bool IsAllFilesExist(string installPath);
        void SetVersionLabelAndCategoryCategories(Label lblVersion, Panel panelCategories);
        void AfterPatchSuccess();
        void AfterPatchFail();
    }
}
