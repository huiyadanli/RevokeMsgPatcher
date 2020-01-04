using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Modifier;
using RevokeMsgPatcher.Utils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace RevokeMsgPatcher
{
    public partial class FormMain : Form
    {
        // 当前使用的修改者
        private AppModifier modifier = null;

        private WechatModifier wechatModifier = null;
        private QQModifier qqModifier = null;
        private TIMModifier timModifier = null;
        private QQLiteModifier qqLiteModifier = null;

        private string thisVersion;
        private bool needUpdate = false;

        private GAHelper ga = new GAHelper(); // Google Analytics 记录

        public void InitModifier()
        {
            // 从配置文件中读取配置
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Bag bag = serializer.Deserialize<Bag>(Properties.Resources.PatchJson);

            // 初始化每个应用对应的修改者
            wechatModifier = new WechatModifier(bag.Apps["Wechat"]);
            qqModifier = new QQModifier(bag.Apps["QQ"]);
            timModifier = new TIMModifier(bag.Apps["TIM"]);
            qqLiteModifier = new QQLiteModifier(bag.Apps["QQLite"]);

            rbtWechat.Tag = wechatModifier;
            rbtQQ.Tag = qqModifier;
            rbtTIM.Tag = timModifier;
            rbtQQLite.Tag = qqLiteModifier;

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
                thisVersion = currentVersion.Substring(0, 3);
                currentVersion = " v" + thisVersion;
            }
            this.Text += currentVersion;

            InitModifier();
            InitControls();

            ga.RequestPageView($"/main/{thisVersion}", $"进入{thisVersion}版本主界面");
        }

        private void InitControls()
        {
            // 自动获取应用安装路径
            txtPath.Text = modifier.FindInstallPath();
            btnRestore.Enabled = false;
            // 显示是否能够备份还原
            if (!string.IsNullOrEmpty(txtPath.Text))
            {
                modifier.InitEditors(txtPath.Text);
                modifier.SetVersionLabel(lblVersion);
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

            // 记录点了什么应用的防撤回
            string enName = GetCheckedRadioButtonNameEn(); // 应用英文名
            string version = modifier.GetVersion(); // 应用版本
            ga.RequestPageView($"{enName}/{version}/patch", "点击防撤回");

            EnableAllButton(false);
            // a.重新初始化编辑器
            modifier.InitEditors(txtPath.Text);
            // b.计算SHA1，验证文件完整性，寻找对应的补丁信息（精确版本、通用特征码两种补丁信息）
            try
            {
                modifier.ValidateAndFindModifyInfo();
            }
            catch (BusinessException ex)
            {
                ga.RequestPageView($"{enName}/{version}/patch/sha1/ex/{ex.ErrorCode}", ex.Message);
                MessageBox.Show(ex.Message);
                EnableAllButton(true);
                btnRestore.Enabled = modifier.BackupExists();
                return;
            }
            catch (IOException ex)
            {
                ga.RequestPageView($"{enName}/{version}/patch/sha1/ex/{ex.HResult.ToString("x4")}", ex.Message);
                MessageBox.Show(ex.Message + " 请以管理员权限启动本程序，并确认当前应用（微信/QQ/TIM）处于关闭状态。");
                EnableAllButton(true);
                btnRestore.Enabled = modifier.BackupExists();
                return;
            }
            catch (Exception ex)
            {
                ga.RequestPageView($"{enName}/{version}/patch/sha1/ex/{ex.HResult.ToString("x4")}", ex.Message);
                MessageBox.Show(ex.Message);
                EnableAllButton(true);
                btnRestore.Enabled = modifier.BackupExists();
                return;
            }

            // c.打补丁
            try
            {
                modifier.Patch();
                ga.RequestPageView($"{enName}/{version}/patch/succ", "防撤回成功");
                MessageBox.Show("补丁安装成功！");
            }
            catch (BusinessException ex)
            {
                Console.WriteLine(ex.Message);
                ga.RequestPageView($"{enName}/{version}/patch/ex/{ex.ErrorCode}", ex.Message);
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ga.RequestPageView($"{enName}/{version}/patch/ex/{ex.HResult.ToString("x4")}", ex.Message);
                MessageBox.Show(ex.Message + " 请以管理员权限启动本程序，并确认当前应用（微信/QQ/TIM）处于关闭状态。");
            }
            finally
            {
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
                    btnRestore.Enabled = false;
                    // 显示是否能够备份还原
                    if (!string.IsNullOrEmpty(txtPath.Text))
                    {
                        modifier.InitEditors(txtPath.Text);
                        modifier.SetVersionLabel(lblVersion);
                        btnRestore.Enabled = modifier.BackupExists();
                    }
                }
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            EnableAllButton(false);
            try
            {
                bool succ = modifier.Restore();
                if (succ)
                {
                    MessageBox.Show("还原成功！");
                }
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
            string json = await HttpUtil.GetPatchJsonAsync();
            if (string.IsNullOrEmpty(json))
            {
                lblUpdatePachJson.Text = "[ 获取最新补丁信息失败 ]";
            }
            else
            {
                try
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    Bag bag = serializer.Deserialize<Bag>(json);

                    wechatModifier.Config = bag.Apps["Wechat"];
                    qqModifier.Config = bag.Apps["QQ"];
                    timModifier.Config = bag.Apps["TIM"];
                    qqLiteModifier.Config = bag.Apps["QQLite"];

                    if (Convert.ToDecimal(bag.LatestVersion) > Convert.ToDecimal(thisVersion))
                    {
                        needUpdate = true;
                        lblUpdatePachJson.Text = $"[ 存在最新版本 {bag.LatestVersion} ]";
                        lblUpdatePachJson.ForeColor = Color.Red;
                    }
                    else
                    {
                        needUpdate = false;
                        lblUpdatePachJson.Text = "[ 获取成功，点击查看更多信息 ]";
                        lblUpdatePachJson.ForeColor = Color.RoyalBlue;
                    }
                    InitControls();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    lblUpdatePachJson.Text = "[ 更换新配置时异常 ]";
                }
            }
        }

        private void lblUpdatePachJson_Click(object sender, EventArgs e)
        {
            string tips = "";
            if (needUpdate)
            {
                tips += "【当前存在最新版本，点击确定进入软件主页下载最新版本。】" + Environment.NewLine + Environment.NewLine;
            }
            tips += "支持以下版本" + Environment.NewLine;
            tips += " ➯ 微信：" + wechatModifier.Config.GetSupportVersionStr() + Environment.NewLine;
            tips += " ➯ QQ：" + qqModifier.Config.GetSupportVersionStr() + Environment.NewLine;
            tips += " ➯ QQ轻聊版：" + qqLiteModifier.Config.GetSupportVersionStr() + Environment.NewLine;
            tips += " ➯ TIM：" + timModifier.Config.GetSupportVersionStr() + Environment.NewLine;

            DialogResult dr = MessageBox.Show(tips, "当前支持防撤回的版本", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK && needUpdate)
            {
                Process.Start("https://github.com/huiyadanli/RevokeMsgPatcher/releases");
            }
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
            else if (rbtQQLite.Checked)
            {
                modifier = (QQLiteModifier)rbtQQLite.Tag;
            }
            txtPath.Text = modifier.FindInstallPath();
            EnableAllButton(true);
            lblVersion.Text = "";
            btnRestore.Enabled = false;
            // 显示是否能够备份还原
            if (!string.IsNullOrEmpty(txtPath.Text))
            {
                modifier.InitEditors(txtPath.Text);
                modifier.SetVersionLabel(lblVersion);
                btnRestore.Enabled = modifier.BackupExists();
            }
            ga.RequestPageView($"{GetCheckedRadioButtonNameEn()}/{lblVersion.Text}/switch", "切换标签页");
        }

        private string GetCheckedRadioButtonNameEn()
        {
            if (rbtWechat.Checked)
            {
                return "wechat";
            }
            else if (rbtQQ.Checked)
            {
                return "qq";
            }
            else if (rbtTIM.Checked)
            {
                return "tim";
            }
            else if (rbtQQLite.Checked)
            {
                return "qqlite";
            }
            return "none";
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

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("本程序仅供学习交流,严禁用于商业用途。\n十六进制编辑器使用的修改数据集收集自网络。\n作者：huiyadanli", "关于本软件");
        }

        private void 主页ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/huiyadanli/RevokeMsgPatcher");
        }

        private void 支持版本ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/huiyadanli/RevokeMsgPatcher/wiki/%E7%89%88%E6%9C%AC%E6%94%AF%E6%8C%81");
        }

        private void 常见问题ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/huiyadanli/RevokeMsgPatcher/wiki#%E5%B8%B8%E8%A7%81%E9%97%AE%E9%A2%98");
        }

        private void 防撤回原理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/huiyadanli/RevokeMsgPatcher/wiki#%E5%8E%9F%E7%90%86");
        }

        private void 完整文档ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/huiyadanli/RevokeMsgPatcher/wiki");
        }

        private void 特征码防撤回强制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("作者正在考虑是否要加上这个功能", "强制使用特征码防撤回");
        }

        private void 手动输入补丁信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("作者正在考虑是否要加上这个功能，该功能可能有安全风险，暂时不加入", "手动输入补丁信息");
        }

        private void 通用微信多开工具ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(Application.StartupPath, "RevokeMsgPatcher.MultiInstance.exe");
            if (File.Exists(path))
            {
                Process p = new Process();
                p.StartInfo.FileName = path;
                p.Start();
            }
            else
            {
                DialogResult dr = MessageBox.Show($"未在同级目录下找到“微信通用多开工具”，位置：{path}，点击“确定”访问微信通用多开工具的主页，你可以在主页上下载到这个工具。", "未找到程序", MessageBoxButtons.OKCancel);
                if (dr == DialogResult.OK)
                {
                    Process.Start("https://github.com/huiyadanli/RevokeMsgPatcher/tree/master/RevokeMsgPatcher.MultiInstance");
                }
            }
        }
    }
}
