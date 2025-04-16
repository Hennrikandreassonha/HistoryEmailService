using System;
using System.Text;
using System.Text.Json;
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
        public string WeeklySubject { get; set; }
        public List<KeyValuePair<string, string>> Images { get; set; }

        public AiGeneratedEvent(string subject, string story, string weeklySubject)
        {
            Subject = subject;
            Story = story;
            WeeklySubject = weeklySubject;
        }
        public bool IsComplete()
        {
            return Subject != null && Story != null && WeeklySubject != null && Images.Count != 0;
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
        public static string OpenAiKey = File.ReadAllLines("../HistoryEmailDocs/openaiapikey.txt")[0];
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
                model = "dall-e-3",
                type = "web_search_preview"
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

        public async Task<string> SearchWithWeb(string todaysSubject, string weeklySubject)
        {
            var prompt = $"Ge mig lite information om {todaysSubject}. Kontexten är: {weeklySubject}. Du ska skriva en berättelse på cirka 300 ord och även ange källor till detta. Källorna ska placeras längst ner (med länkar) och texten ska inte visa några källor. På Svenska";

            var options = new RestClientOptions("https://api.openai.com/v1/responses");
            var client = new RestClient(options);

            var request = new RestRequest("https://api.openai.com/v1/responses", Method.Post);
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", $"Bearer {OpenAiKey}");

            var tools = new[] { new { type = "web_search_preview" } };
            var requestBody = new
            {
                model = "gpt-4.1",
                tools,
                input = prompt
            };

            request.AddJsonBody(requestBody);

            var response = await client.ExecuteAsync(request);
            if (response == null || !response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
            {
                return "Failed to generate image: " + response?.ErrorMessage;
            }

            using JsonDocument doc = JsonDocument.Parse(response.Content);
            var outputArray = doc.RootElement.GetProperty("output");

            var resultBuilder = new StringBuilder();

            foreach (var item in outputArray.EnumerateArray())
            {
                if (item.GetProperty("type").GetString() == "message")
                {
                    var contentArray = item.GetProperty("content");

                    foreach (var contentItem in contentArray.EnumerateArray())
                    {
                        if (contentItem.GetProperty("type").GetString() == "output_text")
                        {
                            if (contentItem.TryGetProperty("text", out var textElement))
                            {
                                resultBuilder.AppendLine(textElement.GetString());
                            }
                            if (contentItem.TryGetProperty("annotations", out var annotations))
                            {
                                foreach (var annotation in annotations.EnumerateArray())
                                {
                                    string title = annotation.GetProperty("title").GetString();
                                    string url = annotation.GetProperty("url").GetString();

                                    resultBuilder.AppendLine($"\nSource: {title} — {url}");
                                }
                            }
                        }
                    }
                }
            }

            return resultBuilder.Length > 0 ? resultBuilder.ToString() : "No relevant content found.";
        }

    }
}
