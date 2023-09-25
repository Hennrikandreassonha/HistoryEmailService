using System.Collections.Specialized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace SendEmailConsoleApp
{
    public class Scraper
    {
        public required string Url { get; set; }
        public string? Header { get; set; }
        public string? Text { get; set; }
        public string? Picture { get; set; }
        public string? PictureText { get; set; }

        public Scraper()
        {
        }
        public async Task<string> Scrape()
        {
            var httpClient = new HttpClient();

            var html = await httpClient.GetStringAsync(Url);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var divInfo = htmlDocument.DocumentNode.Descendants("div").
            Where(x => x.GetAttributeValue("class", "").
            Contains("media-body")).FirstOrDefault();

            var picDiv = htmlDocument.DocumentNode.Descendants("div").
            Where(x => x.GetAttributeValue("class", "").
            Contains("media-year-round")).FirstOrDefault();

            Console.WriteLine(divInfo);
            var picture = picDiv!.Descendants("img").FirstOrDefault();
            string srcValue = picture.GetAttributeValue("src", "");

            if (divInfo != null)
            {

                this.Header = divInfo.Descendants("a").FirstOrDefault()!.InnerText;
                this.Text = divInfo.Descendants("p").ToList()[1]!.InnerText;
                //Fixa

                // var pictureText = 
            }


            return "divInfo";
        }
    }
}