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
        public string? TermsInlineExtraInfo { get; set; }
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

                    // TermsInlineExtraInfo = GetExtraInfo(htmlDocument);

                    PictureUrl = $"https://www.so-rummet.se/{picDiv.Descendants("img").FirstOrDefault()!.GetAttributeValue("src", "")}";

                    var pictureDiv = picDiv.Descendants("div").Where(x => x.GetAttributeValue("class", "").Contains("image-text")).FirstOrDefault();
                    PictureText = pictureDiv.InnerHtml;
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
        }

        private string? GetExtraInfo(HtmlDocument htmlDocument)
        {
            var termsAndExtra = htmlDocument.DocumentNode.Descendants("div").
       Where(x => x.GetAttributeValue("class", "").
       Contains("terms") || x.GetAttributeValue("class", "").
       Contains("terms-inline")).FirstOrDefault();


            return termsAndExtra.InnerHtml;
        }

        public string GetAllParagrafs(HtmlNode divInfo)
        {
            var stringToReturn = "";
            var allParagrafs = divInfo.Descendants("p").ToList();

            foreach (var item in allParagrafs)
            {
                string currentString = item.InnerText;
                if (item.InnerText == "")
                {
                    continue;
                }
                else if (item.InnerHtml.Contains("a href"))
                {
                    currentString = FixReadMore(item.InnerHtml)!;
                    stringToReturn += "<p>";
                    stringToReturn += currentString;
                    stringToReturn += "</p>";
                    continue;
                }
                else
                {
                    stringToReturn += "<p>";
                    stringToReturn += item.InnerText;
                    stringToReturn += "</p>";
                }

            }

            return stringToReturn;
        }

        private string? FixReadMore(string innerHtml)
        {
            //Fixa alla länkar

            //Putting the absolute string since email does not allow relative.
            try
            {
                var indexes = FindAllOccurrences(innerHtml, "a href=");

                var length = "a href=".Length;

                foreach (var index in indexes)
                {
                    innerHtml = innerHtml.Insert(index + length + 1, "https://www.so-rummet.se");
                }

                Console.WriteLine("");

                return innerHtml;

            }
            catch (Exception e)
            {
                if (e.StackTrace != null)
                {
                    Utils.AddToErrorlog(e.StackTrace);
                    Console.WriteLine(e.StackTrace);
                }
            }
            return "";
        }
        static List<int> FindAllOccurrences(string input, string targetWord)
        {
            List<int> indexes = new List<int>();
            int index = 0;

            while ((index = input.IndexOf(targetWord, index)) != -1)
            {
                index += indexes.Count * "https://www.so-rummet.se".Length;

                indexes.Add(index);
                index += targetWord.Length;
            }

            return indexes;
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