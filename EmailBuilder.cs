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

      var sweColorYellow = "#ffcd00";
      var sweColorBlue = "#004b87";

      return $@"
            <html>
              <body>

                <div style='border: 1px solid {sweColorYellow}; background-color: {sweColorBlue};'> 
                    <p style='color: red; font-weight: bolder;'><u>!NYHET!<u></p>
                    <p style='color: white;'>Nu visas <i>ytterliggare</i> en händelse!</p>

                    <h1 style='color: {sweColorYellow}; padding: 0.5rem;'>
                      {scaperObject.Header} <br>
                       <p style= 'padding: 0.5rem; margin: 0px;'>{date} - {todaysEvent.Year}.</p>
                    </h1>

                </div>

                <div style='border: 1px solid {sweColorYellow}; background-color: {sweColorBlue};'> 
                    
                    <h1 style='color: {sweColorYellow}; padding: 0.5rem;'>
                      Dagens händelse ägde rum <br>
                       <p style= 'padding: 0.5rem; margin: 0px;'>{date} - {todaysEvent.Year}.</p>
                    </h1>

                </div>

                <div style='border: 1px solid black; padding: 0.5rem;'>
                  
                  <h2 style='font-weight: bold;'>{formatedHeading}</h2>

                  <hr>

                  <h3>Artiklar relaterade till händelsen:</h3>
                
                  <h2>{todaysEvent.FirstArticleTitle}</h2>
                  {firstArtikleFormatedEventText}
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
      var splittedText = extract.Split('.');
      string formatedText = "<p>";

      //Every sentence is an index
      //Insert new <p> every third sentence.
      //Use modulus?

      for (int i = 0; i < splittedText.Length; i++)
      {


        formatedText += $@"</p> <p>";

        if (splittedText[i] != "")
          formatedText += $"{splittedText[i]}.";
      }

      formatedText += "</p>";

      return formatedText;
    }
  }
}