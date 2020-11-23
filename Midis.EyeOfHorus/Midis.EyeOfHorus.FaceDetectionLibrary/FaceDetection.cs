using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DlibDotNet;
using Midis.EyeOfHorus.FaceDetectionLibrary.Models;
using Newtonsoft.Json;
using Dlib = DlibDotNet.Dlib;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.Linq;

namespace Midis.EyeOfHorus.FaceDetectionLibrary
{
    public class FaceDetectionLibrary
    {
        public static void DetectFacesAsync(string inputFilePath, string subscriptionKey, string uriBase, IFaceClient client, string url)
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
                        MakeAnalysisRequestAsync(_inputFilePath, subscriptionKey, uriBase, files.Name, client, url).Wait();
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
            static async Task MakeAnalysisRequestAsync(string inputFilePath, string subscriptionKey, string uriBase, string fileName, IFaceClient faceClient, string url)
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
                    Console.WriteLine("Person group creation and training");
                    Dictionary<string, string[]> personDictionary =
                        new Dictionary<string, string[]>
                        { {"Obama", new[] { "1.jpg", "11.jpg" } },
                        { "Toni", new[] { "2.jpg", "22.jpg" } },
                        { "Merkel", new[] { "3.jpg", "33.jpg" } },
                        { "Vladimir", new[] { "4.jpg", "44.jpg" } },
                        };

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
                            Console.WriteLine($"Add face to the person group person({groupedFace}) from image `{similarImage}`");

                            using Stream imageFileStream = File.OpenRead($"{url}{similarImage}");
                            await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(
                                personGroupId, person.PersonId, imageFileStream);
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

                    //Person identification
                    foreach (var identifyResult in identifyResults)
                    {
                        if (identifyResult.Candidates[0].Confidence > 0.5)
                        {
                            Person person = await faceClient.PersonGroupPerson.GetAsync(personGroupId, identifyResult.Candidates[0].PersonId);
                            foreach (var detectedFace in infoAboutImage)
                            {
                                detectedFace.Worker = person.Name; ;
                            }
                            Console.WriteLine($"Person '{person.Name}' is identified for face in: {fileName} - {identifyResult.FaceId}," +
                                $" confidence: {identifyResult.Candidates[0].Confidence}.");
                        }
                        else
                            foreach (var detectedFace in infoAboutImage)
                            {
                                detectedFace.Worker = "Unidentified person"; ;
                            }
                    }

                    // Listing each element from JSON response and data transfer to the database.
                    Console.WriteLine("\nWork with database:\n");
                    for (int i = 0; i < infoAboutImage.Count; i++)
                    {
                        using (ApplicationContext db = new ApplicationContext())
                        {
                            DatabaseInfoAboutImage databaseInfoAboutImage = new DatabaseInfoAboutImage
                            {
                                ClientId = 1,
                                CameraId = 1,
                                FileName = fileName,
                                Worker = infoAboutImage[i].Worker,
                                FaceRectangle = infoAboutImage[i].FaceRectangle.ToString()         
                            };
                            db.Add(databaseInfoAboutImage);
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

        //Working group creation and face identification example
        //public static async Task CreatePersonGroupTest(IFaceClient client, string url, string RECOGNITION_MODEL1)
        //{
        //    // Create a dictionary for all your images, grouping similar ones under the same key.
        //    Dictionary<string, string[]> personDictionary =
        //        new Dictionary<string, string[]>
        //        { {"Obama", new[] { "2.jpg", "22.jpg" } },
        //        { "Toni", new[] { "11.jpg", "111.jpg" } },
        //        //{ "Family1-Son", new[] { "Family1-Son1.jpg", "Family1-Son2.jpg" } },
        //        //{ "Family1-Daughter", new[] { "Family1-Daughter1.jpg", "Family1-Daughter2.jpg" } },
        //        //{ "Family2-Lady", new[] { "Family2-Lady1.jpg", "Family2-Lady2.jpg" } },
        //        //{ "Family2-Man", new[] { "Family2-Man1.jpg", "Family2-Man2.jpg" } }
        //        };
        //    // A group photo that includes some of the persons you seek to identify from your dictionary.
        //    string sourceImageFileName = "test2.jpg";

        //    // Create a person group. 
        //    string personGroupId = Guid.NewGuid().ToString();
        //    Console.WriteLine($"Create a person group ({personGroupId}).");
        //    await client.PersonGroup.CreateAsync(personGroupId, personGroupId, null, RECOGNITION_MODEL1);
        //    // The similar faces will be grouped into a single person group person.
        //    foreach (var groupedFace in personDictionary.Keys)
        //    {
        //        // Limit TPS
        //        await Task.Delay(250);
        //        Person person = await client.PersonGroupPerson.CreateAsync(personGroupId: personGroupId, name: groupedFace);
        //        Console.WriteLine($"Create a person group person '{groupedFace}'.");

        //        // Add face to the person group person.
        //        foreach (var similarImage in personDictionary[groupedFace])
        //        {
        //            Console.WriteLine($"Add face to the person group person({groupedFace}) from image `{similarImage}`");

        //            using Stream imageFileStream = File.OpenRead($"{url}{similarImage}");
        //            await client.PersonGroupPerson.AddFaceFromStreamAsync(
        //                personGroupId, person.PersonId, imageFileStream);
        //        }
        //    }

        //    // Start to train the person group.
        //    Console.WriteLine();
        //    Console.WriteLine($"Train person group {personGroupId}.");
        //    await client.PersonGroup.TrainAsync(personGroupId);

        //    // Wait until the training is completed.
        //    while (true)
        //    {
        //        await Task.Delay(1000);
        //        var trainingStatus = await client.PersonGroup.GetTrainingStatusAsync(personGroupId);
        //        Console.WriteLine($"Training status: {trainingStatus.Status}.");
        //        if (trainingStatus.Status == TrainingStatusType.Succeeded) { break; }
        //    }

        //    List<Guid?> sourceFaceIds = new List<Guid?>();
        //    // Detect faces from source image url.
        //    List<DetectedFace> detectedFaces = await DetectFaceRecognize(client, $"{url}{sourceImageFileName}", RECOGNITION_MODEL1);

        //    // Add detected faceId to sourceFaceIds.
        //    foreach (var detectedFace in detectedFaces)
        //    {
        //        sourceFaceIds.Add(detectedFace.FaceId.Value);
        //    }
        //    // Identify the faces in a person group. 
        //    var identifyResults = await client.Face.IdentifyAsync(sourceFaceIds, personGroupId);
        //    foreach (var identifyResult in identifyResults)
        //    {
        //        Person person = await client.PersonGroupPerson.GetAsync(personGroupId, identifyResult.Candidates[0].PersonId);
        //        Console.WriteLine($"Person '{person.Name}' is identified for face in: {sourceImageFileName} - {identifyResult.FaceId}," +
        //            $" confidence: {identifyResult.Candidates[0].Confidence}.");
        //    }
        //    await client.PersonGroup.DeleteAsync(personGroupId);
        //    Console.WriteLine();

        //    // Detect faces from target image.
        //    static async Task<List<DetectedFace>> DetectFaceRecognize(IFaceClient faceClient, string url, string RECOGNITION_MODEL1)
        //    {
        //        using Stream imageFileStream = File.OpenRead(url);
        //        IList<DetectedFace> detectedFaces =
        //            await faceClient.Face.DetectWithStreamAsync(imageFileStream, true, false, null, RECOGNITION_MODEL1, true);

        //        Console.WriteLine($"{detectedFaces.Count} face(s) detected from image `{Path.GetFileName(url)}`");
        //        return detectedFaces.ToList();
        //    }
        //}
    }
}