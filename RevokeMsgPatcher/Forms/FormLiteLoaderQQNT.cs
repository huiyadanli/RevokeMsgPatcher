using Newtonsoft.Json;
using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevokeMsgPatcher.Forms
{
    public partial class FormLiteLoaderQQNT : Form
    {
        List<LiteLoaderRowData> data = new List<LiteLoaderRowData>();

        public FormLiteLoaderQQNT()
        {
            InitializeComponent();
            InitializeDataGridView();
            txtQQNTPath.Text = FindInstallPath();

            InitCboProxyList();
        }

        private void InitCboProxyList()
        {
            // 添加代理 URL 到下拉菜单
            foreach (var proxy in ProxySpeedTester.ProxyUrls)
            {
                cboGithubProxy.Items.Add(proxy.Replace("{0}",""));
            }

            // 异步测试代理速度并设置默认选项
            Task.Run(async () =>
            {
                var fastestProxy = await ProxySpeedTester.GetFastestProxyAsync(ProxySpeedTester.TargetUrl);
                Debug.WriteLine(fastestProxy.Item1);
                if (!string.IsNullOrEmpty(fastestProxy.Item1))
                {
                    cboGithubProxy.Invoke(new Action(() => cboGithubProxy.SelectedItem = fastestProxy.Item1));
                }
            });
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
                    var proxyUrl = cboGithubProxy.Text;
                    Task.Run(() => data[e.RowIndex].CheckAndUpdate(proxyUrl));
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
            var proxyUrl = cboGithubProxy.Text;
            foreach (var item in data)
            {
                Task.Run(() => item.CheckAndUpdate(proxyUrl));
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            string installPath = txtQQNTPath.Text;
            if (!IsAllFilesExist(installPath))
            {
                MessageBox.Show("请选择正确的QQNT安装路径!");
                return;
            }

            try
            {
                string appPath = GetAppPath(installPath);
                RestoreDll(installPath);
                RestorePackageJson(appPath);
                MessageBox.Show("LiteLoaderQQNT 还原成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $@"
                     还原时出现异常：{ex.Source}
                     --
                     {ex.StackTrace}
                     --
                     {ex.Message}
                     "
                );
            }
        }

        private void RestoreDll(string installPath)
        {
            string destPath = Path.Combine(installPath, "dbghelp.dll");
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
        }

        private void RestorePackageJson(string appPath)
        {
            string packageJsonPath = Path.Combine(appPath, "package.json");
            string backupPath = Path.Combine(appPath, "package.json.h.bak");
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, packageJsonPath, true);
            }
            else
            {
                throw new Exception($"在路径{appPath}下，未找到package.json.h.bak备份文件，请确认是否通过本软件安装过 LiteLoaderQQNT");
            }
        }

        /// <summary>
        /// 打补丁
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPath_Click(object sender, EventArgs e)
        {
            string installPath = txtQQNTPath.Text;
            if (!IsAllFilesExist(installPath))
            {
                MessageBox.Show("请选择正确的QQNT安装路径!");
                return;
            }

            try
            {
                string appPath = GetAppPath(installPath);
                MoveDll(installPath);
                CreateLauncherFile(appPath);
                ModifyPackageJson(appPath);
                MessageBox.Show("LiteLoaderQQNT 安装成功！");
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    $@"
请以管理员权限启动本程序，并确认 QQNT 处于关闭状态。
                     安装 LiteLoaderQQNT 时出现异常：{ex.Source}
                     --
                     {ex.StackTrace}
                     --
                     {ex.Message}
                     "
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $@"
                     安装 LiteLoaderQQNT 时出现异常：{ex.Source}
                     --
                     {ex.StackTrace}
                     --
                     {ex.Message}
                     "
                );
            }
        }


        private void MoveDll(string installPath)
        {
            string fileName = "dbghelp.dll";
            string destPath = Path.Combine(installPath, fileName);
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }

            File.Copy(Path.Combine(Application.StartupPath, "Public", fileName), destPath);
        }

        /// <summary>
        /// 查看 QQNT 根目录(txtQQNTPath.Text)，是否存在 versions 文件夹 是，则路径为 QQNT\versions\版本号\resources\app 否，则路径为 QQNT\resources\app 
        /// </summary>
        /// <param name="installPath"></param>
        /// <returns></returns>
        private string GetAppPath(string installPath)
        {
            string versionsPath = Path.Combine(installPath, "versions");
            if (Directory.Exists(versionsPath))
            {
                var versionDirectories = Directory.GetDirectories(versionsPath);
                if (versionDirectories.Length > 0)
                {
                    // 选择最新的版本
                    string latestVersion = versionDirectories
                        .Select(Path.GetFileName)
                        .OrderByDescending(v => new
                        {
                            MainVersion = new Version(v.Split('-')[0]),
                            SubVersion = int.Parse(v.Split('-')[1])
                        })
                        .FirstOrDefault();

                    if (latestVersion != null)
                    {
                        return Path.Combine(versionsPath, latestVersion, "resources", "app");
                    }
                }
            }

            return Path.Combine(installPath, "resources", "app");
        }

        /// <summary>
        /// 创建 app/app_launcher/liteloader.h.js 文件，写入 require(String.raw*) 其中 * 为 LiteLoaderQQNT 的路径
        /// </summary>
        /// <param name="appPath"></param>
        /// <exception cref="Exception"></exception>
        private void CreateLauncherFile(string appPath)
        {
            string launcherDir = Path.Combine(appPath, "app_launcher");
            if (!Directory.Exists(launcherDir))
            {
                throw new Exception($"在路径{appPath}下，未找到app_launcher文件夹");
            }

            string launcherFilePath = Path.Combine(launcherDir, "liteloader.h.js");
            // if (File.Exists(launcherFilePath))
            // {
            //     Debug.WriteLine("已经创建过liteloader.h.js文件");
            //     return;
            // }

            string liteLoaderPath = Path.Combine(Application.StartupPath, "LiteLoaderQQNT");
            string content = $"require(String.raw`{liteLoaderPath}`);";

            File.WriteAllText(launcherFilePath, content, Encoding.UTF8);
        }

        /// <summary>
        /// 修改 app/package.json 文件，将 main 后面的路径改为 ./app_launcher/liteloader.h.js
        /// </summary>
        /// <param name="appPath"></param>
        /// <exception cref="Exception"></exception>
        private void ModifyPackageJson(string appPath)
        {
            string packageJsonPath = Path.Combine(appPath, "package.json");
            if (File.Exists(packageJsonPath))
            {
                string json = File.ReadAllText(packageJsonPath);
                dynamic jsonObj = JsonConvert.DeserializeObject(json);
                if (jsonObj.main != null)
                {
                    var s = (string)jsonObj.main;
                    if (s.Contains("liteloader.h.js"))
                    {
                        Debug.WriteLine("已经修改过package.json文件");
                        return;
                    }
                }

                // 备份
                File.Copy(packageJsonPath, Path.Combine(appPath, "package.json.h.bak"), true);

                // 修改
                jsonObj.main = "./app_launcher/liteloader.h.js";
                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(packageJsonPath, output);
            }
            else
            {
                throw new Exception($"在路径{appPath}下，未找到package.json文件");
            }
        }

        private void btnChoose_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择安装路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath) || !IsAllFilesExist(dialog.SelectedPath))
                {
                    MessageBox.Show("无法找到此应用的关键文件，请选择正确的安装路径!");
                }
                else
                {
                    txtQQNTPath.Text = dialog.SelectedPath;
                }
            }
        }

        /// <summary>
        /// 自动寻找获取QQNT安装路径
        /// </summary>
        /// <returns></returns>
        public string FindInstallPath()
        {
            try
            {
                string installPath = PathUtil.FindInstallPathFromRegistryWOW6432Node("QQ");
                if (!string.IsNullOrEmpty(installPath))
                {
                    installPath = Path.GetDirectoryName(installPath);
                    if (IsAllFilesExist(installPath))
                    {
                        return installPath;
                    }
                }

                installPath = PathUtil.FindInstallPathFromRegistry("QQNT");
                if (!IsAllFilesExist(installPath))
                {
                    List<string> defaultPathList = PathUtil.GetDefaultInstallPaths(@"Tencent\QQNT");
                    foreach (string defaultPath in defaultPathList)
                    {
                        if (IsAllFilesExist(defaultPath))
                        {
                            return defaultPath;
                        }
                    }
                }
                else
                {
                    return installPath;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        private bool IsAllFilesExist(string installPath)
        {
            return File.Exists(Path.Combine(installPath, "QQ.exe"));
        }
    }
}