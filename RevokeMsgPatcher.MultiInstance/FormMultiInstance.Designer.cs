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
            this.txtPath = new System.Windows.Forms.TextBox();
            this.lblPathTag = new System.Windows.Forms.Label();
            this.btnChoosePath = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.startNum = new System.Windows.Forms.NumericUpDown();
            this.btnStart = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.startNum)).BeginInit();
            this.SuspendLayout();
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(86, 60);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(332, 21);
            this.txtPath.TabIndex = 7;
            // 
            // lblPathTag
            // 
            this.lblPathTag.AutoSize = true;
            this.lblPathTag.Location = new System.Drawing.Point(15, 63);
            this.lblPathTag.Name = "lblPathTag";
            this.lblPathTag.Size = new System.Drawing.Size(65, 12);
            this.lblPathTag.TabIndex = 6;
            this.lblPathTag.Text = "微信路径：";
            // 
            // btnChoosePath
            // 
            this.btnChoosePath.Location = new System.Drawing.Point(424, 59);
            this.btnChoosePath.Name = "btnChoosePath";
            this.btnChoosePath.Size = new System.Drawing.Size(36, 23);
            this.btnChoosePath.TabIndex = 8;
            this.btnChoosePath.Text = "...";
            this.btnChoosePath.UseVisualStyleBackColor = true;
            this.btnChoosePath.Click += new System.EventHandler(this.btnChoosePath_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(84, 151);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(293, 24);
            this.label1.TabIndex = 9;
            this.label1.Text = "只要我打开微信的速度足够快，我就多开无限个微信。\r\n我为什么能手动双开微信，别问，问就是手速快。";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 11;
            this.label2.Text = "多开个数：";
            // 
            // startNum
            // 
            this.startNum.Location = new System.Drawing.Point(86, 91);
            this.startNum.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.startNum.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.startNum.Name = "startNum";
            this.startNum.Size = new System.Drawing.Size(55, 21);
            this.startNum.TabIndex = 12;
            this.startNum.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(369, 91);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(91, 23);
            this.btnStart.TabIndex = 13;
            this.btnStart.Text = "点击启动！";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(284, 95);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(41, 12);
            this.linkLabel1.TabIndex = 15;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "GitHub";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(177, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 12);
            this.label3.TabIndex = 14;
            this.label3.Text = "软件主页(开源)：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(323, 36);
            this.label4.TabIndex = 16;
            this.label4.Text = "注意：\r\n1. 使用本程序多开前不能存在正在运行的微信进程。\r\n2. 多开的成功率取决于你的机器性能，不保证每次都能成功";
            // 
            // FormMultiInstance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 133);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.startNum);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.lblPathTag);
            this.Controls.Add(this.btnChoosePath);
            this.Name = "FormMultiInstance";
            this.Text = "微信多开小工具";
            ((System.ComponentModel.ISupportInitialize)(this.startNum)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Label lblPathTag;
        private System.Windows.Forms.Button btnChoosePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown startNum;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}

