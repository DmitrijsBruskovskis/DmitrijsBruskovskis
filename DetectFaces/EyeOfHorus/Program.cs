using Midis.EyeOfHorus.FaceDetectionLibrary;

namespace Consol
{
    /// <summary>
    /// The main program class
    /// </summary>
    class Program
    {
        /// <summary>
        /// The main program entry point
        /// </summary>
        /// <param name="args">The command line arguments</param>
        static void Main(string[] args)
        {
            FaceDetectionLibrary.DetectFaces();  
        }
    }
}