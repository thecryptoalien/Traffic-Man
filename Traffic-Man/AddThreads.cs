using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Traffic_Man
{
    public partial class AddThreads : Form
    {
        public AddThreads()
        {
            InitializeComponent();
            
        }
        public string Threads
        {
            get => textBoxThreads.Text;
            set => textBoxThreads.Text = value;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Threads = "0";
            this.Close();
        }

        private void AddThreads_Load(object sender, EventArgs e)
        {
            textBoxThreads.Focus();
            //DebugBox("Yo Yo Yo");
        }
    }
}
