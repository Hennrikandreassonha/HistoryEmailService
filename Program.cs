using System.Runtime.InteropServices;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendEmailConsoleApp;
using System.Security.Cryptography.X509Certificates;

HttpClient wikiApiClient = new HttpClient();

WikiApi wikiApi = new WikiApi(wikiApiClient);

string currentDay = "";

Console.WriteLine("ss");

while (true)
{
    Console.WriteLine("I loop");

    //Obs currentTime är formaterat såhär: 07, 11, 19.
    string currentTime = Utils.GetCurrentTime(false, true);
    //
    if (currentDay != Utils.GetCurrentDate())
    {
        Console.WriteLine("Skickar mail");

        dynamic response = wikiApi.GetRandomTest();

        //Handling the swe birthday person.
        var allBirths = wikiApi.GetBirths(response);

        List<SweUser> sweBirths = wikiApi.GetSwePersons(allBirths);
        var swePerson = sweBirths.OrderByDescending(x => x.Text.Length).FirstOrDefault();

        //For the "more swede birth" section.
        List<SweUser> moreSweBirths = wikiApi.GetMoreSweBirths(allBirths);

        //Removing the featured sweperson.
        //The rest will be displayed below as "more swe birth persons".
        moreSweBirths = moreSweBirths.OrderBy(x => x.BirthYear).Where(x => x.PageUrl != swePerson.PageUrl).ToList();

        //Handling the event of the day
        var allEvents = response.selected;
        TodaysEvent todaysEvent = wikiApi.GetEvent(allEvents);

        //Getting todaysarticle from Webscraper
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

        //Getting the subscribed users from WebsiteApi.
        //Site isnt live i get it from document instead.
        DagensSverigeApi sverigeApi = new DagensSverigeApi(wikiApiClient);

        var emails = EmailReader.getEmails();

        foreach (var email in emails)
        {
            try
            {
                Console.WriteLine(email);

                if (email != null)
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
                // Handle the exception, e.g., log it or take corrective action.
                Console.WriteLine($"Error adding email: {e.Message}");
            }
        }

        EmailBuilder emailBuilder = new();
        message.Body = emailBuilder.GetEmailContent(swePerson, todaysEvent, moreSweBirths, scraperObject);

        message.IsBodyHtml = true;

        smtpClient.Send(message);

        currentDay = Utils.GetCurrentDate();
    }

    Thread.Sleep(5000);
}