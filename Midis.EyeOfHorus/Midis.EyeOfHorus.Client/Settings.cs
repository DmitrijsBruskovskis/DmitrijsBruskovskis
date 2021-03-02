using Midis.EyeOfHorus.ClientLibrary.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Midis.EyeOfHorus.Client
{
    public partial class Iestatijumi : Form
    {
        public Iestatijumi()
        {
            InitializeComponent();
        }

        DataSet ds;
        ServiceController sc = new ServiceController("VideoDivisionService");
        bool running = false;
        OperationsWithDatabase OpWithDB = new OperationsWithDatabase();

        void GetList()
        {
            ds = OpWithDB.GetDataSet();
            dataGridView1.DataSource = ds.Tables["Cameras"];
            dataGridView1_CellClick(this, new DataGridViewCellEventArgs(0, 0));
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            GetList();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            ds = OpWithDB.GetDataSet();

            var InputPathList = new List<string>();
            foreach (DataRow row in ds.Tables["Cameras"].Rows)
            {
                InputPathList.Add(row["OutputFolder"].ToString());
            }
            foreach (DataRow row in ds.Tables["Cameras"].Rows)
            {
                InputPathList.Add(row["ID"].ToString());
            }
            InputPathList.Add(txtKey.Text);
            InputPathList.Add(txtFrCount.Value.ToString());
            string[] inputPathListArray = InputPathList.ToArray();

            if (running)
                MessageBox.Show("Serviss vēl neapstājas, lūdzu, uzgaidiet", "Kļūdas paziņojums");
            else
            if (InputPathList.Count != 0 && txtKey.Text != "")
            {
                running = true;
                sc.Start(inputPathListArray);
            }
            else
                MessageBox.Show("Nepareizi uzdoti parametri!", "Kļūdas paziņojums");
        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string commandText = "insert into Cameras(Name,OutputFolder) values ('" + textBox3.Text + "','" + textBox4.Text + "')";
            OpWithDB.ExecuteCommand(commandText);
            GetList();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string commandText = "update Cameras set Name='" + textBox3.Text + "',OutputFolder='" + textBox4.Text + "' where ID=" + textBox2.Text + "";
            OpWithDB.ExecuteCommand(commandText);
            GetList();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string commandText = "delete from Cameras where ID=" + textBox2.Text + "";
            OpWithDB.ExecuteCommand(commandText);
            GetList();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                textBox2.Text = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                textBox3.Text = dataGridView1.CurrentRow.Cells[1].Value.ToString();
                textBox4.Text = dataGridView1.CurrentRow.Cells[2].Value.ToString();
            }
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
            if (running)
            {
                sc.Stop();
                running = false;
            }
            else
                MessageBox.Show("Serviss jau apstājas vai procesā", "Kļūdas paziņojums");
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }
    }
}
