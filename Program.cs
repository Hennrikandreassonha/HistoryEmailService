using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using HistoryEmailService;
using RestSharp;
using SendEmailConsoleApp;

HttpClient wikiApiClient = new HttpClient();

WikiApi wikiApi = new WikiApi(wikiApiClient);

string currentDay = "";

var aiService = new AiService(File.ReadAllLines("../openaiapikey.txt")[0]);

var imageApi = new AiImageGenerator();
await imageApi.GenerateImage();

// // if (DateTime.Today.DayOfWeek == DayOfWeek.Monday)

System.Console.WriteLine();
if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
{

    var subjectToFocus = "Svensk industrihistoria";
    var subjects = aiService.GetSubject("../../AiSubjects.txt");

    // var bulletPoints = await aiService.InitWeek($"Give me 7 events or subjects about {subjectToFocus}, these should be bullet points. The historical events should be pretty easy to write about. The essays are going to be 500 characters long.");

    // aiService.ClearList("SubjectsToSkip.txt");
    // List<string> weekSubjects = bulletPoints.Split('*')
    //                              .Select(x => x.Trim())
    //                              .Where(x => !string.IsNullOrEmpty(x))
    //                              .ToList();
    // aiService.AddSubjectsToList("SubjectsToSkip.txt", weekSubjects);
}

string subject = aiService.GetSubject("AiSubjects.txt");
var story = await aiService.SendQuestion($"Berätta en historia om detta ämnet: {subject}.");
//Ändra denna sen till någon bättre
var ss = await aiService.GenerateImageAsync(subject);
Console.WriteLine("");
//Hitta bild till - Kanske med api?
//Man kan skicka in ett ämne som går i en vecka.
//Användare kan skicka in förslag
//Lista som uppdateras varje vecka lägg i ämne som skall skippas.
//Cleara subjectsto skip varje vecka vid nytt ämne.
//Om ingen skickat in ämne så kommer AI på en automatiskt.
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

        DagensSverigeApi sverigeApi = new DagensSverigeApi(wikiApiClient);

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
                    message.To.Add(email);
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

        EmailBuilder emailBuilder = new();
        message.Body = emailBuilder.GetEmailContent(swePerson, todaysEvent, moreSweBirths, scraperObject);

        message.IsBodyHtml = true;

        smtpClient.Send(message);

        currentDay = Utils.GetCurrentDate();
    }

    Thread.Sleep(10000);
}