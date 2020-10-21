namespace Midis.EyeOfHorus.FaceDetectionLibrary.Models
{
    public class DatabaseInfoAboutImage
    {
        public int Id { get; set; }
        public string FaceId { get; set; }
        //public string ClientId { get; set; }
        //public int CameraId { get; set; }
        public string FaceRectangle { get; set; }
    }
}
