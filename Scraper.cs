using System.Collections.Specialized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace SendEmailConsoleApp
{
    public class ScraperObject
    {
        public required string Url { get; set; }
        public string? Header { get; set; }
        public string? Text { get; set; }
        public string? ReadMoreLink { get; set; }
        public string? PictureUrl { get; set; }
        public string? PictureText { get; set; }

        public ScraperObject()
        {
        }
        public async Task ScrapeAsync()
        {
            var httpClient = new HttpClient();

            var html = await httpClient.GetStringAsync(Url);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var divInfo = GetTextDiv(htmlDocument);
            var picDiv = GetPicDiv(htmlDocument);

            if (divInfo != null && picDiv != null)
            {
                try
                {
                    Header = divInfo.Descendants("a").FirstOrDefault()!.InnerText;
                    Text = GetAllParagrafs(divInfo);

                    PictureUrl = $"https://www.so-rummet.se/{picDiv.Descendants("img").FirstOrDefault()!.GetAttributeValue("src", "")}";

                    var pictureDiv = picDiv.Descendants("div").Where(x => x.GetAttributeValue("class", "").Contains("image-text")).FirstOrDefault();
                    PictureText = pictureDiv.InnerHtml;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Err making scraperobject: ", e.Message);
                    Utils.AddToErrorlog(e.Message);
                }
            }
        }
        public string GetAllParagrafs(HtmlNode divInfo)
        {
            var stringToReturn = "";
            var allParagrafs = divInfo.Descendants("p").ToList();

            foreach (var item in allParagrafs)
            {
                if (item.InnerHtml.Contains("a href"))
                {
                    ReadMoreLink = FixReadMore(item.InnerHtml);
                    continue;
                }
                if (item.InnerText == "")
                {
                    continue;
                }

                stringToReturn += "<p>";
                stringToReturn += item.InnerText;
                stringToReturn += "</p>";
            }

            return stringToReturn;
        }

        private string? FixReadMore(string innerHtml)
        {
            //Putting the absolute string since email does not allow relative.
            var index = innerHtml.IndexOf("/fakta-artiklar");
            innerHtml = innerHtml.Insert(index, "https://www.so-rummet.se");

            Console.WriteLine("");

            return innerHtml;
        }

        private static HtmlNode? GetTextDiv(HtmlDocument htmlDocument)
        {
            return htmlDocument.DocumentNode.Descendants("div").
            Where(x => x.GetAttributeValue("class", "").
            Contains("media-body")).FirstOrDefault();
        }
        private static HtmlNode? GetPicDiv(HtmlDocument htmlDocument)
        {
            return htmlDocument.DocumentNode.Descendants("div").
            Where(x => x.GetAttributeValue("class", "").
            Contains("media-year-round")).FirstOrDefault();
        }
    }
}