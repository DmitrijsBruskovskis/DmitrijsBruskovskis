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
            string vocabularyPath = configuration.GetSection("VocabularyPath").Get<string>();

            // Create FaceClient (Azure face API)
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(subscriptionKey)) { Endpoint = "https://midiseu.cognitiveservices.azure.com" };

            // Library using
            FaceDetectionLibrary.DetectFacesAsync(inputFilePath, subscriptionKey, uriBase, client, vocabularyPath);
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