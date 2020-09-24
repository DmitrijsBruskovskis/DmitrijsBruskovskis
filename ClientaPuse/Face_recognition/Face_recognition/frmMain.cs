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
            int AttSkaits;
            string Atslega;
        }

        public void ChooseFolder()
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtMape.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        public class XMLWrite
        {

            public class Book
            {
                public String title;
            }

            public static void WriteXML()
            {
                Book overview = new Book();
                overview.title = "Serialization Overview";
                System.Xml.Serialization.XmlSerializer writer =
                    new System.Xml.Serialization.XmlSerializer(typeof(Book));

                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "//SerializationOverview.xml";
                System.IO.FileStream file = System.IO.File.Create(path);

                writer.Serialize(file, overview);
                file.Close();
            }
        }

        public class DataClass
        {
            public DataClass()
            {
                //AttSkaits no xml
                //Atslega no xml/db
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            ChooseFolder();
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            //int AttSkaits = Convert.ToInt32(txtSkaits.Value);
            //label3.Text = Convert.ToString(AttSkaits);
            
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Hide();
            frmLogin fl = new frmLogin();
            fl.Show();
            //frmLogin login = false;
        }

        //private void FrmMain_Load(object sender, EventArgs e)
        //{

        //}
    }
}
