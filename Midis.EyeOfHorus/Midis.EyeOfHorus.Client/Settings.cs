using Midis.EyeOfHorus.ClientLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Midis.EyeOfHorus.Client
{
    public partial class Iestatijumi : Form
    {
        public Iestatijumi()
        {
            InitializeComponent();

            //LoadData();
            ////xml file
            //var response = new SettingsSerializationHelper().ReadCD("po.xml");
            //txtFrCount.Text = Convert.ToString(response.FrameCount);
            //txtKey.Text = response.ClientKey;

        }

        SQLiteConnection con;
        SQLiteDataAdapter da;
        SQLiteCommand cmd;
        DataSet ds;
        Thread newThread;

        void GetList()
        {
            string cs = @"URI=file:C:\Projects\Git\DmitrijsBruskovskis\Midis.EyeOfHorus\Midis.EyeOfHorus.ClientLibrary\Database\DataBase.db";
            con = new SQLiteConnection(cs);
            da = new SQLiteDataAdapter("Select * From Cameras", con);
            ds = new DataSet();
            con.Open();
            da.Fill(ds, "Cameras");
            dataGridView1.DataSource = ds.Tables["Cameras"];
            con.Close();
            dataGridView1_CellClick(this, new DataGridViewCellEventArgs(0, 0));
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            GetList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ChooseFolder();
        }

        #region private methods
        private void ChooseFolder()
        {

        }
        #endregion

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            ////xml file
            //new SettingsSerializationHelper().CreateCD("po.xml", new ClientData {
            //    FrameCount = Convert.ToInt32(txtFrCount.Value),
            //    ClientKey = txtKey.Text
            //});

            string cs = @"URI=file:C:\Projects\Git\DmitrijsBruskovskis\Midis.EyeOfHorus\Midis.EyeOfHorus.ClientLibrary\Database\DataBase.db";
            con = new SQLiteConnection(cs);
            da = new SQLiteDataAdapter("SELECT * From Cameras", con);
            ds = new DataSet();
            con.Open();
            da.Fill(ds, "Cameras");
            con.Close();

            var InputPathList = new List<string>();
            foreach (DataRow row in ds.Tables["Cameras"].Rows)
            {
                InputPathList.Add(row["OutputFolder"].ToString());
            }

            if (InputPathList.Count != 0 && txtFrCount.Value!= 0)
            {
                newThread = new Thread(() => VideoDivisionFunctions.VideoToFrames(InputPathList, txtFrCount.Value));
                newThread.IsBackground = true;
                newThread.Start();
                //VideoDivisionFunctions.VideoToFrames(InputPathList, txtFrCount.Value);
            }
            else
                MessageBox.Show("Nepareizi uzdoti parametri!", "Kļūdas paziņojums");
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

        private void button3_Click(object sender, EventArgs e)
        {
            cmd = new SQLiteCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = "insert into Cameras(Name,OutputFolder) values ('" + textBox3.Text + "','" + textBox4.Text + "')";
            cmd.ExecuteNonQuery();
            con.Close();
            GetList();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            cmd = new SQLiteCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = "update Cameras set Name='" + textBox3.Text + "',OutputFolder='" + textBox4.Text + "' where ID=" + textBox2.Text + "";
            cmd.ExecuteNonQuery();
            con.Close();
            GetList();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            cmd = new SQLiteCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = "delete from Cameras where ID=" + textBox2.Text + "";
            cmd.ExecuteNonQuery();
            con.Close();
            GetList();
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            textBox2.Text = dataGridView1.CurrentRow.Cells[0].Value.ToString();
            textBox3.Text = dataGridView1.CurrentRow.Cells[1].Value.ToString();
            textBox4.Text = dataGridView1.CurrentRow.Cells[2].Value.ToString();
        }

        private void dataGridView1_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
