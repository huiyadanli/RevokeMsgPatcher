using RevokeMsgPatcher.Forms;
using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Modifier;
using RevokeMsgPatcher.Utils;
using System;
using System.Collections.Generic;
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
        private string getPatchJsonStatus = "GETTING";  // GETTING FAIL SUCCESS

        private readonly GAHelper ga = GAHelper.Instance; // Google Analytics 记录

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
            // 显示是否能够备份还原、版本和功能
            InitEditorsAndUI(txtPath.Text);
        }

        private void InitEditorsAndUI(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                EnableAllButton(false);

                // 清空界面元素
                lblVersion.Text = "";
                panelCategories.Controls.Clear();

                // 重新计算并修改界面元素
                modifier.InitEditors(path);
                modifier.SetVersionLabelAndCategoryCategories(lblVersion, panelCategories);

                EnableAllButton(true);

                // 重新显示备份状态
                btnRestore.Enabled = false;
                btnRestore.Enabled = modifier.BackupExists();

                List<string> categories = UIController.GetCategoriesFromPanel(panelCategories);
                if (categories != null && categories.Count == 0)
                {
                    btnPatch.Enabled = false;
                }
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

            //if (getPatchJsonStatus != "SUCCESS")
            //{
            //    if (MessageBox.Show("当前程序未获取到最新补丁信息(或者正在获取中，如果成功请无视本提示)，可能会出现补丁安装失败的情况，你可以通过以下方法重试:" + Environment.NewLine
            //        + "1. 重新启动本程序，重新获取最新补丁信息" + Environment.NewLine
            //        + "2. 如果每次都是[获取最新补丁信息失败]，请检查自身网络是否有问题，或者等一段时间后重试" + Environment.NewLine
            //        + "点击 \"确定\" 继续安装补丁。",
            //        "提示", MessageBoxButtons.OKCancel) != DialogResult.OK)
            //    {
            //        return;
            //    }
            //}

            EnableAllButton(false);
            // a.重新初始化编辑器
            modifier.InitEditors(txtPath.Text);
            // b.获取选择的功能 （精准匹配返回null） // TODO 此处逻辑可以优化 不可完全信任UI信息
            List<string> categories = UIController.GetCategoriesFromPanel(panelCategories);
            if (categories != null && categories.Count == 0)
            {
                MessageBox.Show("请至少选择一项功能");
                EnableAllButton(true);
                btnRestore.Enabled = modifier.BackupExists();
                return;
            }
            // c.计算SHA1，验证文件完整性，寻找对应的补丁信息（精确版本、通用特征码两种补丁信息）
            try
            {
                modifier.ValidateAndFindModifyInfo(categories);
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

            // d.打补丁
            try
            {
                modifier.Patch();
                ga.RequestPageView($"{enName}/{version}/patch/succ", "补丁安装成功");
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
                InitEditorsAndUI(txtPath.Text);
            }

        }

        private void txtPath_TextChanged(object sender, EventArgs e)
        {
            if (modifier.IsAllFilesExist(txtPath.Text))
            {
                InitEditorsAndUI(txtPath.Text);
            }
            else
            {
                UIController.AddMsgToPanel(panelCategories, "请输入正确的应用路径");
                lblVersion.Text = "";
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
                    // 显示是否能够备份还原、版本和功能
                    InitEditorsAndUI(txtPath.Text);
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
            // 重新计算显示是否能够备份还原、版本和功能
            InitEditorsAndUI(txtPath.Text);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/huiyadanli/RevokeMsgPatcher");
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            // 异步获取最新的补丁信息
            string json = await HttpUtil.GetPatchJsonAsync();
            //string json = null; // local test
            if (string.IsNullOrEmpty(json))
            {
                lblUpdatePachJson.Text = "[ 获取最新补丁信息失败 ]";
                ga.RequestPageView($"/main/json/fail", $"获取最新补丁信息失败");
                getPatchJsonStatus = "FAIL";
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
                    getPatchJsonStatus = "SUCCESS";
                    InitControls();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    lblUpdatePachJson.Text = "[ 更换新配置时异常 ]";
                    ga.RequestPageView($"/main/json/exception", $"更换新配置时异常");
                    getPatchJsonStatus = "FAIL";
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

            // 重新计算显示是否能够备份还原、版本和功能
            InitEditorsAndUI(txtPath.Text);
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
