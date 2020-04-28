using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComputerVision_Tutorial
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        static void Main(string[] args)
        {
            var appConfig = ServicesProvider.GetServiceProvider("appsetting.json");

            if (appConfig is null)
            {
                Console.WriteLine("Missing or invalid appsettings.json... exiting");
                return;
            }

            try
            {
                RunAsync(appConfig).GetAwaiter().GetResult();
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            Console.WriteLine("Pess any key to exit");
            Console.ReadKey();
        }

        private static async Task RunAsync(IConfigurationRoot appConfig)
        {
            var cv = new ComputerVision(appConfig);

            Console.WriteLine("Provide an image url");
            string imgUrl = Console.ReadLine();
            var result = await cv.AnalyzeImageUrl(imgUrl);
            DisplayImageInformation(result);

            Console.WriteLine("Provide an image (url) with text");
            string txtimgurl = Console.ReadLine();
            var txtresult = await cv.BatchReadFileUrl(txtimgurl);
            DisplayImageTextInformation(txtresult);
        }

        private static void DisplayImageTextInformation(IList<TextRecognitionResult> txtresult)
        {
            foreach(var recResult in txtresult)
            {
                foreach(var line in recResult.Lines)
                {
                    Console.WriteLine(line.Text);
                }
            }
            Console.WriteLine();
        }

        private static void DisplayImageInformation(ImageAnalysis result)
        {
            DisplayValue(
                "Summary", 
                result.Description.Captions, 
                (caption) => $"{caption.Text} with confidence {caption.Confidence}");
            
            DisplayValue(
                "Categories:",
                result.Categories,
                (category) => $"{category.Name} with confidence {category.Score}");

            DisplayValue(
                "Tags:",
                result.Tags,
                (tag) => $"{tag.Name} {tag.Confidence}");

            DisplayValue(
                "Objects:",
                result.Objects,
                (item) => $"{item.ObjectProperty} with confidence {item.Confidence} at location " +
                $"{item.Rectangle.X}, {item.Rectangle.X + item.Rectangle.W}, " +
                $"{item.Rectangle.Y}, {item.Rectangle.Y + item.Rectangle.H}");

            DisplayValue(
                "Brands:",
                result.Brands,
                (brand) => $"Logo of {brand.Name} with confidence {brand.Confidence} at location " +
                $"{brand.Rectangle.X}, {brand.Rectangle.X + brand.Rectangle.W}, " +
                $"{brand.Rectangle.Y}, {brand.Rectangle.Y + brand.Rectangle.H}");

            DisplayValue(
                "Faces:",
                result.Faces,
                (face) => $"A {face.Gender} of age {face.Age} at location " +
                $"{face.FaceRectangle.Left}, {face.FaceRectangle.Top + face.FaceRectangle.Width}, " +
                $"{face.FaceRectangle.Top + face.FaceRectangle.Height}");

            foreach (var category in result.Categories)
            {
                // Celebrities in image if any
                if (category.Detail?.Celebrities != null)
                {
                    DisplayValue(
                        "Celebrities:",
                        category.Detail.Celebrities,
                        (celeb) => $"{celeb.Name} with a confidence {celeb.Confidence} at location " +
                        $"{celeb.FaceRectangle.Left}, {celeb.FaceRectangle.Top}" +
                        $"{celeb.FaceRectangle.Height}, {celeb.FaceRectangle.Width}");
                }

                // Landmark in image if any
                if (category.Detail?.Landmarks != null)
                {
                    DisplayValue(
                        "Landmarks:",
                        category.Detail.Landmarks,
                        (landmark) => $"{landmark.Name} with a confidence {landmark.Confidence}");
                }

            }

            // Adult or racy content, if any
            Console.WriteLine("Adult:");
            Console.WriteLine($"Has adult content: {result.Adult.IsAdultContent} with confidence {result.Adult.AdultScore}");
            Console.WriteLine($"Has racy content: {result.Adult.IsRacyContent} with confidence {result.Adult.RacyScore}");

            // Identify tthe color scheme
            Console.WriteLine("Color Scheme:");
            Console.WriteLine("Is black and white?:" + result.Color.IsBWImg);
            Console.WriteLine("Accent color: " + result.Color.AccentColor);
            Console.WriteLine("Dominant background color: " + result.Color.DominantColorBackground);
            Console.WriteLine("Dominant foreground color: " + result.Color.DominantColorForeground);
            Console.WriteLine("Dominant color: " + string.Join(",", result.Color.DominantColors));

            // Detect the image type
            Console.WriteLine("Image Type:");
            Console.WriteLine("Clip Art Type: " + result.ImageType.ClipArtType);
            Console.WriteLine("Line drawing type: " + result.ImageType.LineDrawingType);
        }

        private static void DisplayValue<Item>(string title, IList<Item> items, Func<Item, string> callback)
        {
            Console.WriteLine(title);
            foreach (var item in items) Console.WriteLine(callback(item) + "\n");
        }
    }
}
