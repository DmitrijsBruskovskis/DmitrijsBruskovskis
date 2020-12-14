using Midis.EyeOfHorus.ClientLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Midis.EyeOfHorus.Client
{
    public partial class Iestatijumi : Form
    {
        public Iestatijumi()
        {
            InitializeComponent();
            LoadData();
            ////notiks kļuda, ja nebūs po.xml faila!!!
            //var response = new SettingsSerializationHelper().ReadCD("po.xml");
            //txtFrCount.Text = Convert.ToString(response.FrameCount);
            //txtKey.Text = response.ClientKey;
        }

        public void LoadData()
        {
            string cs = @"URI=file:C:\Projects\Git\DmitrijsBruskovskis\Midis.EyeOfHorus\Midis.EyeOfHorus.ClientLibrary\Database\DataBase.db";

            using var con = new SQLiteConnection(cs);

            con.Open();

            string stm = "SELECT * FROM Cameras ORDER BY ID";

            using var cmd = new SQLiteCommand(stm, con);

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            List<string[]> data = new List<string[]>();

            while (rdr.Read())
            {
                data.Add(new string[3]);

                data[data.Count - 1][0] = rdr[0].ToString();
                data[data.Count - 1][1] = rdr[1].ToString();
                data[data.Count - 1][2] = rdr[2].ToString();
            }

            rdr.Close();

            con.Close();

            foreach (string[] s in data)
                dataGridView1.Rows.Add(s);
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
            VideoDivisionFunctions.VideoToFrames(textBox1.Text, txtFrCount.Value);
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

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
