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
            try
            {
                InitializeComponent();
            }
            catch
            {
                MessageBox.Show("Neizdevas palaist programmu korekti, mēģiniet vēlrēiz", "Kļūdas paziņojums");
            }
        }

        DataSet ds;
        ServiceController sc = new ServiceController("VideoDivisionService");
        bool running = false;
        OperationsWithDatabase OpWithDB = new OperationsWithDatabase();

        void GetList()
        {
            try
            {
                ds = OpWithDB.GetDataSet();
                dataGridView1.DataSource = ds.Tables["Cameras"];
                dataGridView1_CellClick(this, new DataGridViewCellEventArgs(0, 0));
            }
            catch
            {
                MessageBox.Show("Problēma ar pieslēgumu pie datubāzes, vajadzīgi restartēt programmu", "Kļūdas paziņojums");
            }
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            try
            {
                GetList();
            }
            catch
            {
                MessageBox.Show("Problēma ar formas ieladēšanu, vajadzīgi restartēt programmu", "Kļūdas paziņojums");
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            ds = OpWithDB.GetDataSet();

            var InputPathListAndCamerasIDs = new List<string>();
            foreach (DataRow row in ds.Tables["Cameras"].Rows)
            {
                InputPathListAndCamerasIDs.Add(row["OutputFolder"].ToString());
            }
            foreach (DataRow row in ds.Tables["Cameras"].Rows)
            {
                InputPathListAndCamerasIDs.Add(row["ID"].ToString());
            }
            InputPathListAndCamerasIDs.Add(txtKey.Text);
            InputPathListAndCamerasIDs.Add(txtFrCount.Value.ToString());
            string[] inputPathListArray = InputPathListAndCamerasIDs.ToArray();

            if (running)
                MessageBox.Show("Serviss vēl neapstājas, lūdzu, uzgaidiet", "Kļūdas paziņojums");
            else          
                if (InputPathListAndCamerasIDs.Count != 0 && txtKey.Text != "")
                {
                    running = true;
                    label7.Visible = false;
                    label1.Visible = true;
                    sc.Start(inputPathListArray);
                }
                else
                    MessageBox.Show("Nepareizi uzdoti parametri!", "Kļūdas paziņojums");
        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (running)
            {
                sc.Stop();
                running = false;
            }
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox3.Text !="" && textBox4.Text != "")
                {
                    string commandText = "insert into Cameras(Name,OutputFolder) values ('" + textBox3.Text + "','" + textBox4.Text + "')";
                    OpWithDB.ExecuteCommand(commandText);
                    GetList();
                }
                else
                    MessageBox.Show("Nevar pievienot kameru ar šādiem datiem, vajadzīgi pārbaudit tos un pāmeģināt velrēiz", "Kļūdas paziņojums");
            }
            catch
            {
                MessageBox.Show("Nevar pievienot kameru ar šādiem datiem, vajadzīgi pārbaudit tos un pāmeģināt velrēiz", "Kļūdas paziņojums");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox3.Text != "" && textBox4.Text != "")
                {
                    string commandText = "update Cameras set Name='" + textBox3.Text + "',OutputFolder='" + textBox4.Text + "' where ID=" + textBox2.Text + "";
                    OpWithDB.ExecuteCommand(commandText);
                    GetList();
                }
                else
                    MessageBox.Show("Nevar atjaunot kameru ar šādiem datiem, vajadzīgi pārbaudit tos un pāmeģināt velrēiz", "Kļūdas paziņojums");
            }
            catch
            {
                MessageBox.Show("Nevar atjaunot kameru ar šādiem datiem, vajadzīgi pārbaudit tos un pāmeģināt velrēiz", "Kļūdas paziņojums");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox2.Text != "")
                {
                    string commandText = "delete from Cameras where ID=" + textBox2.Text + "";
                    OpWithDB.ExecuteCommand(commandText);
                    commandText = "update sqlite_sequence set seq = 0 WHERE `name` = 'Cameras'";
                    OpWithDB.ExecuteCommand(commandText);
                    GetList();
                }
                else
                    MessageBox.Show("Nevar izdzēst šo kameru, pārbaudiet datus un mēģiniet velrēiz", "Kļūdas paziņojums");
            }
            catch
            {
                MessageBox.Show("Nevar izdzēst šo kameru, pārbaudiet datus un mēģiniet velrēiz", "Kļūdas paziņojums");
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow != null)
                {
                    textBox2.Text = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                    textBox3.Text = dataGridView1.CurrentRow.Cells[1].Value.ToString();
                    textBox4.Text = dataGridView1.CurrentRow.Cells[2].Value.ToString();
                }
            }
            catch
            {
                MessageBox.Show("Problēma ar datubāzi, restartējiet programmu", "Kļūdas paziņojums");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    textBox4.Text = folderBrowserDialog1.SelectedPath;
                }
            }
            catch
            {
                MessageBox.Show("Neizdevas izvēlēt failu, mēģiņiet velrēiz", "Kļūdas paziņojums");
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (running)
                {
                    sc.Stop();
                    running = false;
                    label7.Visible = true;
                    label1.Visible = false;
                }
                else
                    MessageBox.Show("Serviss jau apstājas vai procesā", "Kļūdas paziņojums");
            }
            catch
            {
                MessageBox.Show("Neizdevas uzzināt, vai serviss ir palaists, vajadzīgi pārbaudit to manuāli, atverat serviss menedžeru un atrast tur VideoDivisionServise. Ja viņš ir palaists - izslēgt viņu", "Kļūdas paziņojums");
            }
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }
    }
}
