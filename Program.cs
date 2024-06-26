﻿using System.Net;
using System.Net.Mail;
using SendEmailConsoleApp;

HttpClient wikiApiClient = new HttpClient();

WikiApi wikiApi = new WikiApi(wikiApiClient);

string currentDay = "";

while (true)
{
    Console.WriteLine("I loop");

    //Obs currentTime är formaterat såhär: 07, 11, 19.
    string currentTime = Utils.GetCurrentTime(false, true);

    //currentTime == "07" && 
    if (currentTime == "07" &&  currentDay != Utils.GetCurrentDate())
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

        //For the "more swede birth" section.
        List<SweUser> moreSweBirths = wikiApi.GetMoreSweBirths(allBirths);

        //Removing the featured sweperson.
        //The rest will be displayed below as "more swe birth persons".
        moreSweBirths = moreSweBirths.OrderBy(x => x.BirthYear).Where(x => x.PageUrl != swePerson.PageUrl).ToList();

        //Handling the event of the day
        //Använda events istället
        var allEvents = response.events;
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

    Thread.Sleep(5000);
}