using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RevokeMsgPatcher.Model;

namespace RevokeMsgPatcher.Forms
{
    public partial class FormLiteLoaderQQNT : Form
    {
        List<LiteLoaderRowData> data = new List<LiteLoaderRowData>();

        public FormLiteLoaderQQNT()
        {
            InitializeComponent();
            InitializeDataGridView();
        }


        private void InitializeDataGridView()
        {
            dataGridView1.RowHeadersVisible = false;
            // 设置 DataGridView 的列
            dataGridView1.Columns.Add(new DataGridViewLinkColumn { Name = "NameColumn", HeaderText = "名称", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            dataGridView1.Columns.Add(new DataGridViewLinkColumn { Name = "AuthorColumn", HeaderText = "作者", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            dataGridView1.Columns.Add(new DataGridViewButtonColumn { Name = "UpdateButtonColumn", HeaderText = "更新", Text = "更新", UseColumnTextForButtonValue = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "StatusColumn", HeaderText = "状态", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });

            // 初始化数据并添加行
            data = InitData();
            foreach (var item in data)
            {
                AddRow(item);
            }


            // 处理单元格点击事件
            dataGridView1.CellClick += DataGridView1_CellClick;
        }

        private List<LiteLoaderRowData> InitData()
        {
            var data = new List<LiteLoaderRowData>
            {
                new LiteLoaderRowData
                {
                    Name = "本体 LiteLoaderQQNT",
                    NameLink = "https://github.com/LiteLoaderQQNT/LiteLoaderQQNT",
                    Author = "mo-jinran",
                    AuthorLink = "https://github.com/mo-jinran",
                    Status = "未检查",
                    MainBranchName = "main",
                    LocalPath = Path.Combine(Application.StartupPath, "LiteLoaderQQNT"),
                    DownloadUrl = "https://github.com/LiteLoaderQQNT/LiteLoaderQQNT/releases/download/#{version}/LiteLoaderQQNT.zip"
                },
                new LiteLoaderRowData
                {
                    Name = "补丁 DLLHijackMethod",
                    NameLink = "https://github.com/LiteLoaderQQNT/QQNTFileVerifyPatch/tree/DLLHijackMethod",
                    Author = "sysrom",
                    AuthorLink = "https://github.com/sysrom",
                    LocalPath = Path.Combine(Application.StartupPath, "Public"),
                    Status = "无需更新"
                },
                new LiteLoaderRowData
                {
                    Name = "列表插件 LL-plugin-list-viewer",
                    NameLink = "https://github.com/ltxhhz/LL-plugin-list-viewer",
                    Author = "ltxhhz",
                    AuthorLink = "https://github.com/ltxhhz",
                    Status = "未检查",
                    MainBranchName = "main",
                    LocalPath = Path.Combine(Application.StartupPath, "LiteLoaderQQNT/plugins/list-viewer"),
                    DownloadUrl = "https://github.com/ltxhhz/LL-plugin-list-viewer/releases/download/v#{version}/list-viewer.zip"
                },
                new LiteLoaderRowData
                {
                    Name = "防撤回插件 LiteLoaderQQNT-Anti-Recall",
                    NameLink = "https://github.com/xh321/LiteLoaderQQNT-Anti-Recall",
                    Author = "xh321",
                    AuthorLink = "https://github.com/xh321",
                    Status = "未检查",
                    MainBranchName = "master",
                    LocalPath = Path.Combine(Application.StartupPath, "LiteLoaderQQNT/plugins/qq-anti-recall"),
                    DownloadUrl = "https://github.com/xh321/LiteLoaderQQNT-Anti-Recall/releases/download/#{version}/qq-anti-recall.zip"
                }
            };

            return data;
        }

        private void AddRow(LiteLoaderRowData rowData)
        {
            int rowIndex = dataGridView1.Rows.Add();
            DataGridViewRow row = dataGridView1.Rows[rowIndex];
            rowData.Row = row;

            // 设置名称列
            DataGridViewLinkCell nameCell = (DataGridViewLinkCell)row.Cells["NameColumn"];
            nameCell.Value = rowData.Name;
            nameCell.Tag = rowData.NameLink;

            // 设置作者列
            DataGridViewLinkCell authorCell = (DataGridViewLinkCell)row.Cells["AuthorColumn"];
            authorCell.Value = rowData.Author;
            authorCell.Tag = rowData.AuthorLink;

            // 设置状态列
            row.Cells["StatusColumn"].Value = rowData.Status;

            // 订阅状态更新事件
            rowData.StatusUpdated += (newStatus) =>
            {
                if (dataGridView1.InvokeRequired)
                {
                    dataGridView1.Invoke(new Action(() => row.Cells["StatusColumn"].Value = newStatus));
                }
                else
                {
                    row.Cells["StatusColumn"].Value = newStatus;
                }
            };

            rowData.GetLocalVersionAndUpdateStatus();
        }


        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == dataGridView1.Columns["UpdateButtonColumn"].Index)
                {
                    if (data[e.RowIndex].NameLink.Contains("QQNTFileVerifyPatch"))
                    {
                        MessageBox.Show("此项无需更新！");
                        return;
                    }

                    data[e.RowIndex].Row.Cells["StatusColumn"].Value = "正在更新";
                    Task.Run(() => data[e.RowIndex].CheckAndUpdate());
                }
                else if (e.ColumnIndex == dataGridView1.Columns["NameColumn"].Index || e.ColumnIndex == dataGridView1.Columns["AuthorColumn"].Index)
                {
                    string url = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag.ToString();
                    System.Diagnostics.Process.Start(url);
                }
            }
        }

        private void btnCheckUpdateAll_Click(object sender, EventArgs e)
        {
            foreach (var item in data)
            {
                Task.Run(() => item.CheckAndUpdate());
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {

        }

        private void btnPath_Click(object sender, EventArgs e)
        {

        }
    }
}