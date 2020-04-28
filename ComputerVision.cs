using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ComputerVision_Tutorial
{
    public class ComputerVision
    {
        public ComputerVisionClient Client { get; }

        public ComputerVision(IConfigurationRoot appConfig)
        {
            var cv = appConfig.GetSection("ComputerVision");
            var credential = new ComputerVisionCredential
            {
                SubscriptionKey = cv["SubscriptionKey"],
                EndpointUrl = cv["Endpoint"],
            };
            Client = Authenticate(credential);
        }

        /// <summary>
        /// Analize Image -url Image
        /// 
        /// Analyze url image. Extracts captions, categories, tags, objects, faces, racy/adult content,
        /// brands, celebrities, landmarks, color scheme, and image types.
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <returns></returns>
        public async Task<ImageAnalysis> AnalyzeImageUrl(string imageUrl)
        {
            Console.WriteLine("----------------------------------");
            Console.WriteLine("Analyze image - url");
            Console.WriteLine("");

            // Creating a list that defines the features to be extracted from the image.
            List<VisualFeatureTypes> features = new List<VisualFeatureTypes>
            {
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces,
                VisualFeatureTypes.Adult,
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Color,
                VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects,
            };
            return await Client.AnalyzeImageAsync(imageUrl, features);
        }

        public async Task<IList<TextRecognitionResult>> BatchReadFileUrl(string imageUrl)
        {
            Console.WriteLine("----------------------------------");
            Console.WriteLine("BATCH READ FILE - URL IMAGE");
            Console.WriteLine("");

            BatchReadFileHeaders textHeaders = await Client.BatchReadFileAsync(imageUrl);
            var ol =  textHeaders.OperationLocation;

            // Retrieve the URI where the recognizer text will be stored from the Operatio-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            string operationId = ol.Substring(ol.Length - numberOfCharsInOperationId);

            //Extract the text
            // Delay is between iterations and tries a maximum of 10 times.
            int i = 0;
            int maxRetries = 10;
            ReadOperationResult results;
            Console.WriteLine($"Extracting text from URL image {Path.GetFileName(imageUrl)}...");
            Console.WriteLine();

            do
            {
                results = await Client.GetReadOperationResultAsync(operationId);
                Console.WriteLine("Server status: {0}, waiting {1} seconds...", results.Status, i);
                await Task.Delay(1000);
                if (i == 9)
                {
                    Console.WriteLine("Server timed out");
                }
            }
            while ((results.Status == TextOperationStatusCodes.Running 
            || results.Status == TextOperationStatusCodes.NotStarted) 
            && i++ < maxRetries);

            Console.WriteLine();

            return results.RecognitionResults;
        }

        public ComputerVisionClient Authenticate(ComputerVisionCredential credential) 
            => new ComputerVisionClient(new ApiKeyServiceClientCredentials(credential.SubscriptionKey))
            {
                Endpoint = credential.EndpointUrl,
            };
    }
}
