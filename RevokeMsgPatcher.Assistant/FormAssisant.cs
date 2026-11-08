using RevokeMsgPatcher.Matcher;
using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace RevokeMsgPatcher.Assistant
{
    public partial class FormAssisant : Form
    {
        public FormAssisant()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            JsonData obj = new JsonData();
            string json = obj.BagJson();
            Console.WriteLine(json);

            DirectoryInfo directory = new DirectoryInfo("../../Data/" + obj.Bag().LatestVersion);
            if (!directory.Exists)
            {
                directory.Create();
            }
            string path = Path.Combine(directory.FullName, "patch.json");
            File.WriteAllText(path, json);

            txtInfo.AppendText("生成完毕！位置:" + path + Environment.NewLine);

            try
            {
                string resourcePath = FindPatchResourcePath(AppDomain.CurrentDomain.BaseDirectory)
                    ?? FindPatchResourcePath(Environment.CurrentDirectory);
                if (resourcePath == null)
                {
                    txtInfo.AppendText("未找到 RevokeMsgPatcher 项目，跳过 Resources.PatchJson 同步。" + Environment.NewLine);
                    return;
                }

                UpdatePatchResource(resourcePath, json);
                txtInfo.AppendText("Resources.PatchJson 同步完毕！位置:" + resourcePath + Environment.NewLine);
            }
            catch (Exception ex)
            {
                txtInfo.AppendText("Resources.PatchJson 同步失败:" + ex.Message + Environment.NewLine);
            }
        }

        private static string FindPatchResourcePath(string startDirectory)
        {
            // 从启动位置逐级向上查找，兼容 bin/Debug、bin/Release 和解决方案目录。
            for (DirectoryInfo directory = new DirectoryInfo(startDirectory); directory != null; directory = directory.Parent)
            {
                string projectDirectory = Path.Combine(directory.FullName, "RevokeMsgPatcher");
                string resourcePath = Path.Combine(projectDirectory, "Properties", "Resources.resx");
                if (File.Exists(Path.Combine(projectDirectory, "RevokeMsgPatcher.csproj")) && File.Exists(resourcePath))
                {
                    return resourcePath;
                }
            }

            return null;
        }

        private static void UpdatePatchResource(string resourcePath, string json)
        {
            // 仅更新 PatchJson 的值，保留资源文件中的其他资源、注释和空白。
            XmlDocument document = new XmlDocument { PreserveWhitespace = true };
            document.Load(resourcePath);
            XmlNode value = document.SelectSingleNode("/root/data[@name='PatchJson']/value");
            if (value == null)
            {
                throw new InvalidOperationException("Resources.resx 中未找到 PatchJson 资源项。");
            }

            value.InnerText = json;
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false),
                Indent = false
            };
            using (XmlWriter writer = XmlWriter.Create(resourcePath, settings))
            {
                document.Save(writer);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            byte[] fileByteArray = File.ReadAllBytes(@"");
            byte[] searchBytes = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 E8 8D 55 EC 52 89 5D EC 68 3F 3F 3F 54 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C");
            byte[] replaceBytes = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 E8 8D 55 EC 52 89 5D EC EB 09 90 90 90 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C");
            //int[] indexs = FuzzyMatcher.MatchAll(fileByteArray, searchBytes);
            int[] indexs = FuzzyMatcher.MatchNotReplaced(fileByteArray, searchBytes, replaceBytes);
            txtInfo.AppendText("查找结果位置:" + string.Join(",", indexs) + Environment.NewLine);
            // 371130

            List<Change> changes = ComputChanges(indexs, searchBytes, replaceBytes);
            foreach (Change c in changes)
            {
                txtInfo.AppendText("替换位置:" + Convert.ToString(c.Position, 16) + " 替换内容:" + ByteUtil.ByteArrayToHexString(c.Content) + Environment.NewLine);
            }

        }

        public static List<Change> ComputChanges(int[] indexs, byte[] searchBytes, byte[] replaceBytes)
        {
            if (searchBytes.Length != replaceBytes.Length)
            {
                throw new Exception("查询串与替换串长度不同!");
            }
            // 一个替换串存在多个替换点的情况
            List<Change> changeOffsets = new List<Change>(); // 查询串与替换串变化偏移
            List<byte> diff = null;
            for (int i = 0; i < searchBytes.Length; i++)
            {
                if (searchBytes[i] != replaceBytes[i])
                {
                    if (diff == null)
                    {
                        diff = new List<byte>();
                        Change offset = new Change
                        {
                            Position = i
                        };
                        changeOffsets.Add(offset);
                    }
                    diff.Add(replaceBytes[i]);
                }
                else
                {
                    if (diff != null)
                    {
                        changeOffsets.Last().Content = diff.ToArray();
                        diff = null;
                    }
                }
            }
            // 最后一位也是要被替换的情况
            if (diff != null)
            {
                changeOffsets.Last().Content = diff.ToArray();
                diff = null;
            }

            if (changeOffsets.Count == 0)
            {
                throw new Exception("查询串与替换串完全相同！请确认补丁信息的正确性。");
            }

            List<Change> changes = new List<Change>();
            foreach (int index in indexs)
            {
                foreach (Change offset in changeOffsets)
                {
                    Change c = offset.Clone();
                    c.Position += index;
                    changes.Add(c);
                }
            }
            return changes;
        }

        private void btnGetVersion_Click(object sender, EventArgs e)
        {
            string version = FileUtil.GetFileVersion(@"");
            txtInfo.AppendText("文件版本:" + version + Environment.NewLine);
        }
    }
}
