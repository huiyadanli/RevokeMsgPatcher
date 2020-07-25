using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevokeMsgPatcher.Forms
{
    public class UIController
    {
        public static void AddCategoryCheckBoxToPanel(Panel panel, string[] categories, string[] installed)
        {
            if (categories != null && categories.Length != 0)
            {
                panel.Controls.Clear();
                for (int i = 0; i < categories.Length; i++)
                {
                    CheckBox chk = new CheckBox
                    {
                        Text = categories[i],
                        Name = "chkCategoriesIndex" + i,
                        Checked = true,
                        AutoSize = true
                    };
                    if (installed.Contains(categories[i]))
                    {
                        chk.Text = chk.Text + "（已安装）";
                        chk.Enabled = false;
                    }
                    panel.Controls.Add(chk);
                }
            }
            else
            {
                AddMsgToPanel(panel, "无功能选项");
            }
        }

        public static void AddMsgToPanel(Panel panel, string msg)
        {
            panel.Controls.Clear();
            Label label = new Label
            {
                Name = "lblCategoriesMsg",
                Text = msg,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(panel.Width, panel.Height)
            };
            panel.Controls.Add(label);
        }

        public static List<string> GetCategoriesFromPanel(Panel panel)
        {
            List<string> categories = new List<string>();
            foreach (Control ctrl in panel.Controls)
            {
                if (ctrl is CheckBox checkBox)
                {
                    if (checkBox.Enabled && checkBox.Checked)
                    {
                        categories.Add(checkBox.Text);
                    }
                }
                else if (ctrl is Label label)
                {
                    return null; // 如果是标签, 说明是精准匹配, 直接返回null
                }
            }
            return categories;
        }
    }
}
