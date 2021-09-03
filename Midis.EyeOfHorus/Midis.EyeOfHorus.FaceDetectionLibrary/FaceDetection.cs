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
using System.Linq;
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
        public static bool personGroupExist = false;
        public static void DetectFacesAsync(string inputFilePath, string subscriptionKey, string uriBase, IFaceClient client, string databaseConnString)
        {
            string personGroupId = Guid.NewGuid().ToString();
            List<WorkersForProcessing> tempWorkersForProcessings = new List<WorkersForProcessing>();

            List<WorkersForProcessing> workersForProcessings = GetWorkersFromWebApplication(databaseConnString);
            CreateAndTrainWorkersPersonGroup(client, workersForProcessings, personGroupId).Wait();

            // Variable is needed for infinite loop
            bool enabledInfiniteLoop = true;

            // Infinite loop, which search for new images in the target folder and if search is succesfull,
            // then use the local library to find the faces in the image. If faces are founded, then starting work with
            // Microsot Azure API to identify faces and save results to the database.
            while (enabledInfiniteLoop)
            {
                Thread.Sleep((int)(3000));
                DirectoryInfo dir = new DirectoryInfo(inputFilePath);

                // Dlib library using for local face search
                using (var fd = Dlib.GetFrontalFaceDetector())
                {
                    foreach (FileInfo file in dir.GetFiles("*.jpeg"))
                    {
                        Thread.Sleep((int)(10000));
                        string _inputFilePath = inputFilePath + file.Name;

                        // load input image
                        Array2D<RgbPixel> img = Dlib.LoadImage<RgbPixel>(_inputFilePath);

                        // find all faces in the image
                        Rectangle[] faces = fd.Operator(img);
                        // if search was succesfull then starting work with Microsoft Azure API
                        if (faces.Length != 0)
                        {
                            Console.WriteLine("Picture " + file.Name + " have faces(according to the DlibDotNetNative library), sending data to Microsoft Azure API");
                            tempWorkersForProcessings = GetWorkersFromWebApplication(databaseConnString);
                            bool listsAreEqual = true;
                            if (workersForProcessings.Count() == tempWorkersForProcessings.Count())
                                for (int i = 0; i < workersForProcessings.Count(); i++)
                                {
                                    if (workersForProcessings[i].FullName != tempWorkersForProcessings[i].FullName)
                                    {
                                        listsAreEqual = false;
                                        break;
                                    }
                                    if (!workersForProcessings[i].Avatar.SequenceEqual(tempWorkersForProcessings[i].Avatar))
                                    {
                                        listsAreEqual = false;
                                        break;
                                    }
                                    if (workersForProcessings[i].ClientID != tempWorkersForProcessings[i].ClientID)
                                    {
                                        listsAreEqual = false;
                                        break;
                                    }
                                }
                            else
                                listsAreEqual = false;
                            CheckIfPersonGroupIsOutdated:
                            if(listsAreEqual)
                            {
                                FindFacesWithAPIIdentifyThemAndAddInDB(_inputFilePath, subscriptionKey, uriBase, file.Name, client, inputFilePath, personGroupId).Wait();
                                file.Delete();
                            }
                            else
                            {
                                DeletePersonGroup(client, personGroupId).Wait();
                                CreateAndTrainWorkersPersonGroup(client, tempWorkersForProcessings, personGroupId).Wait();
                                workersForProcessings = tempWorkersForProcessings;
                                listsAreEqual = true;
                                goto CheckIfPersonGroupIsOutdated;
                            }
                            //MakeAnalysisRequestAsync(_inputFilePath, subscriptionKey, uriBase, file.Name, client, databaseConnString, inputFilePath).Wait();
                        }
                        else
                            file.Delete();

                        // After all work is done delete XML file with info about image set, received from client 
                        string xmlName = file.Name.Substring(0, file.Name.LastIndexOf('_')) + ".xml";
                        if (dir.GetFiles(file.Name.Substring(0, file.Name.LastIndexOf('_')) + "*.jpeg").Length <= 0)
                            foreach (FileInfo xmlFile in dir.GetFiles(xmlName))
                                xmlFile.Delete();
                    }
                    DeletePersonGroup(client, personGroupId).Wait();
                }
            }
            // Working system.
            // Gets the analysis of the specified image by using the Face REST API.
            //static async Task MakeAnalysisRequestAsync(string inputFilePath, string subscriptionKey, string uriBase, string fileName, IFaceClient faceClient, string databaseConnString, string inputPathWithoutFileName)
            //{
                //// Get ClientID and CameraID from XML
                //(string, string) infoFromXML = GetInfoFromXML(fileName, inputPathWithoutFileName);

                //// Create a list with all workers from db.    
                //List<WorkersForProcessing> workerListForProcessing = GetWorkersFromWebApplication(databaseConnString);

                //// Create a person group. 
                //string personGroupId = Guid.NewGuid().ToString();
                //await CreatePersonGroup(faceClient, personGroupId);

                //// The person group person creation.
                //await CreatePersonWithFacesInPersonGroup(workerListForProcessing, faceClient, personGroupId);

                //// Train Person group
                //await TrainPersonGroup(faceClient, personGroupId);

                //// Create a request about face search on the image and get response with info about all founded faces
                //IList<InfoAboutImage> infoAboutImage = CreateSearchForFacesRequestToTheAPI(subscriptionKey, uriBase, inputFilePath);

                //// Unidentified Face Identification
                //await GetIdentifyResults(infoAboutImage, faceClient, personGroupId, fileName);

                //// Add results from identification to the Database
                //await AddDataFromResponseToDB(infoAboutImage, infoFromXML, fileName, inputFilePath);

                //// Delete Person Group after all work is done
                //await DeletePersonGroup(faceClient, personGroupId);
            //}

            static async Task CreateAndTrainWorkersPersonGroup(IFaceClient faceClient, List<WorkersForProcessing> workerListForProcessing, string personGroupId)
            {
                // Create a person group. 
                await CreatePersonGroup(faceClient, personGroupId);

                // The person group person creation.
                await CreatePersonWithFacesInPersonGroup(workerListForProcessing, faceClient, personGroupId);

                // Train Person group
                await TrainPersonGroup(faceClient, personGroupId);
            }

            static async Task FindFacesWithAPIIdentifyThemAndAddInDB(string inputFilePath, string subscriptionKey, string uriBase, string fileName, IFaceClient faceClient, string inputPathWithoutFileName, string personGroupId)
            {
                // Get ClientID and CameraID from XML
                (string, string) infoFromXML = GetInfoFromXML(fileName, inputPathWithoutFileName);

                // Create a request about face search on the image and get response with info about all founded faces
                IList<InfoAboutImage> infoAboutImage = CreateSearchForFacesRequestToTheAPI(subscriptionKey, uriBase, inputFilePath);

                // Unidentified Face Identification
                await GetIdentifyResults(infoAboutImage, faceClient, personGroupId, fileName);

                // Add results from identification to the Database
                await AddDataFromResponseToDB(infoAboutImage, infoFromXML, fileName, inputFilePath);
            }
        }

        static IList<InfoAboutImage> CreateSearchForFacesRequestToTheAPI(string subscriptionKey, string uriBase, string inputFilePath)
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

                return infoAboutImage;
            }
        }

        static List<WorkersForProcessing> GetWorkersFromWebApplication(string databaseConnString)
        {
            // GetWorkersFromWebApplication               
            // Create a list with all workers from db.                   
            var workersList = new List<Workers>();
            using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(databaseConnString))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.Workers", con);
                cmd.CommandType = CommandType.Text;
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
            var workerListForProcessing = new List<WorkersForProcessing>();
            for (int i = 0; i < workersList.Count; i++)
            {
                var workerForProcessing = new WorkersForProcessing();
                workerForProcessing.FullName = workersList[i].FullName;
                workerForProcessing.Avatar = workersList[i].Avatar;
                workerForProcessing.ClientID = workersList[i].ClientID;
                workerListForProcessing.Add(workerForProcessing);
            }
            return workerListForProcessing;
        }

        static async Task CreatePersonGroup(IFaceClient faceClient, string personGroupId)
        {
            // Create a person group. 
            if (!personGroupExist)
            {
                Console.WriteLine("Person group creation and training");
                Console.WriteLine($"Create a person group ({personGroupId}).");
                await faceClient.PersonGroup.CreateAsync(personGroupId, personGroupId, null, RecognitionModel.Recognition03);
                personGroupExist = true;
            }
        }

        static async Task CreatePersonWithFacesInPersonGroup(List<WorkersForProcessing> workerListForProcessing, IFaceClient faceClient, string personGroupId)
        {
            // The person group person creation.
            foreach (var worker in workerListForProcessing)
            {
                // Limit TPS
                await Task.Delay(250);
                Person person = await faceClient.PersonGroupPerson.CreateAsync(personGroupId: personGroupId, name: worker.FullName);
                Console.WriteLine($"Create a person group person '{worker.FullName}'.");

                // Add face to the person group person.
                Console.WriteLine($"Add face to the person group person({worker.FullName})");

                MemoryStream stream = new MemoryStream(worker.Avatar);

                await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(
                    personGroupId, person.PersonId, stream);
            }
        }

        static async Task TrainPersonGroup(IFaceClient faceClient, string personGroupId)
        {
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
        }

        static async Task GetIdentifyResults(IList<InfoAboutImage> infoAboutImage, IFaceClient faceClient, string personGroupId, string fileName)
        {
            // Add detected faceId to sourceFaceIds.
            Console.WriteLine("Person identification");
            List<Guid?> sourceFaceIds = new List<Guid?>();

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
        }

        static (string, string) GetInfoFromXML(string fileName, string inputPathWithoutFileName)
        {
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
            return (clientID, cameraID);
        }

        static async Task AddDataFromResponseToDB(IList<InfoAboutImage> infoAboutImage, (String, String) infoFromXML, string fileName, string inputFilePath)
        {
            // Listing each element from JSON response and transfer data to the database.
            Console.WriteLine("\nWork with database:\n");
            for (int i = 0; i < infoAboutImage.Count; i++)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    DatabaseInfoAboutFace databaseInfoAboutFace = new DatabaseInfoAboutFace
                    {
                        ClientID = infoFromXML.Item1,
                        CameraID = Int32.Parse(infoFromXML.Item2),
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
                    await db.AddAsync(databaseInfoAboutFace);
                    await db.SaveChangesAsync();
                }
                Console.WriteLine(infoAboutImage[i].Worker);
                Console.WriteLine(fileName);
                Console.WriteLine(infoAboutImage[i].FaceRectangle);
            }
        }

        static async Task DeletePersonGroup(IFaceClient faceClient, string personGroupId)
        {
            if (personGroupExist)
            {
                Console.WriteLine("Person group deletion");
                await faceClient.PersonGroup.DeleteAsync(personGroupId);
                Console.WriteLine();
                personGroupExist = false;
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