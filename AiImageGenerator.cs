using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;
using SendEmailConsoleApp;

namespace HistoryEmailService
{
    public class AiGeneratedEvent
    {
        public string Subject { get; set; }
        public string Story { get; set; }
        public string ImageUrl { get; set; }
        public string WeeklySubject { get; set; }
        public AiGeneratedEvent(string subject, string story, string weeklySubject)
        {
            Subject = subject;
            Story = story;
            WeeklySubject = weeklySubject;
        }
        public bool IsComplete()
        {
            return Subject != null && Story != null && WeeklySubject != null && ImageUrl != null;
        }
    }
    public class AiImageGenerator
    {
        private readonly AiService _aiService;
        public AiImageGenerator()
        {

        }
        public AiImageGenerator(AiService aiService)
        {
            _aiService = aiService;
        }
        public static string OpenAiKey = File.ReadAllLines("../openaiapikey.txt")[0];
        private async Task<string> GenerateImage(string prompt)
        {
            var options = new RestClientOptions("https://api.openai.com/v1/images/generations");
            var client = new RestClient(options);

            var request = new RestRequest("https://api.openai.com/v1/images/generations", Method.Post);

            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", $"Bearer {OpenAiKey}");

            var imageParams = new
            {
                prompt,
                n = 1,
                size = "1024x1024",
                model = "dall-e-3"
            };
            request.AddJsonBody(imageParams);

            var response = await client.PostAsync(request);
            if (response != null && response.IsSuccessful)
            {
                var jsonResponse = JObject.Parse(response.Content);

                var imageUrl = jsonResponse["data"][0]["url"].ToString();
                Console.WriteLine($"Generated Image URL: {imageUrl}");
                return imageUrl;
            }
            else
            {
                return "Failed to generate image: " + response.ErrorMessage;
            }
        }
        public async Task<string> TryGetImage(string prompt)
        {
            var success = false;
            string picUrl = "";
            while (success == false)
            {
                try
                {
                    picUrl = await GenerateImage(prompt);
                    success = true;
                }
                catch (Exception ex)
                {
                    var err = $"Image generation with Ex message: {ex.Message} And prompt:  {prompt} failed. Date : {DateTime.Now}";
                    Utils.AddToErrorlog(err);
                    prompt = await GetNewPrompt(prompt);
                }
            }
            return picUrl;
        }
        private async Task<string> GetNewPrompt(string oldPrompt)
        {
            var newPrompt = await _aiService.GetReplacementPrompt(oldPrompt);
            return newPrompt;
        }
        public async Task DownloadImage(string url, string filePath)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Get the image data as a byte array
                    byte[] imageData = await client.GetByteArrayAsync(url);

                    // Write the byte array to a file
                    await File.WriteAllBytesAsync(filePath, imageData);

                    Console.WriteLine("Image downloaded successfully to " + filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error downloading image: " + ex.Message);
                }
            }
        }
        public async Task<byte[]> GetImageBytes(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Get the image data as a byte array
                    byte[] imageData = await client.GetByteArrayAsync(url);

                    // Write the byte array to a file
                    return imageData;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error downloading image: " + ex.Message);
                    return Array.Empty<byte>();
                }
            }
        }
    }
}
