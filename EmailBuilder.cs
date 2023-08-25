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
    public string GetEmailContent(SweUser swePerson, TodaysEvent todaysEvent)
    {
      var date = Utils.GetCurrentDate();

      var formatedEventText = FormatText(todaysEvent.Extract);
      var secondFormatedEventText = FormatText(todaysEvent.SecondArticleExtract);

      var sweColorYellow = "#ffcd00";
      var sweColorBlue = "#004b87";


      return $@"
            <html>
              <body>
  
                <div style='border: 1px solid {sweColorYellow}; background-color: {sweColorBlue};'>  
                    
                    <h1 style='color: {sweColorYellow}; padding: 0.5rem;'>
                      Dagens händelse ägde rum <br>
                       {date} - {todaysEvent.Year}.
                    </h1>

                </div>

                <div style='border: 1px solid black; padding: 0.5rem;'>
                  
                  <h2>{todaysEvent.Heading}</h2>
                  {formatedEventText}
                  <img src='{todaysEvent.ImageUrl}'/>
                  
                  <p>Läs mer om dagens händelse på: <a href='{todaysEvent.PageUrl}'>Wikipedia</a></p>

                  <hr>

                  <h2>Dagens relaterade artikel</h2>
                  <h2>{todaysEvent.SecondArticleTitle}</h2>
                  {secondFormatedEventText}
                  <img src='{todaysEvent.SecondArticleImageUrl}'/>
                  <p>Läs mer om dagens relaterade artikel på: <a href='{todaysEvent.SecondArticlePageUrl}'>Wikipedia</a></p>

                </div>

                <div style='border: 1px solid {sweColorYellow}; background-color: {sweColorBlue};'>  
                
                    <h1 style= 'color: {sweColorYellow}; padding: 0.5rem;'>Dagens födelseperson föddes <br> 
                    {date} - {swePerson.BirthYear}.</h1>
                 
                </div>

                <div style='border: 1px solid black; padding: 0.5rem;'>
            
                  <p>{swePerson.Text}</p>
                  <img src='{swePerson.ImageUrl}'/>
                  <p>Läs mer om dagens födelseperson på: <a href='{swePerson.PageUrl}'>Wikipedia</a></p>
                  
                </div>
              </body>
            </html>
            ";
    }
    public static string FormatText(string extract)
    {
      var splittedText = extract.Split('.');
      string formatedText = "<p>";

      //Every sentence is an index
      //Insert new <p> every third sentence.
      //Use modulus?

      for (int i = 0; i < splittedText.Length; i++)
      {

        if (i % 3 == 0)
          formatedText += $@"</p> <p>";

        if (splittedText[i] != "")
          formatedText += $"{splittedText[i]}.";
      }

      formatedText += "</p>";

      return formatedText;
    }
  }
}