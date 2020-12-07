using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Midis.EyeOfHorus.ClientLibrary
{
    public class VideoDivisionFunctions
    {
        public static void VideoToFrames()
        {
            string command = "C:/Projects/ffmpeg/bin/ffmpeg -i C:/Projects/ffmpeg/VideoForTests/test.mp4 -r 0.01 C:/Projects/ffmpeg/Results/%03d.png";
            try
            {
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;

                procStartInfo.CreateNoWindow = false;

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();

                string result = proc.StandardOutput.ReadToEnd();

                Console.WriteLine(result);
                Console.WriteLine();
            }
            catch (Exception objException)
            {
                // Log the exception
            }
        }
    }
}
