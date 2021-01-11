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
        VideoDivisionComponent videoDivisionComponent;
        public Service1()
        {
            InitializeComponent();
            this.CanStop = true;
        }

        protected override void OnStart(string[] args)
        {
            videoDivisionComponent = new VideoDivisionComponent();
            Thread videoDivisionThread = new Thread(new ThreadStart(videoDivisionComponent.Start));
            videoDivisionThread.Start();
        }

        protected override void OnStop()
        {
            videoDivisionComponent.Stop();
            Thread.Sleep(1000);
        }       
    }
    public class VideoDivisionComponent
    {
        bool enabled = true;
        public void Start()
        {
            //VideoToFrames(framesPerMinute, inputPathList);
            while (enabled)
            {
                Thread.Sleep(1000);
            }
        }
        public void Stop()
        {
            enabled = false;
        }

        public static void VideoToFrames(List<string> inputPathList, decimal framesPerMinute)
        {
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
        }
    }
}
