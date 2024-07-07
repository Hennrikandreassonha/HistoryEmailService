using OpenAI.ObjectModels.RequestModels;

namespace SendEmailConsoleApp
{
    public class AiService
    {
        public string ApiKey { get; set; }
        public OpenAI.Managers.OpenAIService ChatService { get; set; }
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
        }
        public async Task<string> SendQuestion(string message)
        {
            var completionResult = await ChatService.ChatCompletion.CreateCompletion(
            new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                new("assistant", "You must tell a historical event. The event must have taken place before 1950. Write nothing extra except the historical event. Start the sentence with a title. Rules: Title: Start with the title of the text and after the title add /, Paragraph Division: For paragraph division use / Language: Always write in swedish, Length: Text should be atleast 500 characters long, divided into paragraph divisions like described before. Links: At the bottom of text refer to 2 wikipedia articles that are essential for the event, Main subject: At the end of the event give 3 essential words or subjects in () that are used to make sure we dont get same historical event twice. Note that these words should not be the main subject, New Subjects: It is important to skip subjects that are presented in prompt, do not "),
                new("user", message),
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
    public static void ClearSubjectsToSkip(string filePath)
    {
        var emptyList = new List<string>();
        File.WriteAllLines(filePath, emptyList);
    }
    public static void AddSubjectToSkip(string filePath, string newSubject)
    {
        var subjectSkips = File.ReadAllLines(filePath).ToList();
        subjectSkips.Add(newSubject);
        File.WriteAllLines(filePath, subjectSkips);
    }
    }
}