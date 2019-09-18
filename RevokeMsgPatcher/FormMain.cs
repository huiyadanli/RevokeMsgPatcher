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
        Patcher patcher = null;

        public FormMain()
        {
            InitializeComponent();
            patcher = new Patcher();
            txtPath.Text = Util.AutoFindInstallPath();
            if (!string.IsNullOrEmpty(txtPath.Text))
            {
                patcher.IntallPath = txtPath.Text;
                btnRestore.Enabled = File.Exists(patcher.BakPath);
            }
            // 标题加上版本号
            string currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (currentVersion.Length > 3)
            {
                currentVersion = " v" + currentVersion.Substring(0, 3);
            }
            this.Text += currentVersion;
        }

        private void btnPatch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPath.Text) || !Util.IsWechatInstallPath(txtPath.Text))
            {
                MessageBox.Show("请选择微信安装路径!");
                return;
            }
            patcher.IntallPath = txtPath.Text;

            try
            {
                btnPatch.Enabled = false;
                string version = patcher.JudgeVersion();
                if (!string.IsNullOrEmpty(version))
                {
                    if (version == "done")
                    {
                        MessageBox.Show("已经安装过防撤回补丁了");
                        btnPatch.Enabled = true;
                        return;
                    }

                    if (patcher.Patch())
                    {
                        MessageBox.Show("成功安装防撤回补丁！原 WeChatWin.dll 文件已经备份到 " + patcher.BakPath + " ，如果有问题可以手动覆盖恢复");
                    }
                    else
                    {
                        MessageBox.Show("防撤回补丁安装失败！");
                    }
                    btnRestore.Enabled = File.Exists(patcher.BakPath);
                }
                else
                {
                    MessageBox.Show("当前微信版本不被支持：" + Util.GetFileVersion(patcher.DllPath));
                }
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
                patcher.IntallPath = txtPath.Text;
                btnRestore.Enabled = File.Exists(patcher.BakPath);
            }
        }

        private void btnChoosePath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择微信安装路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath) || !Util.IsWechatInstallPath(dialog.SelectedPath))
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
                if (File.Exists(patcher.BakPath))
                {
                    File.Copy(patcher.BakPath, patcher.DllPath, true);
                    MessageBox.Show("还原成功");
                }
                else
                {
                    MessageBox.Show("备份文件不存在");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }
            btnRestore.Enabled = File.Exists(patcher.BakPath);
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
                patcher.SetNewPatchJson(json);
                lblUpdatePachJson.Text = "获取成功";
            }

        }

        private void lblUpdatePachJson_Click(object sender, EventArgs e)
        {
            string versions = "";
            patcher.TargetFiles.ForEach(t =>
            {
                versions += t.Version + Environment.NewLine;
            });
            MessageBox.Show("当前所支持的微信版本:" + Environment.NewLine + versions);
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            PathUtil.DisplayAllProgram();
        }
    }
}
