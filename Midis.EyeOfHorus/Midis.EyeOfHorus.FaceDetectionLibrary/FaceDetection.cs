using DlibDotNet;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Midis.EyeOfHorus.FaceDetectionLibrary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using static LinqToDB.DataProvider.SqlServer.SqlServerProviderAdapter;
using Dlib = DlibDotNet.Dlib;

namespace Midis.EyeOfHorus.FaceDetectionLibrary
{
    public class FaceDetectionLibrary
    {
        public static void DetectFacesAsync(string inputFilePath, string subscriptionKey, string uriBase, IFaceClient client, string vocabularyPath)
        {
            bool enabled = true;
            // set up Dlib facedetector
            while (enabled)
            {
                Thread.Sleep((int)(3000));
                DirectoryInfo dir = new DirectoryInfo(inputFilePath);

                using (var fd = Dlib.GetFrontalFaceDetector())
                {
                    foreach (FileInfo file in dir.GetFiles("*.jpeg"))
                    {
                        Thread.Sleep((int)(2000));
                        string _inputFilePath = inputFilePath + file.Name;

                        // load input image
                        Array2D<RgbPixel> img = Dlib.LoadImage<RgbPixel>(_inputFilePath);

                        // find all faces in the image
                        Rectangle[] faces = fd.Operator(img);
                        if (faces.Length != 0)
                        {
                            Console.WriteLine("Picture " + file.Name + " have faces, sending data to Azure");
                            MakeAnalysisRequestAsync(_inputFilePath, subscriptionKey, uriBase, file.Name, client, vocabularyPath, inputFilePath).Wait();
                        }
                        Console.WriteLine();
                        file.Delete();

                        string xmlName = file.Name.Substring(0, file.Name.LastIndexOf('_')) + ".xml";
                        if (dir.GetFiles(file.Name.Substring(0, file.Name.LastIndexOf('_')) + "*.jpeg").Length <= 0)
                            foreach (FileInfo xmlFile in dir.GetFiles(xmlName))
                                xmlFile.Delete();
                    }
                }
            }

            // Gets the analysis of the specified image by using the Face REST API.
            static async Task MakeAnalysisRequestAsync(string inputFilePath, string subscriptionKey, string uriBase, string fileName, IFaceClient faceClient, string vocabularyPath, string inputPathWithoutFileName)
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters. A third optional parameter is "details".
                string requestParameters = "recognitionModel=recognition_03&returnFaceId=true&returnFaceLandmarks=false";
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

                    //Execute the REST API call.
                    response = client.PostAsync(uri, content).GetAwaiter().GetResult();

                    // Get the JSON response.
                    string contentString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    // JSON response deserialization in list
                    var infoAboutImage = JsonConvert.DeserializeObject<IList<InfoAboutImage>>(contentString);

                    //Face group creation                  
                    // Create a dictionary for all images, grouping similar ones under the same key.

                    string connString = "Server=(localdb)\\mssqllocaldb;Database=aspnet-WebApp-2BE43A8A-E317-4564-94DC-4EBE6995F407;Trusted_Connection=True;MultipleActiveResultSets=true";
                    var workersList = new List<Workers>();
                    using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(connString))
                    {
                        SqlCommand cmd = new SqlCommand("DBO_WORKERS_GET_LIST", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        con.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            workersList.Add(new Workers
                            {
                                Id = Convert.ToInt32(rdr[0]),
                                FullName = rdr[1].ToString(),
                                ImageName = rdr[2].ToString(),
                                Avatar = (byte[])rdr[3],
                                ClientID = rdr[4].ToString()
                            });
                        }
                    }
                    var workersForProcessing = new WorkersForProcessing();
                    for (int i = 0; i <= workersList.Count; i++)
                    {
                        workersForProcessing.FullName = workersList[i].FullName;
                        workersForProcessing.Avatar = workersList[i].Avatar;
                    }

                    Console.WriteLine("Person group creation and training");
                    Dictionary<string, byte[]> personDictionary =
                        new Dictionary<string, byte[]>
                        {
                            { workersForProcessing.FullName, workersForProcessing.Avatar}
                            //    {"Dzon Skotch", new[] { "Dzon_Skotch_1.jpeg", "Dzon_Skotch_2.jpeg" } },
                            //{ "Matiju Ferst", new[] { "Matiju_Ferst_1.jpeg"} },
                            //{ "Test1", new[] { "1.jpg", "2.jpg" } },
                            //{ "Test2", new[] { "3.jpg", "4.jpg" } },
                        };
                    //Todo! затестить этого франкнштейна)

                    // Create a person group. 
                    string personGroupId = Guid.NewGuid().ToString();
                    Console.WriteLine($"Create a person group ({personGroupId}).");
                    await faceClient.PersonGroup.CreateAsync(personGroupId, personGroupId, null, RecognitionModel.Recognition03);
                    // The similar faces will be grouped into a single person group person.
                    foreach (var groupedFace in personDictionary.Keys)
                    {
                        // Limit TPS
                        await Task.Delay(250);
                        Person person = await faceClient.PersonGroupPerson.CreateAsync(personGroupId: personGroupId, name: groupedFace);
                        Console.WriteLine($"Create a person group person '{groupedFace}'.");
                        // Add face to the person group person.
                        foreach (var similarImage in personDictionary[groupedFace])
                        {
                            Console.WriteLine($"Add face to the person group person({groupedFace}) from image");

                            //using Stream imageFileStream = File.OpenRead($"{vocabularyPath}{similarImage}");
                            await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(
                                personGroupId, person.PersonId, similarImage);
                            //Нужен стрим картинки, бяка
                        }
                    }

                    // Start to train the person group.
                    Console.WriteLine();
                    Console.WriteLine($"Train person group {personGroupId}.");
                    await faceClient.PersonGroup.TrainAsync(personGroupId);

                    // Wait until the training is completed.
                    while (true)
                    {
                        await Task.Delay(1000);
                        var trainingStatus = await faceClient.PersonGroup.GetTrainingStatusAsync(personGroupId);
                        Console.WriteLine($"Training status: {trainingStatus.Status}.");
                        if (trainingStatus.Status == TrainingStatusType.Succeeded) { break; }
                    }

                    Console.WriteLine("Person identification");
                    List<Guid?> sourceFaceIds = new List<Guid?>();
                    // Add detected faceId to sourceFaceIds.
                    foreach (var detectedFace in infoAboutImage)
                    {
                        sourceFaceIds.Add(Guid.Parse(detectedFace.FaceId));
                    }

                    var identifyResults = await faceClient.Face.IdentifyAsync(sourceFaceIds, personGroupId, null, 1, 0.0000001);

                    for (int i = 0; i < identifyResults.Count; i++)
                    {
                        for (int j = 0; j < identifyResults[i].Candidates.Count; j++)
                        {
                            if (identifyResults[i].Candidates[j].Confidence > 0.51)
                            {
                                Person person = await faceClient.PersonGroupPerson.GetAsync(personGroupId, identifyResults[i].Candidates[j].PersonId);
                                infoAboutImage[i].Worker = person.Name;
                                Console.WriteLine($"Person '{person.Name}' is identified for face in: {fileName} - {identifyResults[i].FaceId}," +
                                    $" confidence: {identifyResults[i].Candidates[j].Confidence}.");
                                break;
                            }
                            else
                                infoAboutImage[i].Worker = "Unidentified person";
                        }
                    }

                    string xmlName = fileName.Substring(0, fileName.LastIndexOf('_')) + ".xml";
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(inputPathWithoutFileName + xmlName);
                    XmlElement xRoot = xDoc.DocumentElement;
                    string clientID = "";
                    string cameraID = "";
                    foreach (XmlNode xnode in xRoot)
                    {
                        XmlNode xmlClientID = xnode.Attributes.GetNamedItem("ClientID");
                        XmlNode xmlCameraID = xnode.Attributes.GetNamedItem("CameraID");
                        clientID = xmlClientID.Value.ToString();
                        cameraID = xmlCameraID.Value.ToString();
                    }

                    // Listing each element from JSON response and transfer data to the database.
                    Console.WriteLine("\nWork with database:\n");
                    for (int i = 0; i < infoAboutImage.Count; i++)
                    {
                        using (ApplicationContext db = new ApplicationContext())
                        {
                            DatabaseInfoAboutFace databaseInfoAboutFace = new DatabaseInfoAboutFace
                            {
                                ClientID = clientID,
                                CameraID = Int32.Parse(cameraID),
                                FileName = fileName,
                                Worker = infoAboutImage[i].Worker,
                                FaceRectangle = infoAboutImage[i].FaceRectangle.ToString()
                            };
                            if (databaseInfoAboutFace.Worker == "Unidentified person")
                            {
                                byte[] imageData = null;
                                imageData = GetImageAsByteArray(inputFilePath);
                                databaseInfoAboutFace.Image = imageData;
                            }
                            db.Add(databaseInfoAboutFace);
                            db.SaveChanges();
                        }
                        Console.WriteLine(infoAboutImage[i].Worker);
                        Console.WriteLine(fileName);
                        Console.WriteLine(infoAboutImage[i].FaceRectangle);
                    }
                    Console.WriteLine("Person group deletion");
                    await faceClient.PersonGroup.DeleteAsync(personGroupId);
                    Console.WriteLine();
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