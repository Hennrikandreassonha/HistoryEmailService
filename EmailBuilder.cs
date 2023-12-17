using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendEmailConsoleApp
{
  public class EmailBuilder
  {
    public EmailBuilder()
    {
    }
    public string GetEmailContent(SweUser swePerson, TodaysEvent todaysEvent,
    List<SweUser> moreSweBirths, ScraperObject scaperObject)
    {
      var date = Utils.GetCurrentDate();

      var firstArtikleFormatedEventText = FormatText(todaysEvent.FirstArticleExtract.ToString());
      var secondFormatedEventText = FormatText(todaysEvent.SecondArticleExtract.ToString());
      var formatedHeading = FormatText(todaysEvent.Heading);

      var formatedMoreSweBirths = FormatMoreSwePersons(moreSweBirths);
      var formatedPersonText = FormatText(swePerson.Text);

      var formatedScrapertext = FormatText(scaperObject.Text!);

      var sweColorYellow = "#ffcd00";
      var sweColorBlue = "#004b87";

      //<h3 style='color: red;'>Passar bra o skriva meddelanden i </h3>

      return $@"
            <html>
              <body>

                <div style='border: 1px solid {sweColorYellow}; background-color: {sweColorBlue}; padding: 0.5rem;''> 

                    <h1 style='color: {sweColorYellow}; padding: 0.5rem;'>
                      {scaperObject.Header} <br>
                    </h1>
                </div>

                <div style='border: 1px solid black; padding: 0.5rem;'>
                  {formatedScrapertext}
                  <img src='{scaperObject.PictureUrl}'/>
                  <br>
                  {scaperObject.PictureText}
                  <br>
                  <div>
                  {scaperObject.ReadMoreLink}
                  </div>
                </div>

                <div style='border: 1px solid {sweColorYellow}; background-color: {sweColorBlue};'> 
                    
                    <h1 style='color: {sweColorYellow}; padding: 0.5rem;'>
                      Dagens händelse ägde rum <br>
                       <p style= 'padding: 0.5rem; margin: 0px;'>{date} - {todaysEvent.Year}.</p>
                    </h1>

                </div>

                <div style='border: 1px solid black; padding: 0.5rem;'>
                  
                  <p>{formatedHeading}</p>

                  <hr>

                  <h3>Artiklar relaterade till händelsen:</h3>
                
                  <h2>{todaysEvent.FirstArticleTitle}</h2>
                  <p>{firstArtikleFormatedEventText}</p>
                  <img src='{todaysEvent.FirstArticleImageUrl}'/>
                  <p>Läs mer om artikel 1: <a href='{todaysEvent.FirstArticlePageUrl}'>Wikipedia</a></p>

                  <hr>

                  <h2 style='font-weight: bold;'>{todaysEvent.SecondArticleTitle}</h2>
                  {secondFormatedEventText}
                  <img src='{todaysEvent.SecondArticleImageUrl}'/>
                  <p>Läs mer om artikel 2: <a href='{todaysEvent.SecondArticlePageUrl}'>Wikipedia</a></p>

                </div>

                <div style='border: 1px solid {sweColorYellow}; background-color: {sweColorBlue};'>  
                
                    <h1 style= 'color: {sweColorYellow}; padding: 0.5rem;'>Dagens födelseperson född <br> 
                    {date} - {swePerson.BirthYear}.</h1>
                 
                </div>

                <div style='border: 1px solid black; padding: 0.5rem;'>
            
                  <p>{formatedPersonText}</p>
                  <img src='{swePerson.ImageUrl}'/>
                  <p>Läs mer om dagens födelseperson på: <a href='{swePerson.PageUrl}'>Wikipedia</a></p>


                </div>
                
                <div style='border: 1px solid {sweColorYellow}; background-color: {sweColorBlue};'> 

                  <h3 style= 'color: {sweColorYellow}; padding: 0.2rem;' 
                  >Fler svenskar födda {date}</h3>

                </div>

                <div style='border: 1px solid black; padding: 0.5rem;'>
                  
                  <p>Sorterat enligt födesleår (äldst först)</p>

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
        var splittedText = extract.Split('.').Select(s => s + ". ").ToList().Select(x => x.Trim()).Where(x => x != ".").ToList();
        List<string> joined = new();

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
          joinedAgain.Insert(0, "<p>").Insert(joinedAgain.Length - 1, "<p>");
          return joinedAgain;
        }
        for (int i = 0; i < toLoop.Count; i++)
        {
          formatedText += $@"</p> <p>";

          if (toLoop[i] != "")
            formatedText += $"{toLoop[i]}";
        }

        formatedText += "</p>";

        return formatedText;
      }
      catch (Exception e)
      {
        Utils.AddToErrorlog(e.Message);
        return "";
      }

    }
  }
}