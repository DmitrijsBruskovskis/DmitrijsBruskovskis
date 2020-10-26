using System;

namespace Midis.EyeOfHorus.FaceDetectionLibrary.Models
{
    public class DatabaseInfoAboutImage
    {
        public int Id { get; set; }
        public string FaceId { get; set; }
        public int ClientId { get; set; }
        public int CameraId { get; set; }
        public string FaceRectangle { get; set; }
        public string FileName { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;

    }
}
