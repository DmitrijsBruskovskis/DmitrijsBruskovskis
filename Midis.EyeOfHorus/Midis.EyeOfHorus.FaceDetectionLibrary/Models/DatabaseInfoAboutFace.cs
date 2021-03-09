using System;

namespace Midis.EyeOfHorus.FaceDetectionLibrary.Models
{
    public class DatabaseInfoAboutFace
    {
        public int Id { get; set; }
        public string Worker { get; set; }
        public string ClientID { get; set; }
        public int CameraID { get; set; }
        public string FaceRectangle { get; set; }
        public string FileName { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;

    }
}
