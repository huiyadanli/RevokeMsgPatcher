using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            Console.WriteLine(new JsonData().BagJson());
        }
    }
}
