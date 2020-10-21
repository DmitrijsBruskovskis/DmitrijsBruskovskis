using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using DlibDotNet;
using Midis.EyeOfHorus.FaceDetectionLibrary.Models;
using Newtonsoft.Json;
using Dlib = DlibDotNet.Dlib;

namespace Midis.EyeOfHorus.FaceDetectionLibrary
{
    public class FaceDetectionLibrary
    {
        public static void DetectFaces(string inputFilePath, string subscriptionKey, string uriBase)
        {
            // set up Dlib facedetector
            DirectoryInfo dir = new DirectoryInfo(inputFilePath);

            using (var fd = Dlib.GetFrontalFaceDetector())
            {
                foreach (FileInfo files in dir.GetFiles("*.jpg"))
                {
                    string _inputFilePath = inputFilePath + files.Name;

                    // load input image
                    Array2D <RgbPixel> img = Dlib.LoadImage<RgbPixel>(_inputFilePath);

                    // find all faces in the image
                    Rectangle[] faces = fd.Operator(img);
                    if (faces.Length != 0)
                    {
                        Console.WriteLine("Picture " + files.Name + " have faces, sending data to Azure");
                        MakeAnalysisRequest(_inputFilePath, subscriptionKey, uriBase);
                    }

                    foreach (var face in faces)
                    {
                        // draw a rectangle for each face
                        Dlib.DrawRectangle(img, face, color: new RgbPixel(0, 255, 255), thickness: 4);
                    }
                    // export the modified image
                    Dlib.SaveJpeg(img, "./Results/" + files.Name);
                }
            }

            // Gets the analysis of the specified image by using the Face REST API.
            static void MakeAnalysisRequest(string inputFilePath, string subscriptionKey, string uriBase)
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters. A third optional parameter is "details".
                string requestParameters = "returnFaceId=true&returnFaceLandmarks=false";
                //"+ &returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," +
                //"emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

                // Assemble the URI for the REST API Call.
                string uri = uriBase + "?" + requestParameters;

                HttpResponseMessage response;

                // Request body. Posts a locally stored JPEG image.
                byte[] byteData = GetImageAsByteArray(inputFilePath);

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses content type "application/octet-stream".
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // Execute the REST API call.
                    response = client.PostAsync(uri, content).GetAwaiter().GetResult();

                    // Get the JSON response.
                    string contentString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    // JSON response deserialization in list
                    var infoAboutImage = JsonConvert.DeserializeObject<IList<InfoAboutImage>>(contentString);

                    // Data transfer to the database
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        DatabaseInfoAboutImage databaseInfoAboutImage = new DatabaseInfoAboutImage { Id = 1, FaceId = "12345-w", FaceRectangle = "Test2" };
                        db.Add(databaseInfoAboutImage);
                        db.SaveChanges();
                    }

                    // Display the JSON response.
                    Console.WriteLine("\nResponse:\n");
                    for (int i = 0; i < infoAboutImage.Count; i++)
                    {
                        Console.WriteLine(infoAboutImage[i].FaceId);
                        Console.WriteLine(infoAboutImage[i].FaceRectangle);
                    }                  
                }
            }

            // Returns the contents of the specified file as a byte array.
            static byte[] GetImageAsByteArray(string inputFilePath)
            {
                using (FileStream fileStream =
                    new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
                {
                    BinaryReader binaryReader = new BinaryReader(fileStream);
                    return binaryReader.ReadBytes((int)fileStream.Length);
                }
            }            
        }
    }
}