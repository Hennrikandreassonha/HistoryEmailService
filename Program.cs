using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using HistoryEmailService;
using RestSharp;
using SendEmailConsoleApp;

HttpClient wikiApiClient = new HttpClient();

WikiApi wikiApi = new WikiApi(wikiApiClient);

var aiService = new AiService(File.ReadAllLines("../openaiapikey.txt")[0]);
var imageApi = new AiImageGenerator(aiService);

ListHandler listHandler = new();

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

    // Now you can compare using integers
    if (currentDay != Utils.GetCurrentDate())
    {
        //(int)DateTime.Now.DayOfWeek == 1
        //Ska egentligen börja på måndag
        var test = (int)DateTime.Now.DayOfWeek;
        if ((int)DateTime.Now.DayOfWeek == 0)
        {
            Console.WriteLine("Initar week");
            weeklySubject = await aiService.InitWeek();
        }
        AiGeneratedEvent aiGeneratedEvent = await aiService.GetTodaysEvent(weeklySubject);
        var prompt = await aiService.GetImagePrompt(aiGeneratedEvent.Story);

        aiGeneratedEvent.ImageUrl = await imageApi.TryGetImage(prompt);

        if (!aiGeneratedEvent.IsComplete())
        {
            Console.WriteLine("AiGenerated Event is not complete");
            Utils.AddToErrorlog("AiGenerated Event is not complete");
        }

        Console.WriteLine("Skickar mail");

        dynamic response = wikiApi.GetRandomTest();

        //Handling the swe birthday person.
        var allBirths = wikiApi.GetBirths(response);

        if (allBirths == null)
        {
            continue;
        }

        List<SweUser> sweBirths = wikiApi.GetSwePersons(allBirths);
        var swePerson = sweBirths.OrderByDescending(x => x.Text.Length).FirstOrDefault();

        List<SweUser> moreSweBirths = wikiApi.GetMoreSweBirths(allBirths);

        moreSweBirths = moreSweBirths.OrderBy(x => x.BirthYear).Where(x => x.PageUrl != swePerson.PageUrl).ToList();

        var allEvents = response.events;
        TodaysEvent todaysEvent = wikiApi.GetEvent(allEvents);

        ScraperObject scraperObject = new()
        {
            Url = "https://www.so-rummet.se/aret-runt"
        };

        await scraperObject.ScrapeAsync();

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