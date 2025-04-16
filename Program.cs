using System.Net;
using System.Net.Mail;
using HistoryEmailService;
using SendEmailConsoleApp;

HttpClient wikiApiClient = new HttpClient();

WikiApi wikiApi = new WikiApi(wikiApiClient);
ListHandler _listHandler = new ListHandler();
GoogleImageApi googleImageApi = new();

AiImageGenerator aiImage = new();

var aiService = new AiService(File.ReadAllLines("../HistoryEmailDocs/openaiapikey.txt")[0]);
var imageApi = new AiImageGenerator(aiService);

ListHandler listHandler = new();
        Utils.AddToErrorlog($" errTest");


var weeklySubject = "";

var currentDay = "";

// weeklySubject = "Karl den 12e";
while (true)
{
    Console.WriteLine("I loop");

    //Obs currentTime är formaterat såhär: 07, 11, 19.
    string currentTime = Utils.GetCurrentTime(false, true);

    // currentTime == "07" && currentDay != Utils.GetCurrentDate()
    int hour = int.Parse(currentTime); // Convert currentTime to an integer
    Console.WriteLine("Skriver timme");

    // Now you can compare using integers
    if (currentDay != Utils.GetCurrentDate())
    {
        Console.WriteLine("Inne i if-satsen");

        dynamic response = wikiApi.GetRandomTest();
        var allBirths = wikiApi.GetBirths(response);

        if (allBirths == null)
        {
            Utils.AddToErrorlog("Could not get births.");
            continue;
        }
        //Denna ska användas: 
        //(int)DateTime.Now.DayOfWeek == 1
        //Ska egentligen börja på måndag
        //Måndag är 1, söndag 0
        var test = (int)DateTime.Now.DayOfWeek;
        ScraperObject scraperObject = new()
        {
            Url = "https://www.so-rummet.se/aret-runt"
        };

        await scraperObject.ScrapeAsync();
        if ((int)DateTime.Now.DayOfWeek == 3)
        {
            Console.WriteLine("Initar week");
            try
            {
                weeklySubject = await aiService.InitWeek();
            }
            catch (Exception ex)
            {
                Utils.AddToErrorlog($"Err {ex.Message}");
            }
        }
        Console.WriteLine("Starting AI event");
        var todaysSubject = _listHandler.GetSubject("../HistoryEmailDocs/AiSubjects.txt");
        AiGeneratedEvent aiGeneratedEvent = await aiService.GetTodaysEvent(weeklySubject, todaysSubject);
        aiGeneratedEvent.WeeklySubject = weeklySubject;
        string imageSearchPrompt = await aiService.GetImagePromptGoogle(todaysSubject);
        aiGeneratedEvent.Images = await googleImageApi.GetImageLinksWithClickAsync(imageSearchPrompt);

        aiGeneratedEvent.Story = await aiImage.SearchWithWeb(todaysSubject, weeklySubject);

        if (!aiGeneratedEvent.IsComplete())
        {
            Console.WriteLine("AiGenerated Event is not complete");
            Utils.AddToErrorlog("AiGenerated Event is not complete");
            continue;
        }

        List<SweUser> sweBirths = wikiApi.GetSwePersons(allBirths);
        var swePerson = sweBirths.OrderByDescending(x => x.Text.Length).FirstOrDefault();

        List<SweUser> moreSweBirths = wikiApi.GetMoreSweBirths(allBirths);

        moreSweBirths = moreSweBirths.OrderBy(x => x.BirthYear).Where(x => x.PageUrl != swePerson.PageUrl).ToList();

        var allEvents = response.events;
        TodaysEvent todaysEvent = wikiApi.GetEvent(allEvents);

        Console.WriteLine("Skickar mail");

        MailMessage message = new MailMessage();

        string fromEmail = "karinsvaxthus@gmail.com";
        string fromEmailPw = "uaittetofmedckvy";
        message.From = new MailAddress(fromEmail);
        message.Subject = $"Historieuppdatering {Utils.GetCurrentDate(true)}";

        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(fromEmail, fromEmailPw),
            EnableSsl = true,
        };

        // DagensSverigeApi sverigeApi = new DagensSverigeApi(wikiApiClient);

        // var emails = EmailReader.getEmails();

        //för testning
        string[] emails = { "henrik1995a@live.se" };

        foreach (var email in emails)
        {
            try
            {
                Console.WriteLine(email);

                if (email != null && email != "")
                {
                    message.Bcc.Add(email);
                }
                else
                {
                    Console.WriteLine("Skipping null email address.");
                }
            }
            catch (Exception e)
            {
                if (e.StackTrace != null)
                {
                    Utils.AddToErrorlog(e.StackTrace);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        EmailBuilder emailBuilder = new(swePerson, todaysEvent, moreSweBirths, scraperObject, aiGeneratedEvent);
        message.Body = emailBuilder.GetEmailContent();

        message.IsBodyHtml = true;

        smtpClient.Send(message);

        currentDay = Utils.GetCurrentDate();
    }

    Thread.Sleep(10000);
}