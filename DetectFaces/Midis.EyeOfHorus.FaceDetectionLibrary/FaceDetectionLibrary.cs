using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using DlibDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Dlib = DlibDotNet.Dlib;

namespace Midis.EyeOfHorus.FaceDetectionLibrary
{
    public class FaceDetectionLibrary
    {
        [TypeConverter(typeof(JsonConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<InfoAboutImage>))]

        public class JsonConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
            {
                if (sourceType == typeof(string))
                {
                    return true;
                }
                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string)
                {
                    string s = value.ToString();
                    //s = s.Replace("\\", "");
                    InfoAboutImage f = JsonConvert.DeserializeObject<InfoAboutImage>(s);
                    return f;
                }
                return base.ConvertFrom(context, culture, value);
            }
        }

        public class NoTypeConverterJsonConverter<T> : JsonConverter
        {
            static readonly IContractResolver resolver = new NoTypeConverterContractResolver();

            class NoTypeConverterContractResolver : DefaultContractResolver
            {
                protected override JsonContract CreateContract(Type objectType)
                {
                    if (typeof(T).IsAssignableFrom(objectType))
                    {
                        var contract = this.CreateObjectContract(objectType);
                        contract.Converter = null; // Also null out the converter to prevent infinite recursion.
                        return contract;
                    }
                    return base.CreateContract(objectType);
                }
            }

            public bool CanConvert(Type objectType)
            {
                return typeof(T).IsAssignableFrom(objectType);
            }

            public object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return JsonSerializer.CreateDefault(new JsonSerializerSettings { ContractResolver = resolver }).Deserialize(reader, objectType);
            }

            public void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                JsonSerializer.CreateDefault(new JsonSerializerSettings { ContractResolver = resolver }).Serialize(writer, value);
            }
        }

        public class TestClass
        {
            public InfoAboutImage InfoAboutImage { get; set; }
            public static void Test()
            {
                var json = "{\"Foo\":{\"faceId\":true,\"faceRectangle\":false,}}"; // {"Foo":{"a":true,"b":false,"c":false}}

                var test = JsonConvert.DeserializeObject<TestClass>(json);
                Console.WriteLine(JsonConvert.SerializeObject(test, Formatting.Indented));

                var fooJson = JsonConvert.SerializeObject(test.InfoAboutImage);
                var foo2 = (InfoAboutImage)TypeDescriptor.GetConverter(typeof(InfoAboutImage)).ConvertFromString(fooJson);
                Console.WriteLine(JsonConvert.SerializeObject(foo2, Formatting.Indented));

                // This is what the JSON for TestClass would look like if Foo were serialized as a string:
                Console.WriteLine(JsonConvert.SerializeObject(new { Foo = JsonConvert.SerializeObject(foo2) }, Formatting.None)); // {"Foo":"{\"a\":true,\"b\":false,\"c\":false}"}
            }
        }

        public static void DetectFaces(string inputFilePath, string subscriptionKey, string uriBase)
        {
            // set up Dlib facedetector

            using (var fd = Dlib.GetFrontalFaceDetector())
            {
                // load input image
                Array2D<RgbPixel> img = Dlib.LoadImage<RgbPixel>(inputFilePath);

                // find all faces in the image
                Rectangle[] faces = fd.Operator(img);
                if (faces.Length != 0)
                {
                    Console.WriteLine("Picture have faces, sending data to Azure");

                    if (File.Exists(inputFilePath))
                    {
                        try
                        {
                            MakeAnalysisRequest(inputFilePath, subscriptionKey, uriBase);
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
                Dlib.SaveJpeg(img, "./Results/Output.jpg");
            }


            // Gets the analysis of the specified image by using the Face REST API.
            static async void MakeAnalysisRequest(string inputFilePath, string subscriptionKey, string uriBase)
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
                    response = await client.PostAsync(uri, content);

                    // Get the JSON response.
                    string contentDeserializeVersion = response.Content.ToString();
                    string contentString = await response.Content.ReadAsStringAsync();

                    // Display the JSON response.

                    //InfoAboutImage infoAboutImage = JsonSerializer.Deserialize<InfoAboutImage>(contentDeserializeVersion);

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