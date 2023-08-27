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

while (true)
{
    Console.WriteLine("I loop");

    //Obs currentTime är formaterat såhär: 07, 11, 19.
    string currentTime = Utils.GetCurrentTime(false, true);
    
    if (currentTime == "15" &&
    currentDay != Utils.GetCurrentDate())
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
        DagensSverigeApi sverigeApi = new DagensSverigeApi(wikiApiClient);

        // var subscribedEmails = sverigeApi.GetSubscribers();

        //Adding to emaillist.
        // foreach (var emails in subscribedEmails)
        // {
        //     try
        //     {
        //         message.To.Add(emails);
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //     }
        // }

        //Egentligen ska man hämta från ett api men sidan är inte live.
        message.To.Add("henrik1995a@live.se");

        message.To.Add("Karin.eh@hotmail.se");
        message.To.Add("henrik.kjellberg46@gmail.com");
        message.To.Add("andreasson6300@gmail.com");
        message.To.Add("Richard.jurmo.berg@gmail.com");
        message.To.Add("Victor.kaka@hotmail.com");
        message.To.Add("matilda.herngren@outlook.com");
        message.To.Add("m.johansson92@hotmail.com");


        EmailBuilder emailBuilder = new();
        message.Body = emailBuilder.GetEmailContent(swePerson, todaysEvent, moreSweBirths);

        message.IsBodyHtml = true;

        smtpClient.Send(message);
        //Jas
        currentDay = Utils.GetCurrentDate();
    }
    Thread.Sleep(5000);
}