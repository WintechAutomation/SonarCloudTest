using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SonarCloudTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        int ClickCount = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            //TestCode
            if (true)
            {
                if (true)
                {

                }
            }
            label1.Text = string.Format("{0}번 누름", ++ClickCount);
        }
    }
}
