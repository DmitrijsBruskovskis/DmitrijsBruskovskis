using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VideoDivisionRestarter
{
    public partial class Service1 : ServiceBase
    {
        Thread videoDivisionThread;
        bool enabled = true;
        public Service1()
        {
            InitializeComponent();
            this.CanStop = true;
        }

        protected override void OnStart(string[] args)
        {
            VideoToFramesArg videoToFramesArg = new VideoToFramesArg();
            string framesPerMinuteString = args.Last();
            videoToFramesArg.framesPerMinute = Convert.ToDecimal(framesPerMinuteString);
            videoToFramesArg.inputPathList = args.ToList();
            videoToFramesArg.inputPathList.RemoveAt(videoToFramesArg.inputPathList.Count-1);
            enabled = true;

            videoDivisionThread = new Thread(new ParameterizedThreadStart(VideoToFrames));
            videoDivisionThread.Start(videoToFramesArg);
        }

        protected override void OnStop()
        {
            Thread.Sleep(1000);
            enabled = false;
            //videoDivisionThread.Abort();
        }

        public void VideoToFrames(object obj)
        {
            List<string> inputPathList = null;
            decimal framesPerMinute = 0;
            for (int i = 1; i < 2; i++)
            {
                VideoToFramesArg c = (VideoToFramesArg)obj;
                inputPathList = c.inputPathList;
                framesPerMinute = c.framesPerMinute;
            }

            while (enabled)
            {
                Stopwatch sWatch = new Stopwatch();
                sWatch.Start();
                foreach (var inputPath in inputPathList)
                {
                    DirectoryInfo dir = new DirectoryInfo(inputPath);
                    decimal seconds = 60 / framesPerMinute;

                    foreach (FileInfo files in dir.GetFiles("*.mp4"))
                    {
                        if (!enabled)
                            break;
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
                if (sWatch.ElapsedMilliseconds >= 60000)
                    continue;
                else
                {
                    long timeToSleep = 60000 - sWatch.ElapsedMilliseconds;
                    Thread.Sleep((int)(timeToSleep / 1000));
                }
            }
        }
    }
    public class VideoToFramesArg
    {
        public List<string> inputPathList;
        public decimal framesPerMinute;
    }
}
