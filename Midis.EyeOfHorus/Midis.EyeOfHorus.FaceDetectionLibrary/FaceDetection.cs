using DlibDotNet;
using Midis.EyeOfHorus.FaceDetectionLibrary.Models;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Midis.EyeOfHorus.FaceDetectionLibrary
{
    public class FaceDetection
    {
        public static void DetectFaces()
        {
            /*
             appsettings ielāde 
            https://stackoverflow.com/questions/38398022/access-from-class-library-to-appsetting-json-in-asp-net-core
            https://pradeeploganathan.com/dotnet/configuration-in-a-net-core-console-application/

            kā piemēru var izmantot šo resursu
            https://blog.bitscry.com/2017/05/30/appsettings-json-in-net-core-console-app/

            app.config jāpārnes uz console programmu un jāpārsauc 
             */
            var inputFilePath = ConfigurationManager.AppSettings["inputFilePath"];
            var subscriptionKey = ConfigurationManager.AppSettings["subscriptionKey"];
            var uriBase = ConfigurationManager.AppSettings["uriBase"];

            // set up Dlib facedetector
            using (var fd = Dlib.GetFrontalFaceDetector())
            {
                // load input image
                var img = Dlib.LoadImage<RgbPixel>(ProjectConstants.inputFilePath);

                // find all faces in the image
                var faces = fd.Operator(img);
                if (faces.Length != 0)
                {
                    Console.WriteLine("Picture have faces, sending data to Azure");

                    if (File.Exists(ProjectConstants.inputFilePath))
                    {
                        try
                        {
                            MakeAnalysisRequest(ProjectConstants.inputFilePath);
                            Console.WriteLine("\nWait a moment for the results to appear.\n");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("\n" + e.Message + "\nPress Enter to exit...\n");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nInvalid file path.\nPress Enter to exit...\n");
                    }
                    Console.ReadLine();
                }

                foreach (var face in faces)
                {
                    // draw a rectangle for each face
                    Dlib.DrawRectangle(img, face, color: new RgbPixel(0, 255, 255), thickness: 4);
                }
                // export the modified image
                Dlib.SaveJpeg(img, "./Tests/output.jpg");
            }

            // Gets the analysis of the specified image by using the Face REST API.
            static async void MakeAnalysisRequest(string inputFilePath)
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", ProjectConstants.subscriptionKey);

                // Request parameters. A third optional parameter is "details".
                string requestParameters = "returnFaceId=true&returnFaceLandmarks=false";
                //"+ &returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," +
                //"emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

                // Assemble the URI for the REST API Call.
                string uri = ProjectConstants.uriBase + "?" + requestParameters;

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
                    response = await client.PostAsync(uri, content);

                    // Get the JSON response.
                    string contentString = await response.Content.ReadAsStringAsync();

                    // Display the JSON response.

                    //var test = Json.Encode(contentString);
                    //Console.WriteLine(test);

                    //Gson testGSON = new Gson();
                    //InfoAboutImage infoAboutImage = testGSON.fromJson(contentString, Image.FaceId, Image.FaceRectangle);

                    //InfoAboutImage infoAboutImage = JsonSerializer.Deserialize<InfoAboutImage>(contentString);

                    Console.WriteLine("\nResponse:\n");
                    Console.WriteLine(JsonPrettyPrint(contentString));
                    Console.WriteLine("\nPress Enter to exit...");
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

            // Formats the given JSON string by adding line breaks and indents.
            static string JsonPrettyPrint(string json)
            {
                if (string.IsNullOrEmpty(json))
                    return string.Empty;

                json = json.Replace(Environment.NewLine, "").Replace("\t", "");

                StringBuilder sb = new StringBuilder();
                bool quote = false;
                bool ignore = false;
                int offset = 0;
                int indentLength = 3;

                foreach (char ch in json)
                {
                    switch (ch)
                    {
                        case '"':
                            if (!ignore) quote = !quote;
                            break;
                        case '\'':
                            if (quote) ignore = !ignore;
                            break;
                    }

                    if (quote)
                        sb.Append(ch);
                    else
                    {
                        switch (ch)
                        {
                            case '{':
                            case '[':
                                sb.Append(ch);
                                sb.Append(Environment.NewLine);
                                sb.Append(new string(' ', ++offset * indentLength));
                                break;
                            case '}':
                            case ']':
                                sb.Append(Environment.NewLine);
                                sb.Append(new string(' ', --offset * indentLength));
                                sb.Append(ch);
                                break;
                            case ',':
                                sb.Append(ch);
                                sb.Append(Environment.NewLine);
                                sb.Append(new string(' ', offset * indentLength));
                                break;
                            case ':':
                                sb.Append(ch);
                                sb.Append(' ');
                                break;
                            default:
                                if (ch != ' ') sb.Append(ch);
                                break;
                        }
                    }
                }
                return sb.ToString().Trim();
            }
        }
    }
}
