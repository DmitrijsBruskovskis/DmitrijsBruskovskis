using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Midis.EyeOfHorus.ClientLibrary
{
    public class XmlFile
    {
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
    }
}
