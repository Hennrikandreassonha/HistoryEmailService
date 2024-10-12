using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace HistoryEmailService
{
    public class AiImageGenerator
    {
        public static string OpenAiKey = File.ReadAllLines("../openaiapikey.txt")[0];
        public async Task GenerateImage()
        {
            // OpenAI DALL-E API base URL
            var options = new RestClientOptions("https://api.openai.com/v1/images/generations");
            var client = new RestClient(options);

            // Creating the POST request
            var request = new RestRequest("https://api.openai.com/v1/images/generations", Method.Post);

            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", $"Bearer {OpenAiKey}");

            // Defining the parameters for the image generation
            var imageParams = new
            {
                prompt = "generate a picture of a soilder during 30 åriga kriget swedish with realstic clothes and weapons going into war",
                n = 1,
                size = "1024x1024",
                model = "dall-e-3"
            };

            // Adding the parameters as JSON body
            request.AddJsonBody(imageParams);

            // Sending the POST request and awaiting the response
            var response = await client.PostAsync(request);

            // Output the response (image URL or error message)
            if (response != null && response.IsSuccessful)
            {
                // Print the raw JSON response
                Console.WriteLine(response.Content);

                // Parse the JSON response using JObject from Newtonsoft.Json
                var jsonResponse = JObject.Parse(response.Content);

                // Extract the image URL from the response
                var imageUrl = jsonResponse["data"][0]["url"].ToString();

                // Print the image URL to the console
                Console.WriteLine($"Generated Image URL: {imageUrl}");

                // Optionally: Open the image in the browser
                // System.Diagnostics.Process.Start(new ProcessStartInfo(imageUrl) { UseShellExecute = true });
            }
            else
            {
                Console.WriteLine("Failed to generate image: " + response.ErrorMessage);
            }
        }
    }
}
