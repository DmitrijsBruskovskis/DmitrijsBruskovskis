using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Midis.EyeOfHorus.FaceDetectionLibrary;
using Npgsql;
using System;
using System.IO;

namespace Consol
{
    /// <summary>
    /// The main program class
    /// </summary>
    class Program
    {
        public static IConfigurationRoot configuration;

        static void Main(string[] args)
        {
            // Create service collection
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Create service provider
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            // Get data from appsettings.json
            string inputFilePath = configuration.GetSection("InputFilePath").Get<string>();
            string subscriptionKey = configuration.GetSection("SubscriptionKey").Get<string>();
            string uriBase = configuration.GetSection("UriBase").Get<string>();

            // Create FaceClient (Azure face API)
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(subscriptionKey)) { Endpoint = "https://midiseu.cognitiveservices.azure.com" };
            //string IMAGE_BASE_URL = "https://drive.google.com/drive/folders/1CelWdtWzC_dQptYLNz0rP6sXgrEXlNXr?usp=sharing/";
            //string IMAGE_BASE_URL = "C:/Projects/Git/DmitrijsBruskovskis/Midis.EyeOfHorus/Midis.EyeOfHorus.Server.Consol/bin/x64/Debug/netcoreapp3.1/Images/";
            string IMAGE_BASE_URL = "https://onedrive.live.com/?id=1F90A58E448F5E7A%21103&cid=1F90A58E448F5E7A/";
            //string IMAGE_BASE_URL = "https://csdx.blob.core.windows.net/resources/Face/Images/";

            // Library using
            //FaceDetectionLibrary.DetectFacesAsync(inputFilePath, subscriptionKey, uriBase);
            FaceDetectionLibrary.CreatePersonGroup(client, IMAGE_BASE_URL, RecognitionModel.Recognition03).Wait();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Build configuration
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("./appsettings.json", false)
                .Build();

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton(configuration);
        }
    }
}