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
using System.Xml;
using System.Xml.Serialization;

namespace Face_recognition
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            ReadCD("po.xml");

            //t.ReadCD("po.xml");
        }


        public void ChooseFolder()
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtMape.Text = folderBrowserDialog1.SelectedPath;
            }
        }


        //public class Data
        //{
        //    [XmlAttribute]
        //    public int FrameCount;
        //}

        //private void CreatePO(string filename)
        //{
        // Create an instance of the XmlSerializer class;
        // specify the type of object to serialize.
        //XmlSerializer serializer =
        //new XmlSerializer(typeof(PurchaseOrder));
        //TextWriter writer = new StreamWriter(filename);
        //PurchaseOrder po = new PurchaseOrder();

        // Create an address to ship and bill to.
        //Data billAddress = new Data();
        //billAddress.FrameCount = Convert.ToInt32(txtSkaits.Value);



        //public class XMLWrite
        //{



        //    public static void WriteXML()
        //    {
        //        Book overview = new Book();
        //        overview.title = "Serialization Overview";
        //        System.Xml.Serialization.XmlSerializer writer =
        //            new System.Xml.Serialization.XmlSerializer(typeof(Book));

        //        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "//SerializationOverview.xml";
        //        System.IO.FileStream file = System.IO.File.Create(path);

        //        writer.Serialize(file, overview);
        //        file.Close();
        //    }
        //}

        //public class DataClass
        //{
        //    public DataClass()
        //    {
        //        //FrameCount no xml
        //        //ClientKey no xml/db
        //    }
        //}

        //}


        [XmlRootAttribute("ClientData", Namespace = "http://www.cpandl.com", IsNullable = false)]
        public class ClientData
        {
            public int FrameCount;
            public string ClientKey;
        }

        private void CreateCD(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ClientData));
            TextWriter writer = new StreamWriter(filename);
            ClientData cd = new ClientData();

            cd.FrameCount = Convert.ToInt32(txtFrCount.Value);
            cd.ClientKey = txtKey.Text;


            serializer.Serialize(writer, cd);
            writer.Close();
        }

        protected void ReadCD(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ClientData));
            
            serializer.UnknownNode += new
            XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new
            XmlAttributeEventHandler(serializer_UnknownAttribute);

            FileStream fs = new FileStream(filename, FileMode.Open);
            ClientData cd;
            cd = (ClientData)serializer.Deserialize(fs);

            txtFrCount.Text = Convert.ToString(cd.FrameCount);
            txtKey.Text = cd.ClientKey;

            fs.Close();
        }

        private void serializer_UnknownNode
   (object sender, XmlNodeEventArgs e)
        {
            Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
        }

        private void serializer_UnknownAttribute
        (object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            Console.WriteLine("Unknown attribute " +
            attr.Name + "='" + attr.Value + "'");
        }




        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Hide();
            frmLogin fl = new frmLogin();
            fl.Show();
            //frmLogin login = false;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            ChooseFolder();
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            CreateCD("po.xml");
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }
    }

}
