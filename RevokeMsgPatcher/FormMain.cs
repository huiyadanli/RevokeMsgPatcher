using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Modifier;
using RevokeMsgPatcher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevokeMsgPatcher
{
    public partial class FormMain : Form
    {
        AppModifier modifier = null;

        public void InitModifier()
        {
            Bag bag = new JsonData().Bag();

            WechatModifier wechatModifier = new WechatModifier(bag.Apps["Wechat"]);
            QQModifier qqModifier = new QQModifier(bag.Apps["QQ"]);
            TIMModifier timModifier = new TIMModifier(bag.Apps["TIM"]);

            rbtWechat.Tag = wechatModifier;
            rbtQQ.Tag = qqModifier;
            rbtTIM.Tag = timModifier;

            // 默认微信
            rbtWechat.Enabled = true;
            modifier = wechatModifier;
        }

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

            InitModifier();
            InitControls();

        }

        private void InitControls()
        {
            // 自动获取应用安装路径
            txtPath.Text = modifier.FindInstallPath();
            lblVersion.Text = modifier.GetVersion();
            // 显示是否能够备份还原
            if (!string.IsNullOrEmpty(txtPath.Text))
            {
                modifier.InitEditors(txtPath.Text);
                btnRestore.Enabled = modifier.BackupExists();
            }
        }

        private void btnPatch_Click(object sender, EventArgs e)
        {
            if (!modifier.IsAllFilesExist(txtPath.Text))
            {
                MessageBox.Show("请选择正确的安装路径!");
                return;
            }
            EnableAllButton(false);
            // a.重新初始化编辑器
            modifier.InitEditors(txtPath.Text);
            // b.计算SHA1，验证文件完整性，寻找对应的补丁信息
            try
            {
                modifier.ValidateAndFindModifyInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                EnableAllButton(true);
                btnRestore.Enabled = modifier.BackupExists();
                return;
            }
            // c.打补丁
            try
            {
                modifier.Patch();
                MessageBox.Show("补丁安装成功！");
                EnableAllButton(true);
                btnRestore.Enabled = modifier.BackupExists();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message + " 请以管理员权限启动本程序，并确认微信处于关闭状态。");
                EnableAllButton(true);
                btnRestore.Enabled = modifier.BackupExists();
            }
        }

        private void txtPath_TextChanged(object sender, EventArgs e)
        {
            if (modifier.IsAllFilesExist(txtPath.Text))
            {
                modifier.InitEditors(txtPath.Text);
                btnRestore.Enabled = modifier.BackupExists();
            }
            else
            {
                btnPatch.Enabled = false;
                btnRestore.Enabled = false;
            }
        }

        private void btnChoosePath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择安装路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath) || !modifier.IsAllFilesExist(dialog.SelectedPath))
                {
                    MessageBox.Show("无法找到此应用的关键文件，请选择正确的安装路径!");
                }
                else
                {
                    txtPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            EnableAllButton(false);
            try
            {
                modifier.Restore();
                MessageBox.Show("还原成功！");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }
            EnableAllButton(true);
            btnRestore.Enabled = modifier.BackupExists();
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
            if (string.IsNullOrEmpty(json))
            {
                lblUpdatePachJson.Text = "[ 获取失败 ]";

            }
            else
            {
                //patcher.SetNewPatchJson(json);
                lblUpdatePachJson.Text = "[ 获取成功 ]";
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

        private void radioButtons_CheckedChanged(object sender, EventArgs e)
        {
            EnableAllButton(false);
            RadioButton radioButton = sender as RadioButton;
            // 切换使用不同的防撤回对象
            if (rbtWechat.Checked)
            {
                modifier = (WechatModifier)rbtWechat.Tag;
            }
            else if (rbtQQ.Checked)
            {
                modifier = (QQModifier)rbtQQ.Tag;
            }
            else if (rbtTIM.Checked)
            {
                modifier = (TIMModifier)rbtTIM.Tag;
            }
            txtPath.Text = modifier.FindInstallPath();
            lblVersion.Text = modifier.GetVersion();
            EnableAllButton(true);
            // 显示是否能够备份还原
            if (!string.IsNullOrEmpty(txtPath.Text))
            {
                modifier.InitEditors(txtPath.Text);
                btnRestore.Enabled = modifier.BackupExists();
            }
        }

        private void EnableAllButton(bool state)
        {
            foreach (Control c in this.Controls)
            {
                if (c is Button)
                {
                    c.Enabled = state;
                }
            }
        }
    }
}
