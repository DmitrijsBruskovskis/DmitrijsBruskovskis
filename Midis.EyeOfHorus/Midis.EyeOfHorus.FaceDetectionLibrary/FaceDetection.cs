using DlibDotNet;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.EntityFrameworkCore;
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
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Dlib = DlibDotNet.Dlib;

namespace Midis.EyeOfHorus.FaceDetectionLibrary
{
    public class FaceDetectionLibrary
    {
        public static void DetectFacesAsync(string inputFilePath, string subscriptionKey, string uriBase, IFaceClient client, string databaseConnString)
        {
            //GetTheListOfPersonGroupsFromAPIAndDeleteThem(client); //temporarily

            // Getting workers from web and from local db
            List<List<WorkersForProcessing>> listOfWorkerLists = GetWorkersFromWebApplicationInDifferentListsDividedByClientID(databaseConnString);
            List<Client> clientsInDB = GetClientListFromLocalDB();
            List<DatabaseInfoAboutWorker> workersFromLocalDB = GetWorkersFromLocalDB();

            // Change workersFromLocalDB format from List<DatabaseInfoAboutWorker> to List<List<WorkersForProcessing>>
            List<List<WorkersForProcessing>> listOfWorkerListsFromLocalDB = GroupUpWorkersFromLocalDBInDifferentListsDividedByClientID(workersFromLocalDB);

            // Prepare List, which will help update list or lists in API
            ListForUpdatingWithGroupId listForUpdatingWithGroupId = new ListForUpdatingWithGroupId();
            listForUpdatingWithGroupId.ListOfGroupId = new List<string>();
            listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating = new List<List<WorkersForProcessing>>();
            listForUpdatingWithGroupId.UpdateSource = new List<List<WorkersForProcessing>>();
            bool listsAreEqual = true;
            Client clientForUpdate = new Client();

            List<Client> listWithClientsFromWEB = new List<Client>();
            foreach (var workerList in listOfWorkerLists)
            {
                Client newClient = new Client();
                newClient.PersonGroupID = Guid.NewGuid().ToString();
                newClient.ClientID = workerList[0].ClientID;
                listWithClientsFromWEB.Add(newClient);
            }

            // Check if local DB have old Clients, and if it does,
            // then thansfer PersonGroupID(API) to the Clients in the new list
            if((listWithClientsFromWEB.Count() != 0) && (clientsInDB.Count() != 0))
            {
                if(listWithClientsFromWEB.Count() >= clientsInDB.Count())
                    for(int i = 0; i < clientsInDB.Count(); i++)
                    {
                        if (clientsInDB[i].ClientID == listWithClientsFromWEB[i].ClientID)
                        {
                            listWithClientsFromWEB[i].PersonGroupID = clientsInDB[i].PersonGroupID;
                        }
                    }           
                else
                    for (int i = 0; i < listWithClientsFromWEB.Count(); i++)
                    {
                        if (listWithClientsFromWEB[i].ClientID == clientsInDB[i].ClientID)
                        {
                            listWithClientsFromWEB[i].PersonGroupID = clientsInDB[i].PersonGroupID;
                        }
                    }
            }

            // Check if worker lists from WEB(aka Clients) and from local DB(aka API) are equal
            if (listOfWorkerLists.Count() == listOfWorkerListsFromLocalDB.Count())
                for (int j = 0; j < listOfWorkerLists.Count(); j++)
                {
                    if(listOfWorkerLists[j][0].ClientID != listOfWorkerListsFromLocalDB[j][0].ClientID)
                    {
                        listsAreEqual = false;
                        listForUpdatingWithGroupId.ListOfGroupId.Clear();
                        listForUpdatingWithGroupId.UpdateSource.Clear();
                        listForUpdatingWithGroupId.ListOfGroupId.Clear();
                        break;
                    }
                    if (listOfWorkerLists[j].Count() == listOfWorkerListsFromLocalDB[j].Count())
                        for (int i = 0; i < listOfWorkerLists[j].Count(); i++)
                        {
                            if (listOfWorkerLists[j][i].FullName != listOfWorkerListsFromLocalDB[j][i].FullName)
                            {
                                if (!listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Contains(listOfWorkerListsFromLocalDB[j]))
                                {
                                    listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Add(listOfWorkerListsFromLocalDB[j]);
                                    listForUpdatingWithGroupId.UpdateSource.Add(listOfWorkerLists[j]);
                                    clientForUpdate = listWithClientsFromWEB.FirstOrDefault(x => x.ClientID == listOfWorkerListsFromLocalDB[j][0].ClientID);
                                    listForUpdatingWithGroupId.ListOfGroupId.Add(clientForUpdate.PersonGroupID);
                                    listsAreEqual = false;
                                }
                            }
                            if (!listOfWorkerLists[j][i].Avatar.SequenceEqual(listOfWorkerListsFromLocalDB[j][i].Avatar))
                            {
                                if (!listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Contains(listOfWorkerListsFromLocalDB[j]))
                                {
                                    listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Add(listOfWorkerListsFromLocalDB[j]);
                                    listForUpdatingWithGroupId.UpdateSource.Add(listOfWorkerLists[j]);
                                    clientForUpdate = listWithClientsFromWEB.FirstOrDefault(x => x.ClientID == listOfWorkerListsFromLocalDB[j][0].ClientID);
                                    listForUpdatingWithGroupId.ListOfGroupId.Add(clientForUpdate.PersonGroupID);
                                    listsAreEqual = false;
                                }
                            }
                            if (listOfWorkerLists[j][i].ClientID != listOfWorkerListsFromLocalDB[j][i].ClientID)
                            {
                                if (!listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Contains(listOfWorkerListsFromLocalDB[j]))
                                {
                                    listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Add(listOfWorkerListsFromLocalDB[j]);
                                    listForUpdatingWithGroupId.UpdateSource.Add(listOfWorkerLists[j]);
                                    clientForUpdate = listWithClientsFromWEB.FirstOrDefault(x => x.ClientID == listOfWorkerListsFromLocalDB[j][0].ClientID);
                                    listForUpdatingWithGroupId.ListOfGroupId.Add(clientForUpdate.PersonGroupID);
                                    listsAreEqual = false;
                                }
                            }
                        }
                    else
                    {
                        listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Add(listOfWorkerListsFromLocalDB[j]);
                        listForUpdatingWithGroupId.UpdateSource.Add(listOfWorkerLists[j]);
                        clientForUpdate = listWithClientsFromWEB.FirstOrDefault(x => x.ClientID == listOfWorkerListsFromLocalDB[j][0].ClientID);
                        listForUpdatingWithGroupId.ListOfGroupId.Add(clientForUpdate.PersonGroupID);
                        listsAreEqual = false;
                    }
                }
            else
                listsAreEqual = false;

            if (listsAreEqual)
            {   //case when groups on the API are equal to the lists from WEB
                listWithClientsFromWEB.Clear();
                goto BeginningOfInfiniteLoop;
            }
            else
            {   //case when client count was equal but one or many clients have been changed in
                //WEB and needs to change them in the local DB(aka API)
                if (listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Count() != 0)
                {
                    for (int i = 0; i < listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Count(); i++)
                    {                       
                        DeletePersonGroup(client, listForUpdatingWithGroupId.ListOfGroupId[i]).Wait();
                        CreateAndTrainWorkersPersonGroup(client, listForUpdatingWithGroupId.UpdateSource[i], listForUpdatingWithGroupId.ListOfGroupId[i]).Wait();

                    }
                    listForUpdatingWithGroupId.UpdateSource.Clear();
                    listForUpdatingWithGroupId.ListOfGroupId.Clear();
                    listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Clear();
                    listOfWorkerListsFromLocalDB = listOfWorkerLists;
                    clientsInDB.Clear();
                    for (int i = 0; i < listWithClientsFromWEB.Count(); i++)
                    {
                        clientsInDB.Add(listWithClientsFromWEB[i]);
                    }
                    DeleteClientsAndWorkersFromLocalDB();
                    AddClientsToLocalDB(listWithClientsFromWEB).Wait();
                    AddWorkersToLocalDB(listOfWorkerLists).Wait();
                    listWithClientsFromWEB.Clear();
                    listsAreEqual = true;
                }
                else
                {   //case when client count wasn't equal
                    DeletePersonGroups(client, clientsInDB).Wait();
                    CreateAndTrainWorkersPersonGroups(client, listOfWorkerLists, listWithClientsFromWEB).Wait();

                    listOfWorkerListsFromLocalDB = listOfWorkerLists;
                    clientsInDB.Clear();
                    for (int i = 0; i < listWithClientsFromWEB.Count(); i++)
                    {
                        clientsInDB.Add(listWithClientsFromWEB[i]);
                    }
                    DeleteClientsAndWorkersFromLocalDB();
                    AddClientsToLocalDB(listWithClientsFromWEB).Wait();
                    AddWorkersToLocalDB(listOfWorkerLists).Wait();
                    listWithClientsFromWEB.Clear();
                    listsAreEqual = true;
                }
            }

            //Here ends part, which check Local DB and WEB DB and update Local DB And API if needed
            BeginningOfInfiniteLoop:
            List<List<WorkersForProcessing>> templistOfWorkerLists = new List<List<WorkersForProcessing>>();

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
                        Thread.Sleep((int)(60000));
                        string _inputFilePath = inputFilePath + file.Name;

                        // load input image
                        Array2D<RgbPixel> img = Dlib.LoadImage<RgbPixel>(_inputFilePath);

                        // find all faces in the image
                        Rectangle[] faces = fd.Operator(img);
                        // if search was succesfull then starting work with Microsoft Azure API
                        if (faces.Length != 0)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Picture " + file.Name + " have faces(according to the DlibDotNetNative library), sending data to Microsoft Azure API");
                            templistOfWorkerLists = GetWorkersFromWebApplicationInDifferentListsDividedByClientID(databaseConnString);

                            foreach (var workerList in templistOfWorkerLists)
                            {
                                Client newClient = new Client();
                                newClient.PersonGroupID = Guid.NewGuid().ToString();
                                newClient.ClientID = workerList[0].ClientID;
                                listWithClientsFromWEB.Add(newClient);
                            }

                            // Check if local DB have old Clients, and if it does,
                            // then thansfer PersonGroupID(API) to the Clients in the new list
                            if ((listWithClientsFromWEB.Count() != 0) && (clientsInDB.Count() != 0))
                            {
                                if (listWithClientsFromWEB.Count() >= clientsInDB.Count())
                                    for (int i = 0; i < clientsInDB.Count(); i++)
                                    {
                                        if (clientsInDB[i].ClientID == listWithClientsFromWEB[i].ClientID)
                                        {
                                            listWithClientsFromWEB[i].PersonGroupID = clientsInDB[i].PersonGroupID;
                                        }
                                    }
                                else
                                    for (int i = 0; i < listWithClientsFromWEB.Count(); i++)
                                    {
                                        if (listWithClientsFromWEB[i].ClientID == clientsInDB[i].ClientID)
                                        {
                                            listWithClientsFromWEB[i].PersonGroupID = clientsInDB[i].PersonGroupID;
                                        }
                                    }
                            }

                            listsAreEqual = true;

                            if (listOfWorkerLists.Count() == templistOfWorkerLists.Count())
                                for (int j = 0; j < listOfWorkerLists.Count(); j++)
                                {
                                    if (listOfWorkerLists[j][0].ClientID != templistOfWorkerLists[j][0].ClientID)
                                    {
                                        listsAreEqual = false;
                                        listForUpdatingWithGroupId.ListOfGroupId.Clear();
                                        listForUpdatingWithGroupId.UpdateSource.Clear();
                                        listForUpdatingWithGroupId.ListOfGroupId.Clear();
                                        break;
                                    }
                                    if (listOfWorkerLists[j].Count() == templistOfWorkerLists[j].Count())
                                        for (int i = 0; i < listOfWorkerLists[j].Count(); i++)
                                        {
                                            if (listOfWorkerLists[j][i].FullName != templistOfWorkerLists[j][i].FullName)
                                            {
                                                if (!listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Contains(listOfWorkerLists[j]))
                                                {
                                                    listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Add(listOfWorkerLists[j]);
                                                    listForUpdatingWithGroupId.UpdateSource.Add(templistOfWorkerLists[j]);
                                                    clientForUpdate = listWithClientsFromWEB.FirstOrDefault(x => x.ClientID == listOfWorkerListsFromLocalDB[j][0].ClientID);
                                                    listForUpdatingWithGroupId.ListOfGroupId.Add(clientForUpdate.PersonGroupID);
                                                    listsAreEqual = false;
                                                }
                                            }
                                            if (!listOfWorkerLists[j][i].Avatar.SequenceEqual(templistOfWorkerLists[j][i].Avatar))
                                            {
                                                if (!listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Contains(listOfWorkerLists[j]))
                                                {
                                                    listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Add(listOfWorkerLists[j]);
                                                    listForUpdatingWithGroupId.UpdateSource.Add(templistOfWorkerLists[j]);
                                                    clientForUpdate = listWithClientsFromWEB.FirstOrDefault(x => x.ClientID == listOfWorkerListsFromLocalDB[j][0].ClientID);
                                                    listForUpdatingWithGroupId.ListOfGroupId.Add(clientForUpdate.PersonGroupID);
                                                    listsAreEqual = false;
                                                }
                                            }
                                            if (listOfWorkerLists[j][i].ClientID != templistOfWorkerLists[j][i].ClientID)
                                            {
                                                if (!listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Contains(listOfWorkerLists[j]))
                                                {
                                                    listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Add(listOfWorkerLists[j]);
                                                    listForUpdatingWithGroupId.UpdateSource.Add(templistOfWorkerLists[j]);
                                                    clientForUpdate = listWithClientsFromWEB.FirstOrDefault(x => x.ClientID == listOfWorkerListsFromLocalDB[j][0].ClientID);
                                                    listForUpdatingWithGroupId.ListOfGroupId.Add(clientForUpdate.PersonGroupID);
                                                    listsAreEqual = false;
                                                }
                                            }
                                        }
                                    else
                                    {
                                        listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Add(listOfWorkerLists[j]);
                                        listForUpdatingWithGroupId.UpdateSource.Add(templistOfWorkerLists[j]);
                                        clientForUpdate = listWithClientsFromWEB.FirstOrDefault(x => x.ClientID == listOfWorkerListsFromLocalDB[j][0].ClientID);
                                        listForUpdatingWithGroupId.ListOfGroupId.Add(clientForUpdate.PersonGroupID);
                                        listsAreEqual = false;
                                    }
                                }
                            else
                                listsAreEqual = false;

                            CheckIfPersonGroupsIsOutdated:
                            if (listsAreEqual)
                            {   //case when groups on the API are equal to the lists from local and WEB DB
                                // Get ClientID and CameraID from XML
                                (string, string) infoFromXML = GetInfoFromXML(file.Name, inputFilePath);
                                for (int i = 0; i < listOfWorkerLists.Count(); i++)
                                {
                                    if (listOfWorkerLists[i][0].ClientID == infoFromXML.Item1)
                                    {
                                        FindFacesWithAPIIdentifyThemAndAddInDB(_inputFilePath, subscriptionKey, uriBase, file.Name, client, inputFilePath, clientsInDB[i].PersonGroupID).Wait();
                                        file.Delete();
                                    }
                                }
                                listWithClientsFromWEB.Clear();
                            }
                            else
                            {   //case when client count was equal but one or many clients have been changed
                                if (listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Count() != 0)
                                {
                                    for (int i = 0; i < listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Count(); i++)
                                    {
                                        DeletePersonGroup(client, listForUpdatingWithGroupId.ListOfGroupId[i]).Wait();
                                        CreateAndTrainWorkersPersonGroup(client, listForUpdatingWithGroupId.UpdateSource[i], listForUpdatingWithGroupId.ListOfGroupId[i]).Wait();
                                    }
                                    listForUpdatingWithGroupId.UpdateSource.Clear();
                                    listForUpdatingWithGroupId.ListOfGroupId.Clear();
                                    listForUpdatingWithGroupId.ListOfWorkerGroupsForUpdating.Clear();
                                    listOfWorkerListsFromLocalDB = templistOfWorkerLists;
                                    listOfWorkerLists = templistOfWorkerLists;
                                    clientsInDB.Clear();
                                    for (int i = 0; i < listWithClientsFromWEB.Count(); i++)
                                    {
                                        clientsInDB.Add(listWithClientsFromWEB[i]);
                                    }
                                    DeleteClientsAndWorkersFromLocalDB();
                                    AddClientsToLocalDB(clientsInDB).Wait();
                                    AddWorkersToLocalDB(listOfWorkerListsFromLocalDB).Wait();
                                    listWithClientsFromWEB.Clear();
                                    listsAreEqual = true;
                                    goto CheckIfPersonGroupsIsOutdated;
                                }
                                else
                                {   //case when client count wasn't equal
                                    DeletePersonGroups(client, clientsInDB).Wait();
                                    CreateAndTrainWorkersPersonGroups(client, templistOfWorkerLists, listWithClientsFromWEB).Wait();
                                    listOfWorkerListsFromLocalDB = templistOfWorkerLists;
                                    listOfWorkerLists = templistOfWorkerLists;
                                    clientsInDB.Clear();
                                    for (int i = 0; i < listWithClientsFromWEB.Count(); i++)
                                    {
                                        clientsInDB.Add(listWithClientsFromWEB[i]);
                                    }
                                    DeleteClientsAndWorkersFromLocalDB();
                                    AddClientsToLocalDB(clientsInDB).Wait();
                                    AddWorkersToLocalDB(listOfWorkerListsFromLocalDB).Wait();
                                    listWithClientsFromWEB.Clear();
                                    listsAreEqual = true;
                                    goto CheckIfPersonGroupsIsOutdated;
                                }
                            }
                        }
                        else
                            file.Delete();

                        // After all work is done delete XML file with info about image set, received from client 
                        string xmlName = file.Name.Substring(0, file.Name.LastIndexOf('_')) + ".xml";
                        if (dir.GetFiles(file.Name.Substring(0, file.Name.LastIndexOf('_')) + "*.jpeg").Length <= 0)
                            foreach (FileInfo xmlFile in dir.GetFiles(xmlName))
                                xmlFile.Delete();
                    }
                }
            }
            //Person group creation for one Client
            static async Task CreateAndTrainWorkersPersonGroup(IFaceClient faceClient, List<WorkersForProcessing> listOfWorkersForProcessing, string personGroupId)
            {
                // Create a person group. 
                await CreatePersonGroup(faceClient, personGroupId);

                // The person group person creation.
                await CreatePersonsWithFacesInPersonGroup(listOfWorkersForProcessing, faceClient, personGroupId);

                // Train Person group
                await TrainPersonGroup(faceClient, personGroupId);
            }

            //Person groups creation for all Clients
            static async Task CreateAndTrainWorkersPersonGroups(IFaceClient faceClient, List<List<WorkersForProcessing>> listOfWorkersListsForProcessing, List<Client> clients)
            {
                // Create a person group. 
                for (int i = 0; i < clients.Count(); i++)
                {
                    await CreatePersonGroup(faceClient, clients[i].PersonGroupID);

                    // The person group person creation.
                    await CreatePersonsWithFacesInPersonGroup(listOfWorkersListsForProcessing[i], faceClient, clients[i].PersonGroupID);

                    // Train Person group
                    await TrainPersonGroup(faceClient, clients[i].PersonGroupID);
                }
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

        static async void GetTheListOfPersonGroupsFromAPIAndDeleteThem(IFaceClient faceClient)
        {
            IList<PersonGroup> listOfPersonGroups = await faceClient.PersonGroup.ListAsync();
            Console.WriteLine("Person Groups deletion...");
            foreach(PersonGroup group in listOfPersonGroups)
            {
                await faceClient.PersonGroup.DeleteAsync(group.PersonGroupId);
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
        static List<List<WorkersForProcessing>> GroupUpWorkersFromLocalDBInDifferentListsDividedByClientID(List<DatabaseInfoAboutWorker> workersFromLocalDB)
        {
            // Change workersFromLocalDB format from List<> to List<List<>>
            var allWorkerListForProcessing = new List<WorkersForProcessing>();
            for (int i = 0; i < workersFromLocalDB.Count; i++)
            {
                var workerForProcessing = new WorkersForProcessing();
                workerForProcessing.FullName = workersFromLocalDB[i].FullName;
                workerForProcessing.Avatar = workersFromLocalDB[i].Avatar;
                workerForProcessing.ClientID = workersFromLocalDB[i].ClientID;
                allWorkerListForProcessing.Add(workerForProcessing);
            }

            List<string> listWithClientIDs = new List<string>();
            foreach (WorkersForProcessing worker in allWorkerListForProcessing)
            {
                if (!listWithClientIDs.Contains(worker.ClientID))
                {
                    listWithClientIDs.Add(worker.ClientID);
                }
            }

            var listOfWorkersListsByClientID = new List<List<WorkersForProcessing>>();
            foreach (string clientID in listWithClientIDs)
            {
                List<WorkersForProcessing> listWithClientID = new List<WorkersForProcessing>();
                foreach (WorkersForProcessing worker in allWorkerListForProcessing)
                {
                    if (worker.ClientID == clientID)
                    {
                        listWithClientID.Add(worker);
                    }
                }
                listOfWorkersListsByClientID.Add(listWithClientID);
            }
            return listOfWorkersListsByClientID;
        }
        static List<List<WorkersForProcessing>> GetWorkersFromWebApplicationInDifferentListsDividedByClientID(string databaseConnString)
        {
            // GetWorkersFromWebApplication               
            // Create a different lists with all workers from db.                   
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
            var allWorkerListForProcessing = new List<WorkersForProcessing>();
            for (int i = 0; i < workersList.Count; i++)
            {
                var workerForProcessing = new WorkersForProcessing();
                workerForProcessing.FullName = workersList[i].FullName;
                workerForProcessing.Avatar = workersList[i].Avatar;
                workerForProcessing.ClientID = workersList[i].ClientID;
                allWorkerListForProcessing.Add(workerForProcessing);
            }

            List<string> listWithClientIDs = new List<string>();
            foreach (WorkersForProcessing worker in allWorkerListForProcessing)
            {
                if (!listWithClientIDs.Contains(worker.ClientID))
                {
                    listWithClientIDs.Add(worker.ClientID);
                }
            }

            var listOfWorkersListsByClientID = new List<List<WorkersForProcessing>>();
            foreach (string clientID in listWithClientIDs)
            {
                List<WorkersForProcessing> listWithClientID = new List<WorkersForProcessing>();
                foreach (WorkersForProcessing worker in allWorkerListForProcessing)
                {
                    if (worker.ClientID == clientID)
                    {
                        listWithClientID.Add(worker);
                    }
                }
                listOfWorkersListsByClientID.Add(listWithClientID);
            }
            return listOfWorkersListsByClientID;
        }
        static async Task CreatePersonGroup(IFaceClient faceClient, string personGroupId)
        {
            // Create a person group. 
            Console.WriteLine();
            Console.WriteLine("Person group creation and training");
            Console.WriteLine($"Create a person group ({personGroupId}).");
            await faceClient.PersonGroup.CreateAsync(personGroupId, personGroupId, null, RecognitionModel.Recognition03);
        }
        static async Task CreatePersonsWithFacesInPersonGroup(List<WorkersForProcessing> workerListForProcessing, IFaceClient faceClient, string personGroupId)
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
        static async Task AddDataFromResponseToDB(IList<InfoAboutImage> infoAboutImage, (string, string) infoFromXML, string fileName, string inputFilePath)
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
        static async Task AddClientsToLocalDB(List<Client> clients)
        { 
            // Listing each element from Client list and transfer data to the database.
            Console.WriteLine("\nAdding clients to local DB\n");
            for (int i = 0; i < clients.Count; i++)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    Client newClient = new Client
                    {
                        ClientID = clients[i].ClientID,
                        PersonGroupID = clients[i].PersonGroupID,
                    };

                    await db.AddAsync(newClient);
                    await db.SaveChangesAsync();
                }
            }
        }
        static async Task AddWorkersToLocalDB(List<List<WorkersForProcessing>> listOfWorkersLists)
        {
            // Listing each element from worker lists and transfer data to the database.
            Console.WriteLine("\nAdding workers from Web to local DB\n");
            for (int i = 0; i < listOfWorkersLists.Count; i++)
            {
                for(int j = 0; j< listOfWorkersLists[i].Count(); j++)
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        DatabaseInfoAboutWorker databaseInfoAboutWorker = new DatabaseInfoAboutWorker
                        {
                            FullName = listOfWorkersLists[i][j].FullName,
                            ClientID = listOfWorkersLists[i][j].ClientID,
                            Avatar = listOfWorkersLists[i][j].Avatar
                        };

                        await db.AddAsync(databaseInfoAboutWorker);
                        await db.SaveChangesAsync();
                    }
                }
            }
        }
        static List<DatabaseInfoAboutWorker> GetWorkersFromLocalDB()
        {
            Console.WriteLine("\nGetting worker list from local DB\n");
            List<DatabaseInfoAboutWorker> workers = new List<DatabaseInfoAboutWorker>();
            using (ApplicationContext db = new ApplicationContext())
            {
                workers = db.WorkersInAPI.ToList();
            }
            return workers;
        }
        static List<Client> GetClientListFromLocalDB()
        {
            Console.WriteLine("\nGetting the list of Clients from DB\n");
            List<Client> clients = new List<Client>();
            using (ApplicationContext db = new ApplicationContext())
            {
                clients = db.Clients.ToList();
            }
            return clients;
        }
        static void DeleteClientsAndWorkersFromLocalDB()
        {
            Console.WriteLine("Deleting Clients and Workers from local DB");
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Database.ExecuteSqlRaw("TRUNCATE \"WorkersInAPI\"");
                db.Database.ExecuteSqlRaw("TRUNCATE \"Clients\"");
            } 
            Console.WriteLine();
        }
        static async Task DeletePersonGroup(IFaceClient faceClient, string personGroupId)
        {
            Console.WriteLine("Person group " + personGroupId + " deletion");
            await faceClient.PersonGroup.DeleteAsync(personGroupId);
            Console.WriteLine();
        }
        static async Task DeletePersonGroups(IFaceClient faceClient, List<Client> listOfClients)
        {
            foreach (var client in listOfClients)
            {
                Console.WriteLine("Person group " + client.PersonGroupID + " deletion");
                await faceClient.PersonGroup.DeleteAsync(client.PersonGroupID);
            }
            Console.WriteLine();
        }
        static byte[] GetImageAsByteArray(string inputFilePath)
        {
            // Returns the contents of the specified file as a byte array.
            using (FileStream fileStream =
                new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}