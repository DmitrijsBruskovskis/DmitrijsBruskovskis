using System;
using System.Collections.Generic;
using System.Text;

namespace Midis.EyeOfHorus.FaceDetectionLibrary.Models
{
    public class DatabaseInfoAboutWorker
    {
        public int Id { get; set; }

        public string FullName { get; set; }
        public byte[] Avatar { get; set; }
        public string ClientID { get; set; }

    }
}
