using RevokeMsgPatcher.Modifier;
using RevokeMsgPatcher.Utils;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevokeMsgPatcher
{
    public partial class FormMain : Form
    {
        WechatModifier wechatModifier = new WechatModifier();

        public FormMain()
        {
            InitializeComponent();

            // 标题加上版本号
            string currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (currentVersion.Length > 3)
            {
                currentVersion = " v" + currentVersion.Substring(0, 3);
            }
            this.Text += currentVersion;

            // 自动获取应用安装路径
            txtPath.Text = wechatModifier.FindInstallPath();
            // 显示是否能够备份还原
            if (!string.IsNullOrEmpty(txtPath.Text))
            {
                wechatModifier.InitEditors(txtPath.Text);
                btnRestore.Enabled = wechatModifier.BackupExists();
            }

        }

        private void btnPatch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPath.Text) || !wechatModifier.IsAllFilesExist(txtPath.Text))
            {
                MessageBox.Show("请选择微信安装路径!");
                return;
            }
            // a.重新初始化编辑器
            wechatModifier.InitEditors(txtPath.Text);
            btnPatch.Enabled = false;
            // b.计算SHA1，验证文件完整性，寻找对应的补丁信息
            try
            {
                wechatModifier.ValidateAndFindModifyInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            // c.打补丁
            try
            {
                wechatModifier.Patch();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message + " 请以管理员权限启动本程序，并确认微信处于关闭状态。");
            }
            btnPatch.Enabled = true;
        }

        private void txtPath_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPath.Text))
            {
                wechatModifier.InitEditors(txtPath.Text);
                btnRestore.Enabled = wechatModifier.BackupExists();
            }
        }

        private void btnChoosePath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择微信安装路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath) || !wechatModifier.IsAllFilesExist(dialog.SelectedPath))
                {
                    MessageBox.Show("无法找到微信关键文件，请选择正确的微信安装路径!");
                }
                else
                {
                    txtPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            btnRestore.Enabled = false;
            try
            {
                wechatModifier.Restore();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }
            btnRestore.Enabled = wechatModifier.BackupExists();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/huiyadanli/RevokeMsgPatcher");
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            // 异步获取最新的补丁信息
            Task<string> t = new Task<string>(() =>
            {
                return new WebClient().DownloadString("https://huiyadanli.coding.me/i/patch.json");
            });
            t.Start();
            string json = await t;
            if(string.IsNullOrEmpty(json))
            {
                lblUpdatePachJson.Text = "获取失败";

            } else
            {
                //patcher.SetNewPatchJson(json);
                lblUpdatePachJson.Text = "获取成功";
            }

        }

        private void lblUpdatePachJson_Click(object sender, EventArgs e)
        {
            string versions = "";
            //patcher.TargetFiles.ForEach(t =>
            //{
            //    versions += t.Version + Environment.NewLine;
            //});
            MessageBox.Show("当前所支持的微信版本:" + Environment.NewLine + versions);
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            PathUtil.DisplayAllProgram();
        }
    }
}
