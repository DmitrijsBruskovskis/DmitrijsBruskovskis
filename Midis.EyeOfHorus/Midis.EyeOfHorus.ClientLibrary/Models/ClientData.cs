using System.Xml.Serialization;

namespace Midis.EyeOfHorus.ClientLibrary
{
    [XmlRootAttribute("ClientData", Namespace = "http://www.cpandl.com", IsNullable = false)]
    public class ClientData
    {
        public int FrameCount;
        public string ClientKey;
    }
}
