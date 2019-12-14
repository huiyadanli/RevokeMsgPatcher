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
                Process[] processes = Process.GetProcessesByName("WeChat");
                ProcessUtil.CloseMutexHandle(processes);
                // 启动多个实例
                for (int i = 0; i < startNum.Value; i++)
                {
                    //var t = new Task(() =>
                    //{
                    //    Process newInstance = Process.Start(txtPath.Text);
                    //    newInstance.WaitForInputIdle();
                    //    ProcessUtil.CloseMutexHandle(newInstance);
                    //});
                    //t.Start();
                    Process newInstance = Process.Start(txtPath.Text);
                    //newInstance.WaitForInputIdle();
                    //ProcessUtil.CloseMutexHandle(newInstance);
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

        private void button1_Click(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("WeChat");
            ProcessUtil.CloseMutexHandle(processes);
        }

        private void mutexHandleCloseTimer_Tick(object sender, EventArgs e)
        {

        }
    }
}
