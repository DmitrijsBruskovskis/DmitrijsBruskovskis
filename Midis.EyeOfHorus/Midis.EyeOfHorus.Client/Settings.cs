using Midis.EyeOfHorus.ClientLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Midis.EyeOfHorus.Client
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();

            ////notiks kļuda, ja nebūs po.xml faila!!!
            //var response = new SettingsSerializationHelper().ReadCD("po.xml");
            //txtFrCount.Text = Convert.ToString(response.FrameCount);
            //txtKey.Text = response.ClientKey;
        }

        private void Settings_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            ChooseFolder();
        }

        #region private methods
        private void ChooseFolder()
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }
        #endregion

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            //new SettingsSerializationHelper().CreateCD("po.xml", new ClientData {
            //    FrameCount = Convert.ToInt32(txtFrCount.Value),
            //    ClientKey = txtKey.Text
            //});
            VideoDivisionFunctions.VideoToFrames();
        }

        private void logOut_Click(object sender, EventArgs e)
        {
            this.Hide();
            Login fl = new Login();
            fl.Show();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
