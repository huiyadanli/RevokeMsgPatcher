namespace RevokeMsgPatcher.MultiInstance
{
    partial class FormMultiInstance
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnStartTimer = new System.Windows.Forms.Button();
            this.btnKillAll = new System.Windows.Forms.Button();
            this.mutexHandleCloseTimer = new System.Windows.Forms.Timer(this.components);
            this.btnCloseAllMutex = new System.Windows.Forms.Button();
            this.btnStopTimer = new System.Windows.Forms.Button();
            this.lblProcNum = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lblHowToUse = new System.Windows.Forms.LinkLabel();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStartTimer
            // 
            this.btnStartTimer.Location = new System.Drawing.Point(170, 46);
            this.btnStartTimer.Name = "btnStartTimer";
            this.btnStartTimer.Size = new System.Drawing.Size(91, 23);
            this.btnStartTimer.TabIndex = 99;
            this.btnStartTimer.Text = "启动多开";
            this.btnStartTimer.UseVisualStyleBackColor = true;
            this.btnStartTimer.Click += new System.EventHandler(this.btnStartTimer_Click);
            // 
            // btnKillAll
            // 
            this.btnKillAll.Location = new System.Drawing.Point(69, 41);
            this.btnKillAll.Name = "btnKillAll";
            this.btnKillAll.Size = new System.Drawing.Size(118, 23);
            this.btnKillAll.TabIndex = 17;
            this.btnKillAll.Text = "关闭所有微信进程";
            this.btnKillAll.UseVisualStyleBackColor = true;
            this.btnKillAll.Click += new System.EventHandler(this.btnKillAll_Click);
            // 
            // mutexHandleCloseTimer
            // 
            this.mutexHandleCloseTimer.Interval = 500;
            this.mutexHandleCloseTimer.Tick += new System.EventHandler(this.mutexHandleCloseTimer_Tick);
            // 
            // btnCloseAllMutex
            // 
            this.btnCloseAllMutex.Location = new System.Drawing.Point(53, 80);
            this.btnCloseAllMutex.Name = "btnCloseAllMutex";
            this.btnCloseAllMutex.Size = new System.Drawing.Size(152, 23);
            this.btnCloseAllMutex.TabIndex = 18;
            this.btnCloseAllMutex.Text = "清理所有微信的互斥句柄";
            this.btnCloseAllMutex.UseVisualStyleBackColor = true;
            this.btnCloseAllMutex.Click += new System.EventHandler(this.btnCloseAllMutex_Click);
            // 
            // btnStopTimer
            // 
            this.btnStopTimer.Enabled = false;
            this.btnStopTimer.Location = new System.Drawing.Point(170, 75);
            this.btnStopTimer.Name = "btnStopTimer";
            this.btnStopTimer.Size = new System.Drawing.Size(91, 23);
            this.btnStopTimer.TabIndex = 98;
            this.btnStopTimer.Text = "停止";
            this.btnStopTimer.UseVisualStyleBackColor = true;
            this.btnStopTimer.Click += new System.EventHandler(this.btnStopTimer_Click);
            // 
            // lblProcNum
            // 
            this.lblProcNum.AutoSize = true;
            this.lblProcNum.Location = new System.Drawing.Point(170, 23);
            this.lblProcNum.Name = "lblProcNum";
            this.lblProcNum.Size = new System.Drawing.Size(95, 12);
            this.lblProcNum.TabIndex = 20;
            this.lblProcNum.Text = "当前微信数量：0";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(286, 146);
            this.tabControl1.TabIndex = 21;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.textBox1);
            this.tabPage1.Controls.Add(this.btnStopTimer);
            this.tabPage1.Controls.Add(this.lblProcNum);
            this.tabPage1.Controls.Add(this.btnStartTimer);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(278, 120);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "自动模式";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBox1.Location = new System.Drawing.Point(7, 14);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(157, 92);
            this.textBox1.TabIndex = 22;
            this.textBox1.Text = "1.使用方法：点击【启动多开】之后，就可以启动多个微信了。\r\n2.注意：启动多个微信频率太快时，可能会失败。";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lblHowToUse);
            this.tabPage2.Controls.Add(this.btnCloseAllMutex);
            this.tabPage2.Controls.Add(this.btnKillAll);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(263, 120);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "手动功能";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // lblHowToUse
            // 
            this.lblHowToUse.AutoSize = true;
            this.lblHowToUse.Location = new System.Drawing.Point(96, 16);
            this.lblHowToUse.Name = "lblHowToUse";
            this.lblHowToUse.Size = new System.Drawing.Size(65, 12);
            this.lblHowToUse.TabIndex = 19;
            this.lblHowToUse.TabStop = true;
            this.lblHowToUse.Text = "如何使用？";
            this.lblHowToUse.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblHowToUse_LinkClicked);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Controls.Add(this.linkLabel1);
            this.tabPage3.Controls.Add(this.label3);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(263, 120);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "关于";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(23, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(221, 61);
            this.label1.TabIndex = 17;
            this.label1.Text = "本工具是\r\n【 PC版微信/QQ/TIM防撤回补丁】\r\n的额外产物\r\n更多信息可以在软件主页查看";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(160, 88);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(41, 12);
            this.linkLabel1.TabIndex = 16;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "GitHub";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(53, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 12);
            this.label3.TabIndex = 15;
            this.label3.Text = "软件主页(开源)：";
            // 
            // FormMultiInstance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(286, 146);
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.Name = "FormMultiInstance";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "微信多开小工具";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMultiInstance_FormClosed);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnStartTimer;
        private System.Windows.Forms.Button btnKillAll;
        private System.Windows.Forms.Timer mutexHandleCloseTimer;
        private System.Windows.Forms.Button btnCloseAllMutex;
        private System.Windows.Forms.Button btnStopTimer;
        private System.Windows.Forms.Label lblProcNum;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel lblHowToUse;
    }
}

