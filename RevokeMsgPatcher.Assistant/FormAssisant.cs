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

            txtInfo.AppendText("生成完毕！位置:" + path);
        }
    }
}
