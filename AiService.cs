using System.Text.RegularExpressions;
using HistoryEmailService;
using OpenAI.ObjectModels.RequestModels;
using OpenAI_API;
using OpenAI_API.Images;
namespace SendEmailConsoleApp
{
    public class AiService
    {
        public string ApiKey { get; set; }
        public OpenAI.Managers.OpenAIService _chatService { get; set; }

        private readonly OpenAIAPI _openAIAPI;
        private readonly ListHandler _listHandler = new ListHandler();
        public string WeeklySubject { get; set; }
        public AiService()
        {
        }
        public AiService(string apiKey)
        {
            ApiKey = apiKey;
            //Lägga nyckeln i en fil utanför projektet
            _chatService = new OpenAI.Managers.OpenAIService(new OpenAI.OpenAiOptions()
            {
                ApiKey = ApiKey
            });
            _openAIAPI = new OpenAIAPI(apiKey);
        }
        public async Task<string> GenerateImageAsync(string prompt)
        {
            var imageResult = await _openAIAPI.ImageGenerations
            .CreateImageAsync(new ImageGenerationRequest
            {
                Prompt = prompt + "realistic",
            });
            return imageResult.Data[0].Url;
        }
        private async Task<string> GetCompletion(string systemMessage, string userMessage, float temperature)
        {
            var completionResult = await _chatService.ChatCompletion.CreateCompletion(
            new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                new("assistant", systemMessage),
                new("user", userMessage),
                },
                Model = OpenAI.ObjectModels.Models.Gpt_3_5_Turbo,
                Temperature = temperature,
                MaxTokens = 1000,
                N = 1
            });

            if (completionResult.Successful)
            {
                return completionResult.Choices[0].Message.Content;
            }
            else
            {
                if (completionResult.Error == null)
                {
                    throw new Exception("Unknown Error");
                }
                Console.WriteLine($"{completionResult.Error.Code}: {completionResult.Error.Message}");
            }

            return "";
        }
        private async Task<string> GetWeeklySubject(string filePath)
        {
            var allWeeklySubjects = File.ReadAllLines(filePath).ToList();
            string subject;
            if (allWeeklySubjects.Count == 0)
            {
                subject = await GenerateBackUpWeeklySubject();
                return subject;
            }
            Random random = new Random();
            var index = random.Next(allWeeklySubjects.Count);

            subject = allWeeklySubjects[index];
            _listHandler.RemoveFromList(filePath, index);

            return subject;
        }

        private async Task<string> GenerateBackUpWeeklySubject()
        {
            string systemMessage = $"Please give me a random history subject that i can write small stories about.";
            return await GetCompletion(systemMessage, "", 0.6F);
        }
        private async Task<string> GetBulletPoints(string newWeekSubject)
        {
            string systemMessage = $"Give me 7 events or subjects about {newWeekSubject}, these should be bullet points. The historical events should be pretty easy to write about. The essays are going to be 500 characters long. Always respond in Swedish. It is important that these subjects are historically correct. Pleas split stories by '\n'";
            return await GetCompletion(systemMessage, newWeekSubject, 0.6F);
        }
        public async Task<string> SendHistoryQuestion(string message)
        {
            string systemMessage = "Berätta en intressant historia som är minst 500 karaktärer lång. Längst ned skall du också ha en länk till en wikipedia artikel angående historien. Historien ska vara verklig och historisk korrekt. Det är viktigt att Splitta halva historien med \n";
            return await GetCompletion(systemMessage, message, 0.6F);
        }

        public async Task<string> GetImagePrompt(string message)
        {
            string systemMessage = $"Based on this text, generate a prompt suitable for generating an image about the subject. Include good details for the prompt like realistic high quality, and so on. Emphasize on correct colors and correct clothes for this time. The subject is: {message}";
            return await GetCompletion(systemMessage, message, 0.45F);
        }
        public async Task<string> GetReplacementPrompt(string message)
        {
            string systemMessage = $"This prompt did not work. Please generate a new one that i can use. Based on this text, generate a prompt suitable for generating an image about the subject. Include good details for the prompt like realistic high quality, and so on. Emphasize on correct colors and correct clothes for this time. The subject is: {message}";
            return await GetCompletion(systemMessage, message, 0.45F);
        }
        public async Task<string> InitWeek()
        {
            string weeklySubject;
            try
            {
                weeklySubject = await GetWeeklySubject("../AiWeeklySubjects.txt");
                WeeklySubject = weeklySubject;
                var bulletPoints = await GetBulletPoints($"Subject to generate bullet points about: {weeklySubject}");
                await AddBulletPointsToList(bulletPoints);

            }

            catch (Exception ex)
            {
                Utils.AddToErrorlog($"Could not init week EX {ex.Message} at Date: {DateTime.Now}");
                return "";
            }
            return weeklySubject;
        }
        public async Task<string> GetTodaysStory(string subject)
        {
            var story = await SendHistoryQuestion($"Berätta en historia om detta ämnet: {subject}.");
            return story;
        }
        public async Task AddBulletPointsToList(string bulletPoints)
        {
            var pointsSplitted = bulletPoints.Split('\n').ToList();
            while (pointsSplitted.Count != 7)
            {
                var bulletPointsRetry = await GetBulletPoints(WeeklySubject);
                pointsSplitted = bulletPointsRetry.Split('\n').ToList();
            }
            await _listHandler.AddSubjectsToList("../AiSubjects.txt", pointsSplitted);
        }
        public async Task<AiGeneratedEvent> GetTodaysEvent(string weeklySubject)
        {
            var todaysAiSubject = _listHandler.GetSubject("../AiSubjects.txt");
            var todaysAiStory = await GetTodaysStory(todaysAiSubject);

            return new AiGeneratedEvent(todaysAiSubject, todaysAiStory,weeklySubject);
        }
    }
    public class ImageGenerationParams
    {
        public string ModelId { get; set; }
        public string Prompt { get; set; }
        public string OutputFormat { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Steps { get; set; }
        public int Guidance { get; set; }
    }
}