using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Midis.EyeOfHorus.ClientLibrary
{
    public class VideoDivisionFunctions
    {
        public static void VideoToFrames(string inputPath, decimal framesPerMinute)
        {
            DirectoryInfo dir = new DirectoryInfo(inputPath);
            decimal seconds = 60 / framesPerMinute;

            foreach (FileInfo files in dir.GetFiles("*.mp4"))
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files.FullName);
                string command = "C:/Projects/Git/DmitrijsBruskovskis/Midis.EyeOfHorus/Midis.EyeOfHorus.Client/bin/Debug/netcoreapp3.1/ffmpeg/bin/ffmpeg -i "
                                 + inputPath.Replace(@"\\", @"/") + "/" + files.Name + " -vf fps=1/" + seconds +
                                 " C:/Projects/Git/DmitrijsBruskovskis/Midis.EyeOfHorus/Midis.EyeOfHorus.Client/bin/Debug/netcoreapp3.1/ffmpeg/Results/" + fileNameWithoutExtension + "_%03d.png";

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
        }
    }
}
