using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HistoryEmailService;

namespace SendEmailConsoleApp
{
  public class EmailBuilder
  {
    public SweUser TodaysSwePerson { get; set; }
    public TodaysEvent TodaysEvent { get; set; }
    public List<SweUser> MoreSweBirths { get; set; }
    public ScraperObject ScraperObject { get; set; }
    public AiGeneratedEvent AiGeneratedEvent { get; set; }
    public EmailBuilder(SweUser todaysSwePerson, TodaysEvent todaysEvent, List<SweUser> moreSweBirths, ScraperObject scraperObject, AiGeneratedEvent aiGeneratedEvent)
    {
      TodaysSwePerson = todaysSwePerson;
      TodaysEvent = todaysEvent;
      MoreSweBirths = moreSweBirths;
      ScraperObject = scraperObject;
      AiGeneratedEvent = aiGeneratedEvent;
    }
    public string GetEmailContent()
    {
      var date = Utils.GetCurrentDate();

      var firstArtikleFormatedEventText = FormatText(TodaysEvent.FirstArticleExtract.ToString());
      var secondFormatedEventText = FormatText(TodaysEvent.SecondArticleExtract.ToString());
      var formatedHeading = FormatText(TodaysEvent.Heading);

      var formatedMoreSweBirths = FormatMoreSwePersons(MoreSweBirths);
      var formatedPersonText = FormatText(TodaysSwePerson.Text);

      var sweColorYellow = "#ffcd00";
      var sweColorBlue = "#004b87";
      var textCenter = "text-align: center;";
      var flexAlignCenter = "display: flex; justify-content: center;";
      //<h3 style='color: red;'>Passar bra o skriva meddelanden i </h3>
      //Finns lite text längst ned om man vill skriva meddelande tex välkomna nya premunranter

      // var htmlContent = File.ReadAllText("./mail.html");
      Console.WriteLine("");
      return $@"
            <html>
              <body style='max-width: 600px''>
                    <div style='border: 1px solid {sweColorYellow}; background-color: {sweColorBlue}; padding: 0.5rem;'>
                      <h4 style=' color: {sweColorYellow};'>
                        Veckans ämne: {AiGeneratedEvent.WeeklySubject}
                        <br>
                        Dag {(DateTime.Now.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)DateTime.Now.DayOfWeek)}/7
                      </h4>
                      <h1 style=' color: {sweColorYellow}; padding: 0.5rem; {textCenter}'>
                          {AiGeneratedEvent.Subject} <br>
                      </h1>
                    </div>

                {FormatText(AiGeneratedEvent.Story)}

                <div style='width: 100%; 'border: 1px solid black; padding: 0.5rem;'>
                  <div style='display: flex;'>
                    <img src='{AiGeneratedEvent.Images[0].Key}' width='50%'>
                    <img src='{AiGeneratedEvent.Images[1].Key}' width='50%'>
                  </div>
                  <br>
                  <div style='width:50%; display:inline-block;'>
                    {AiGeneratedEvent.Images[0].Value}
                   </div>
                  <div style='display:inline-block;'>
                      {AiGeneratedEvent.Images[1].Value}
                  </div> 
                </div>

                <div style='border: 1px solid {sweColorYellow}; background-color: {sweColorBlue}; padding: 0.5rem;'> 

                    <h1 style='color: {sweColorYellow}; padding: 0.5rem; {textCenter}'>
                      {ScraperObject.Header} <br>
                    </h1>
                </div>

                <div style='border: 1px solid black; padding: 0.5rem;'>
                  {ScraperObject.Text}
                  <img src='{ScraperObject.PictureUrl}'/>
                  <br>
                  {ScraperObject.PictureText}
                  <br>
                  <div>
                  {ScraperObject.TermsInlineExtraInfo}  
                </div>
                </div>

                <div style='border: 1px solid {sweColorYellow}; background-color: {sweColorBlue};'> 
                    
                    <h1 style='color: {sweColorYellow}; padding: 0.5rem; {textCenter}'>
                      Dagens händelse ägde rum <br>
                       <p style= 'padding: 0.5rem; margin: 0px;'>{date} - {TodaysEvent.Year}.</p>
                    </h1>

                </div>

                <div style='border: 1px solid black; padding: 0.5rem;'>
                  
                  <p>{formatedHeading}</p>

                  <hr>

                  <h3>Artiklar relaterade till händelsen:</h3>
                
                  <h2>{TodaysEvent.FirstArticleTitle}</h2>
                  <p>{firstArtikleFormatedEventText}</p>
                  <img src='{TodaysEvent.FirstArticleImageUrl}'/>
                  <p>Läs mer om artikel 1: <a href='{TodaysEvent.FirstArticlePageUrl}'>Wikipedia</a></p>

                  <hr>

                  <h2 style='font-weight: bold;'>{TodaysEvent.SecondArticleTitle}</h2>
                  {secondFormatedEventText}
                  <img src='{TodaysEvent.SecondArticleImageUrl}'/>
                  <p>Läs mer om artikel 2: <a href='{TodaysEvent.SecondArticlePageUrl}'>Wikipedia</a></p>

                </div>

                <div style='border: 1px solid {sweColorYellow}; background-color: {sweColorBlue};'>  
                
                    <h1 style= 'color: {sweColorYellow}; padding: 0.5rem; {textCenter}'>Dagens födelseperson född <br> 
                    {date} - {TodaysSwePerson.BirthYear}.</h1>
                 
                </div>

                <div style='border: 1px solid black; padding: 0.5rem;'>
            
                  <p>{formatedPersonText}</p>
                  <img src='{TodaysSwePerson.ImageUrl}'/>
                  <p>Läs mer om dagens födelseperson på: <a href='{TodaysSwePerson.PageUrl}'>Wikipedia</a></p>


                </div>
                
                <div style='border: 1px solid {sweColorYellow}; background-color: {sweColorBlue};'> 

                  <h3 style= 'color: {sweColorYellow}; padding: 0.2rem; {textCenter}' 
                  >Fler svenskar födda {date}</h3>

                </div>

                <div style='border: 1px solid black; padding: 0.5rem;'>
                  {formatedMoreSweBirths}
                </div>
              </body>
            </html>
            ";
    }
    public static string FormatMoreSwePersons(List<SweUser> moreSweBirths)
    {
      string formatedText = "";

      foreach (var item in moreSweBirths)
      {
        formatedText += "<div>";

        formatedText += $"<p> <a href='{item.PageUrl}'>{item.Name}</a>, {item.BirthYear}, {item.Heading} </p>";

        formatedText += "</div>";

      }

      return formatedText;
    }
    public static string FormatText(string extract)
    {
      try
      {
        // Ensure there is a space after every period (dot)
        var splittedText = extract.Split('.')
                                  .Select(s => s.Trim() + ". ")  // Add space after each period and trim each sentence
                                  .Where(x => x != ". ")         // Remove empty or single dot results
                                  .ToList();

        List<string> joined = new List<string>();

        for (int i = 0; i < splittedText.Count; i++)
        {
          var joinedd = "";

          if (i != 0 && i % 3 == 0)
          {
            for (int j = i - 3; j < i; j++)
            {
              joinedd += splittedText[j];
            }
          }
          joined.Add(joinedd);
        }

        string formatedText = "<p>";

        List<string> toLoop = new();
        if (joined.Any(x => !string.IsNullOrWhiteSpace(x.Trim())))
        {
          toLoop = joined;
        }
        else
        {
          var joinedAgain = string.Join(" ", splittedText);
          joinedAgain = $"<p>{joinedAgain}</p>"; // Properly wrap the joined text in paragraph tags
          return joinedAgain;
        }

        for (int i = 0; i < toLoop.Count; i++)
        {
          formatedText += "</p> <p>";

          if (!string.IsNullOrEmpty(toLoop[i]))
          {
            formatedText += $"{toLoop[i]}";
          }
        }

        formatedText += "</p>";

        return formatedText;
      }
      catch (Exception e)
      {
        if (e.StackTrace != null)
        {
          Utils.AddToErrorlog(e.StackTrace);
          Console.WriteLine(e.StackTrace);
        }
        return "";
      }
    }

  }
}