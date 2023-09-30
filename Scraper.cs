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
                    this.Header = divInfo.Descendants("a").FirstOrDefault()!.InnerText;
                    Text = divInfo.Descendants("p").ToList()[1]!.InnerText;

                    PictureUrl = $"https://www.so-rummet.se/{picDiv.Descendants("img").FirstOrDefault()!.GetAttributeValue("src", "")}";

                    PictureText = picDiv.Descendants("p").FirstOrDefault()!.InnerText;
                    Console.WriteLine("jas");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Err making scraperobject: ", e.Message);
                }
            }
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