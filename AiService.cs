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
            _chatService = new OpenAI.Managers.OpenAIService(new OpenAI.OpenAiOptions()
            {
                ApiKey = ApiKey,
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
        private async Task<string> GetCompletion(string systemMessage, string userMessage, float temperature, bool webSearch = false)
        {
            var completionResult = await _chatService.ChatCompletion.CreateCompletion(
new ChatCompletionCreateRequest
{
    Messages = new List<ChatMessage>
    {
        new ChatMessage("assistant", systemMessage),
        new ChatMessage("user", userMessage),
    },
    Model = OpenAI.ObjectModels.Models.Gpt_4,
    Temperature = temperature,
    N = 1,
    Tools = webSearch ? new List<ToolDefinition>
    {
        new ToolDefinition
        {
            Type = "function", // Tool type is "function"
            Function = new FunctionDefinition
            {
                Name = "web_search_preview",
            }
        }
    } : null
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
            // var index = random.Next(allWeeklySubjects.Count);

            subject = allWeeklySubjects.First();
            _listHandler.RemoveFromList(filePath, 0);

            return subject;
        }

        private async Task<string> GenerateBackUpWeeklySubject()
        {
            string systemMessage = $"Ge mig ett slumpmässigt historieämne som jag kan skriva små berättelser om. På svenska! Endast ämnet";
            return await GetCompletion(systemMessage, "", 0.15F);
        }
        private async Task<string> GetBulletPoints(string newWeekSubject)
        {
            string systemMessage = $"Give me 7 events or subjects about {newWeekSubject}, these should be bullet points. The essays are going to be 500 characters long. Always respond in Swedish. It is important that these subjects are historically correct. Pleas split stories by '\n'. Only return the heading";
            return await GetCompletion(systemMessage, newWeekSubject, 0.15F);
        }
        public async Task<string> SendHistoryQuestion(string message, string weeklySubject)
        {
            string systemMessage = $"Contexten för historian är: {weeklySubject}.  Skriv alltid på svenska. Du skall utgå från källor på nätet och refera till dessa längst ned med Källor: Det är viktigt att Splitta halva historien med \n";
            return await GetCompletion(systemMessage, message, 0.1F, true);
        }
        public async Task<string> ConfirmHistoryQuestion(string generatedText, string weeklySubject)
        {
            string systemMessage = "Du är en historisk granskare. Du kommer att få en text och ämnet som texten handlar om. Texten har också ett ämne som den tillhör, det är viktigt att den är korrekt kopplad till ämnet. Du svarar endast: KORREKT om texten är korrekt. Om den inte är korrekt svara 'Ej korrekt'.";
            string message = $"Är denna texten är historisk korrekt? Texten tillhör ämnet: {weeklySubject} TEXT: {generatedText}. Om den är korrekt svara endast :'KORREKT'.";
            return await GetCompletion(systemMessage, message, 0.15F);
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
        public async Task<string> GetImagePromptGoogle(string message)
        {
            string systemMessage = $"Based on this heading. Generate a search query that i can use to get images from google: {message}, Should be short for example 'Vikingarnas handelsresor till östra Europa och Mellanöstern' could be 'Vikingarnas handelsresor europa'. Just take the bullet words and in swedish please";
            return await GetCompletion(systemMessage, message, 0.45F);
        }
        public async Task<string> InitWeek()
        {
            string weeklySubject;
            try
            {
                weeklySubject = await GetWeeklySubject("../HistoryEmailDocs/AiWeeklySubjects.txt");
                WeeklySubject = weeklySubject;
                var bulletPoints = await GetBulletPoints($"Subject to generate bullet points about: {weeklySubject}");
                await AddBulletPointsToList(bulletPoints);

            }

            catch (Exception ex)
            {
                Utils.AddToErrorlog($"Could not init week EX {ex.Message} at Date: {DateTime.Now}");
                throw;
            }
            return weeklySubject;
        }
        public async Task<string> GetTodaysStory(string subject, string weeklySubject)
        {
            string textIsCorrect = "";
            var story = "";
            // while (textIsCorrect.ToUpper() != "KORREKT")
            // {
                story = await SendHistoryQuestion($"Information om detta ämnet: {subject}.", weeklySubject);
                // textIsCorrect = await ConfirmHistoryQuestion(story, weeklySubject);

                // if (textIsCorrect != "KORREKT")
                // {
                //     Utils.AddToErrorlog($"Text var inte korrekt {story}. Svaret från Confirm: {textIsCorrect} Datum :{DateTime.Now}");
                // }
            // }
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
            await _listHandler.AddSubjectsToList("../HistoryEmailDocs/AiSubjects.txt", pointsSplitted);
        }
        public async Task<AiGeneratedEvent> GetTodaysEvent(string weeklySubject, string todaysSubject)
        {
            var todaysAiStory = await GetTodaysStory(todaysSubject, weeklySubject);

            return new AiGeneratedEvent(todaysSubject, todaysAiStory, weeklySubject);
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