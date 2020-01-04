using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace RevokeMsgPatcher.MultiInstance
{
    public partial class FormMultiInstance : Form
    {
        public FormMultiInstance()
        {
            InitializeComponent();

            // 标题加上版本号
            string currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (currentVersion.Length > 3)
            {
                currentVersion = " v" + currentVersion.Substring(0, 3);
            }
            this.Text += currentVersion;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/huiyadanli/RevokeMsgPatcher");
        }

        private void btnStartTimer_Click(object sender, EventArgs e)
        {
            mutexHandleCloseTimer.Start();
            btnStartTimer.Enabled = false;
            btnStopTimer.Enabled = true;
        }

        private void btnStopTimer_Click(object sender, EventArgs e)
        {
            mutexHandleCloseTimer.Stop();
            btnStartTimer.Enabled = true;
            btnStopTimer.Enabled = false;
        }

        private List<WechatProcess> wechatProcesses = new List<WechatProcess>();

        private void mutexHandleCloseTimer_Tick(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("WeChat");
            Console.WriteLine("WeChat进程数：" + processes.Length);
            // 添加新进程
            foreach (Process p in processes)
            {
                int i = 0;
                for (i = 0; i < wechatProcesses.Count; i++)
                {
                    WechatProcess wechatProcess = wechatProcesses[i];
                    if (wechatProcess.Proc.Id == p.Id)
                    {
                        break;
                    }
                }
                if (i == wechatProcesses.Count)
                {
                    wechatProcesses.Add(new WechatProcess(p));
                }
            }
            // 关闭所有存在互斥句柄的进程
            int num = 0;
            for (int i = wechatProcesses.Count - 1; i >= 0; i--)
            {
                WechatProcess wechatProcess = wechatProcesses[i];
                if (!wechatProcess.MutexClosed)
                {
                    wechatProcess.MutexClosed = ProcessUtil.CloseMutexHandle(wechatProcess.Proc);
                    Console.WriteLine("进程：" + wechatProcess.Proc.Id + ",关闭互斥句柄：" + wechatProcess.MutexClosed);
                }
                else
                {
                    if (wechatProcess.Proc.HasExited)
                    {
                        // 移除不存在的线程
                        wechatProcesses.RemoveAt(i);
                    }
                    else
                    {
                        num++;
                    }

                }
            }
            lblProcNum.Text = "当前微信数量：" + num.ToString();
        }

        private void btnKillAll_Click(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("WeChat");
            if (processes.Length > 0)
            {
                foreach (Process p in processes)
                {
                    p.Kill();
                }
                MessageBox.Show("已经关闭所有微信进程，共" + processes.Length + "个", "提示");
            }
            else
            {
                MessageBox.Show("当前无微信进程", "提示");
            }
        }

        private void btnCloseAllMutex_Click(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("WeChat");
            ProcessUtil.CloseMutexHandle(processes);
        }

        private void lblHowToUse_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/huiyadanli/RevokeMsgPatcher/tree/master/RevokeMsgPatcher.MultiInstance");
        }

        private void FormMultiInstance_FormClosed(object sender, FormClosedEventArgs e)
        {
            mutexHandleCloseTimer.Stop();
        }
    }
}
