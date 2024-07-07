using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using SendEmailConsoleApp;

HttpClient wikiApiClient = new HttpClient();

WikiApi wikiApi = new WikiApi(wikiApiClient);

string currentDay = "";

var key = File.ReadAllLines("../openaiapikey.txt");

var ss = new AiService(key[0]);
// AiService.ClearSubjectsToSkip("SubjectsToSkip.txt");

var subjectToFocus = "Rome";
var currentSubjectSkip = File.ReadAllLines("SubjectsToSkip.txt");

var subjectsToSkipString = string.Join(", ", currentSubjectSkip);

var historyEvent = await ss.SendQuestion($"Tell me a historic event focusing around {subjectToFocus} IMPORTANT! Do not post an historical event regarding these subjects, otherwise we get duplicate events: {subjectsToSkipString}");

var subjectsToSkip = new string(historyEvent
    .SkipWhile(x => x != '(')
    .Skip(1)
    .TakeWhile(x => x != ')')
    .ToArray());

var skip = File.ReadAllLines("SubjectsToSkip.txt").ToList();
skip.Add(subjectsToSkip);
File.WriteAllLines("SubjectsToSkip.txt", skip);


//Hitta bild till 
//Man kan skicka in ett ämne som går i en vecka.
//Användare kan skicka in förslag
//Lista som uppdateras varje vecka lägg i ämne som skall skippas.
//Cleara subjectsto skip varje vecka vid nytt ämne.
while (true)
{
    Console.WriteLine("I loop");

    //Obs currentTime är formaterat såhär: 07, 11, 19.
    string currentTime = Utils.GetCurrentTime(false, true);


    //currentTime == "07" && 
    if (currentTime == "07" && currentDay != Utils.GetCurrentDate())
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

        var emails = EmailReader.getEmails();

        //för testning
        // string[] emails = { "henrik1995a@live.se" };

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