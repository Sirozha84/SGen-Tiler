using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SGen_Tiler
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(Program.Web);
        }

        private void FormAbout_Load(object sender, EventArgs e)
        {
            Text = "О " + Application.ProductName;
            label1.Text = Application.ProductName;
            label2.Text = Program.Version;
            label3.Text = Program.Autor;
        }
    }
}
