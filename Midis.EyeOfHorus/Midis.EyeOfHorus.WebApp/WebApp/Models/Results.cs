using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    public class Results
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string FaceRectangle { get; set; }
        public string ClientID { get; set; }
        public int  CameraID { get; set; }
        public string FileName { get; set; }
        public DateTime DateTime { get; set; }
    }
}
