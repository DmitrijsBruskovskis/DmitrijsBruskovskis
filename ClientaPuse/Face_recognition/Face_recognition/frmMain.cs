using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Face_recognition
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //int size = -1;
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.FileName;
                labelFileName.Text = file;
                //try
                //{
                //    string text = File.ReadAllText(file);
                //    size = text.Length;
                //}
                //catch (IOException)
                //{
                //}
            }
            //Console.WriteLine(size); // <-- Shows file size in debugging mode.
            //Console.WriteLine(result); // <-- For debugging use.
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            int AttSkaits = Convert.ToInt32(txtSkaits.Value);
            label3.Text = Convert.ToString(AttSkaits);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Hide();
            frmLogin fl = new frmLogin();
            fl.Show();
            //frmLogin login = false;
        }
    }
}
