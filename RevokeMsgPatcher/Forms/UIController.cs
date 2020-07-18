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
        public static void AddCategoryCheckBoxToPanel(Panel panel, string[] categories = null)
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
                    // 只有一个选项时,必选
                    if (categories.Length == 1)
                    {
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
                TextAlign = ContentAlignment.MiddleLeft
            };
            panel.Controls.Add(label);
        }

        public static string[] GetCategoriesFromPanel(Panel panel)
        {
            List<string> categories = new List<string>();
            foreach (CheckBox checkBox in panel.Controls)
            {
                if (checkBox.Checked)
                {
                    categories.Add(checkBox.Text);
                }
            }
            return categories.ToArray();
        }
    }
}
