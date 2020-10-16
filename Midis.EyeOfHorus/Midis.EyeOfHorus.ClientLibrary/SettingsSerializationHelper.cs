using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Midis.EyeOfHorus.ClientLibrary
{
    public class SettingsSerializationHelper
    {
        public ClientData ReadCD(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ClientData));

            serializer.UnknownNode += new XmlNodeEventHandler(SerializerUnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(SerializerUnknownAttribute);

            FileStream fs = new FileStream(filename, FileMode.Open);
            ClientData cd;
            cd = (ClientData)serializer.Deserialize(fs);

            var response = new ClientData { 
                ClientKey = cd.ClientKey,
                FrameCount = cd.FrameCount
            };
            fs.Close();

            return response;
        }

        public void CreateCD(string filename, ClientData clientData)
        {
            ///Notiks neapstrādāta kļūda, ja nebūs tiesību uz failu
            XmlSerializer serializer = new XmlSerializer(typeof(ClientData));
            TextWriter writer = new StreamWriter(filename);
            ClientData cd = new ClientData();

            cd.FrameCount = Convert.ToInt32(clientData.FrameCount);
            cd.ClientKey = clientData.ClientKey;


            serializer.Serialize(writer, cd);
            writer.Close();
        }

        private void SerializerUnknownNode(object sender, XmlNodeEventArgs e)
        {
            Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
        }

        private void SerializerUnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            Console.WriteLine("Unknown attribute " +
            attr.Name + "='" + attr.Value + "'");
        }
    }
}
