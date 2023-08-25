using System.Runtime.InteropServices.ComTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SendEmailConsoleApp
{
    public class SweUser
    {
        public required string Name { get; set; }
        public required string Heading { get; set; }
        public required string Text { get; set; }
        public required string BirthYear { get; set; }
        public required string ImageUrl { get; set; }
        public required string PageUrl { get; set; }
    }
    public class TodaysEvent
    {
        public string Heading { get; set; } = null!;
        //The extract is the text
        public string Extract { get; set; } = null!;
        public string Year { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string PageUrl { get; set; } = null!;

        //A related article.
        public string SecondArticleTitle { get; set; } = null!;
        public string SecondArticleExtract { get; set; } = null!;
        public string SecondArticleImageUrl { get; set; } = null!;
        public string SecondArticlePageUrl { get; set; } = null!;
    }
    public class WikiApi
    {
        private readonly HttpClient _httpClient;
        private readonly string _historyApiAccessToken;
        private readonly string _apiUserAgent;
        private readonly string _historyApiBaseUrl;

        public WikiApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _historyApiAccessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJhdWQiOiI4ZmQ1MmJhMTA1MmJjMmIwOTViZjAw";
            _apiUserAgent = "henrik1995a@live.se";
            _historyApiBaseUrl = "https://api.wikimedia.org/feed/v1/wikipedia/sv/onthisday/all";
        }

        public System.Object? GetRandomTest()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", _historyApiAccessToken);
            _httpClient.DefaultRequestHeaders.Add("Api-User-Agent", _apiUserAgent);

            var currentDate = Utils.GetCurrentDate();

            object? json = null;
            var requestResult = _httpClient.GetAsync($"https://api.wikimedia.org/feed/v1/wikipedia/sv/onthisday/all/{currentDate}").Result;

            if (requestResult.IsSuccessStatusCode)
            {
                string? responseBody = requestResult.Content.ReadAsStringAsync().Result;
                json = JsonConvert.DeserializeObject<object>(responseBody);
            }

            return json;
        }

        public dynamic? GetBirths(dynamic apiResponse)
        {
            return apiResponse.births;
        }
        public List<SweUser> GetSwePersons(dynamic? personList)
        {
            List<SweUser> swePersons = new();

            foreach (var person in personList)
            {
                string personTextControl = person.text;

                if (personTextControl.Contains("svensk", StringComparison.OrdinalIgnoreCase) && person.pages.Count != 0)
                {
                    string[] personSplitted = personTextControl.Split(',');

                    string personHeading = "";
                    for (int index = 1; index < personSplitted.Length; index++)
                    {
                        personHeading += personSplitted[index];
                    }

                    personHeading = personHeading.TrimStart();
                    personHeading = CapitalizeFirstLetter(personHeading);
                    string personText = person.pages[0].extract;
                    string personUrl = person.pages[0].content_urls.desktop.page;

                    Console.WriteLine(personUrl);

                    SweUser swePerson = new SweUser()
                    {
                        Name = personSplitted[0],
                        Heading = personHeading,
                        Text = personText,
                        BirthYear = person.year,
                        PageUrl = personUrl,
                        ImageUrl = ""
                    };

                    if (person.pages[0]?.thumbnail?.source != null)
                    {
                        swePerson.ImageUrl = person.pages[0].thumbnail.source;
                    }

                    if (!PersonHasBoringBirthDate(swePerson) && swePerson.ImageUrl != "")
                        swePersons.Add(swePerson);
                }
            }
            return swePersons;
        }
        public TodaysEvent GetEvent(dynamic? eventList)
        {
            //This doesnt have the desired extractlength.
            //It will be returned if no good is found.
            TodaysEvent backupEvent = new TodaysEvent();
            TodaysEvent eventWithLength = new TodaysEvent();

            foreach (var item in eventList)
            {

                // int test = item.pages[0].extract.Length;
                int pageCount = item.pages.Count;

                string extractLength = item.pages[0].extract;
                int extractLengthInt = extractLength.Length;
                // Performing a check in case imageUrls are null, as we require an image in the email.


                var picNumbOne = item.pages[0].thumbnail;
                // string? picNumbOne = item.pages[0].thumbnail?.source;

                string? picNumbTwo = item.pages.Count > 1 ? item.pages[1]?.thumbnail?.source : null;


                if (picNumbOne == null || picNumbTwo == null)
                {
                    continue;
                }
                picNumbOne = item.pages[0].thumbnail.source;

                //Each even has an amount of pages. Every page is a related article.
                foreach (dynamic? page in item.pages)
                {

                    if (page == null || pageCount >= 2 && extractLengthInt > 500)
                    {
                        eventWithLength.Heading = item.text;
                        eventWithLength.Extract = item.pages[0].extract;
                        eventWithLength.Year = item.year;
                        eventWithLength.ImageUrl = item.pages[0].thumbnail.source;
                        eventWithLength.PageUrl = item.pages[0].content_urls.desktop.page;

                        string testTitle = item.pages[1].title;

                        eventWithLength.SecondArticleTitle = NormalizeString(testTitle);
                        eventWithLength.SecondArticleExtract = item.pages[1].extract;
                        eventWithLength.SecondArticleImageUrl = item.pages[1].thumbnail.source;
                        eventWithLength.SecondArticlePageUrl = item.pages[1].content_urls.desktop.page;

                        // An article of sufficient length will be prioritized and returned immediately.
                        return eventWithLength;
                    }

                    else if (item == null)
                    {

                        backupEvent.Heading = item.text;
                        backupEvent.Extract = item.pages[0].extract;
                        backupEvent.ImageUrl = item.pages[0].thumbnail.source;
                        backupEvent.PageUrl = item.pages[0].content_urls.desktop.page;

                        string testTitle = item.pages[1].title;
                        var normalized = NormalizeString(testTitle);

                        backupEvent.SecondArticleTitle = NormalizeString(item.pages[1].title);
                        backupEvent.SecondArticleExtract = item.pages[1].extract;
                        backupEvent.SecondArticleImageUrl = item.pages[1].thumbnail.source;
                        backupEvent.SecondArticlePageUrl = item.pages[1].content_urls.desktop.page;
                    }
                }
            }
            return backupEvent;
        }
        public int GetRandomIndex(int listLength)
        {
            Random rand = new();
            return rand.Next(0, listLength);
        }
        bool PersonHasBoringBirthDate(SweUser user)
        {
            //Returns true if person is born after 1965.

            return int.Parse(user.BirthYear) > 1920;
        }
        string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1);
        }
        public string NormalizeString(string input)
        {
            return input.Replace("_", " ");
        }
    }
}