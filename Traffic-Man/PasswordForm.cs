﻿using System;
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
    public partial class PasswordForm : Form
    {
        public PasswordForm()
        {
            InitializeComponent();

        }
        public string UserName
        {
            get => textBoxUserName.Text;
            set => textBoxUserName.Text = value;
        }
        public string Password
        {
            get => textBoxPassword.Text;
            set => textBoxPassword.Text = value; 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
