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

if (true)
{
    var subjectToFocus = "Svensks krigsindustri under 1600-2000";

    var bulletPoints = await aiService.InitWeek($"Subject to generate bullet points about: {subjectToFocus}");
    aiService.AddSubjectsToList("../AiSubjects.txt", bulletPoints.Split("*").ToList());

    var subjects = aiService.GetSubject("../AiSubjects.txt");

    aiService.ClearList("SubjectsToSkip.txt");
    List<string> weekSubjects = bulletPoints.Split('*')
                                 .Select(x => x.Trim())
                                 .Where(x => !string.IsNullOrEmpty(x))
                                 .ToList();
    aiService.AddSubjectsToList("SubjectsToSkip.txt", weekSubjects);
}

string subject = new string(aiService.GetSubject("../AiSubjects.txt")
                               .SkipWhile(x => !char.IsLetter(x))
                               .ToArray());
var story = await aiService.SendHistoryQuestion($"Berätta en historia om detta ämnet: {subject}.");

var prompt = await aiService.GetImagePrompt(story);

var imageUrl = await imageApi.TryGetImage(prompt);

AiGeneratedEvent aiGeneratedEvent = new AiGeneratedEvent(subject, story, imageUrl);

var currentDay = "";

while (true)
{
    Console.WriteLine("I loop");

    //Obs currentTime är formaterat såhär: 07, 11, 19.
    string currentTime = Utils.GetCurrentTime(false, true);


    // currentTime == "07" && currentDay != Utils.GetCurrentDate()
    if (true)
    {
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