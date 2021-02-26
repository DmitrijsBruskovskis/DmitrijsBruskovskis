using System;
using System.Windows;
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
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Runtime.InteropServices;

namespace VideoDivisionRestarter
{
    public partial class Service1 : ServiceBase
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int Wow64DisableWow64FsRedirection(ref IntPtr ptr);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int Wow64EnableWow64FsRedirection(ref IntPtr ptr);

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
        }

        public void VideoToFrames(object obj)
        {
            var ffmpeg = ConfigurationManager.AppSettings["ffmpegPath"];
            var resultPath = ConfigurationManager.AppSettings["resultFilePath"];
            var afterDivisionPath = ConfigurationManager.AppSettings["videoAfterDivisionPath"];

            var ffmpegAbsolutePath = Path.GetFullPath(ffmpeg);
            var resultAbsolutePath = Path.GetFullPath(resultPath);
            var afterDivisionAbsolutePath = Path.GetFullPath(afterDivisionPath);

            DirectoryInfo resultDir = new DirectoryInfo(resultAbsolutePath);

            List<string> inputPathList = null;
            decimal framesPerMinute = 0;
            for (int i = 1; i < 2; i++)
            {
                VideoToFramesArg c = (VideoToFramesArg)obj;
                inputPathList = c.inputPathList;
                framesPerMinute = c.framesPerMinute;
            }

            IntPtr val = IntPtr.Zero;
            while (enabled)
            {
                Wow64DisableWow64FsRedirection(ref val);
                Stopwatch sWatch = Stopwatch.StartNew();
                foreach (var inputPath in inputPathList)
                {
                    DirectoryInfo dir = new DirectoryInfo(inputPath);
                    decimal seconds = 60 / framesPerMinute;

                    foreach (FileInfo file in dir.GetFiles("*.mp4"))
                    {
                        if (!enabled)
                        {
                            Wow64EnableWow64FsRedirection(ref val);
                            break;
                        }
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FullName);
                        string command = ffmpegAbsolutePath + " -i " + inputPath.Replace(@"\\", @"/") + "/" + file.Name + " -vf fps=1/" + seconds + " " + resultAbsolutePath + fileNameWithoutExtension + "_%03d.png";
                        //StreamWriter sw2 = new StreamWriter("C:/Projects/Git/DmitrijsBruskovskis/Midis.EyeOfHorus/Midis.EyeOfHorus.VideoDivisionService/bin/Debug/Test2.txt");
                        //sw2.WriteLine(command);
                        //sw2.Close();
                        ProcessStartInfo procStartInfo =
                            new ProcessStartInfo("cmd", "/c " + command);

                        procStartInfo.RedirectStandardOutput = true;
                        procStartInfo.UseShellExecute = false;

                        procStartInfo.CreateNoWindow = false;

                        Process proc = new Process();
                        proc.StartInfo = procStartInfo;
                        proc.Start();

                        string result = proc.StandardOutput.ReadToEnd();

                        File.Move(file.FullName, afterDivisionAbsolutePath + file.Name);

                        foreach (FileInfo image in resultDir.GetFiles("*.png"))
                        {
                            string imageNameWithoutExtension = Path.GetFileNameWithoutExtension(image.FullName);
                            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://192.168.1.88/Images/" + imageNameWithoutExtension + ".png");
                            request.UseBinary = true;
                            request.Method = WebRequestMethods.Ftp.UploadFile;
                            request.Credentials = new NetworkCredential("Midis0215", "Midis0215");

                            FileStream fs = new FileStream(image.FullName, FileMode.Open);

                            byte[] fileContents = new byte[fs.Length];
                            fs.Read(fileContents, 0, fileContents.Length);
                            fs.Close();
                            request.ContentLength = fileContents.Length;

                            Stream requestStream = request.GetRequestStream();
                            requestStream.Write(fileContents, 0, fileContents.Length);
                            requestStream.Close();

                            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                            response.Close();
                            image.Delete();
                        }
                        Wow64EnableWow64FsRedirection(ref val);
                    }
                }
                sWatch.Stop();
                if (sWatch.ElapsedMilliseconds < 60000)
                {
                    long timeToSleep = 60000 - sWatch.ElapsedMilliseconds;
                    Thread.Sleep((int)(timeToSleep));
                }
                Wow64EnableWow64FsRedirection(ref val);
            }
        }
    }
    public class VideoToFramesArg
    {
        public List<string> inputPathList;
        public decimal framesPerMinute;
    }
}
