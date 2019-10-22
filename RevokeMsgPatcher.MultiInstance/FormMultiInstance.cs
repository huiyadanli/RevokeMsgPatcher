using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevokeMsgPatcher.MultiInstance
{
    public partial class FormMultiInstance : Form
    {
        public FormMultiInstance()
        {
            InitializeComponent();
            string installFolder = FindInstallPathFromRegistry("Wechat");
            if (!string.IsNullOrEmpty(installFolder))
            {
                string wechatPath = Path.Combine(installFolder, "WeChat.exe");
                if (File.Exists(wechatPath))
                {
                    txtPath.Text = wechatPath;
                }
            }
        }

        private void btnChoosePath_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "请选择微信启动主程序",
                Filter = "微信主程序|WeChat.exe"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = dialog.FileName;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (File.Exists(txtPath.Text))
            {
                // 检测微信进程是否存在
                Process[] ps = Process.GetProcessesByName("WeChat");
                if (ps.Length > 0)
                {
                    DialogResult result = MessageBox.Show("当前存在运行中的微信进程，请先关闭当前微信才能使用该功能。点击【确定】强制关闭当前所有微信进程并进行多开，点击【取消】不做任何处理。", "当前存在运行中的微信进程", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (result == DialogResult.OK)
                    {
                        foreach (Process p in ps)
                            p.Kill();
                    }
                    else
                    {
                        return;
                    }
                }
                // 启动多个实例
                for (int i = 0; i < startNum.Value; i++)
                {
                    Process.Start(txtPath.Text);
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/huiyadanli/RevokeMsgPatcher");
        }

        /// <summary>
        /// 从注册表中寻找安装路径
        /// </summary>
        /// <param name="uninstallKeyName">
        /// 安装信息的注册表键名
        /// 微信：WeChat
        /// QQ：{052CFB79-9D62-42E3-8A15-DE66C2C97C3E} 
        /// TIM：TIM
        /// </param>
        /// <returns>安装路径</returns>
        public static string FindInstallPathFromRegistry(string uninstallKeyName)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{uninstallKeyName}");
                if (key == null)
                {
                    return null;
                }
                object installLocation = key.GetValue("InstallLocation");
                key.Close();
                if (installLocation != null && !string.IsNullOrEmpty(installLocation.ToString()))
                {
                    return installLocation.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }
    }
}
