using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    [Table ("InfoAboutFaces", Schema ="public")]
    public class Results
    {
        [Key]
        public int Id { get; set; }
        public string Worker { get; set; }
        public string FaceRectangle { get; set; }
        public string ClientID { get; set; }
        public int CameraID { get; set; }
        public string FileName { get; set; }
        public byte[] Image { get; set; }
        public DateTime DateTime { get; set; }
    }
}
