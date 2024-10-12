using OpenAI.ObjectModels.RequestModels;
using OpenAI_API;
using OpenAI_API.Images;
namespace SendEmailConsoleApp
{
    public class AiService
    {
        public string ApiKey { get; set; }
        public OpenAI.Managers.OpenAIService ChatService { get; set; }
        private readonly OpenAIAPI _openAIAPI;
        public AiService()
        {
        }
        public AiService(string apiKey)
        {
            ApiKey = apiKey;
            //Lägga nyckeln i en fil utanför projektet
            ChatService = new OpenAI.Managers.OpenAIService(new OpenAI.OpenAiOptions()
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
        public async Task<string> InitWeek(string newWeekSubject)
        {
            var completionResult = await ChatService.ChatCompletion.CreateCompletion(
            new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                new("assistant", "You must tell 7 different titles about subjects that i can write about. Always respond in swedish. Please split the events by *"),
                new("user", newWeekSubject),
                },
                Model = OpenAI.ObjectModels.Models.Gpt_3_5_Turbo,
                Temperature = 1.0F,
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
        public async Task<string> SendQuestion(string message)
        {
            var completionResult = await ChatService.ChatCompletion.CreateCompletion(
            new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                new("assistant", "Berätta en intressant historia som är minst 500 karaktärer lång. Längst ned skall du också ha en länk till en wikipedia artikel angående historien. Splitta halva historien med \n"),
                new("user", message),
                },
                Model = OpenAI.ObjectModels.Models.Gpt_3_5_Turbo,
                Temperature = 0.35F,
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
        public void ClearList(string filePath)
        {
            var emptyList = new List<string>();
            File.WriteAllLines(filePath, emptyList);
        }
        public void AddSubjectsToList(string filePath, List<string> newSubjects)
        {
            var subjectSkips = File.ReadAllLines(filePath).ToList();
            subjectSkips.AddRange(newSubjects);
            File.WriteAllLines(filePath, subjectSkips);
        }

        public string GetSubject(string filePath)
        {
            var subjectSkips = File.ReadAllLines(filePath).ToList();
            string subject = subjectSkips.First();
            subjectSkips.RemoveAt(0);
            File.WriteAllLines(filePath, subjectSkips);
            return subject;
        }
    }
    public class ImageGenerationParams
    {
        public string ModelId { get; set;}
        public string Prompt { get; set; }
        public string OutputFormat { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Steps { get; set; }
        public int Guidance { get; set; }
    }
}