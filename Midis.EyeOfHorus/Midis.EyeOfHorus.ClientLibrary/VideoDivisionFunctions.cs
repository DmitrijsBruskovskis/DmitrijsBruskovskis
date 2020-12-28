using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace Midis.EyeOfHorus.ClientLibrary
{
    public class VideoDivisionFunctions
    {
        public static void VideoToFrames(List<string> inputPathList, decimal framesPerMinute)
        {
            Restart:
            Stopwatch sWatch = new Stopwatch();
            sWatch.Start();
            foreach (var inputPath in inputPathList)
            {
                DirectoryInfo dir = new DirectoryInfo(inputPath);
                decimal seconds = 60 / framesPerMinute;

                foreach (FileInfo files in dir.GetFiles("*.mp4"))
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files.FullName);
                    string command = "C:/Projects/Git/DmitrijsBruskovskis/Midis.EyeOfHorus/Midis.EyeOfHorus.Client/bin/Debug/netcoreapp3.1/ffmpeg/bin/ffmpeg -i "
                                     + inputPath.Replace(@"\\", @"/") + "/" + files.Name + " -vf fps=1/" + seconds +
                                     " C:/Projects/Git/DmitrijsBruskovskis/Midis.EyeOfHorus/Midis.EyeOfHorus.Client/bin/Debug/netcoreapp3.1/ffmpeg/Results/" + fileNameWithoutExtension + "_%03d.png";

                    ProcessStartInfo procStartInfo =
                        new ProcessStartInfo("cmd", "/c " + command);

                    procStartInfo.RedirectStandardOutput = true;
                    procStartInfo.UseShellExecute = false;

                    procStartInfo.CreateNoWindow = false;

                    Process proc = new Process();
                    proc.StartInfo = procStartInfo;
                    proc.Start();

                    string result = proc.StandardOutput.ReadToEnd();

                    Console.WriteLine(result);
                    Console.WriteLine();
                }
            }
            sWatch.Stop();
            ifTimeThenRestart:
            if (sWatch.ElapsedMilliseconds > 60000)
                goto Restart;
            sWatch.Start();
            goto ifTimeThenRestart;
        }
    }
}
