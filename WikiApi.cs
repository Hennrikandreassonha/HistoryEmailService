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
        public SweUser()
        {

        }
        public string Name { get; set; }
        public string Heading { get; set; }
        public string Text { get; set; }
        public string BirthYear { get; set; }
        public string ImageUrl { get; set; }
        public string PageUrl { get; set; }
    }
    // public class WikiPerson
    // {
    //     public required string Name { get; set; }
    //     public required string WikiUrl { get; set; }
    // }

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
            if (apiResponse == null)
            {
                return null;
            }

            dynamic births = apiResponse.births;
            return births;
        }
        public List<SweUser> GetSwePersons(dynamic? personList)
        {
            List<SweUser> swePersons = new();

            foreach (var person in personList)
            {
                string personTextControl = person.text;

                if (personTextControl.Contains("svensk", StringComparison.OrdinalIgnoreCase) && person.pages.Count != null &&
                person.pages.Count != 0)
                {
                    SweUser swePerson = new SweUser();

                    try
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

                        swePerson.Name = personSplitted[0];
                        swePerson.Heading = personHeading;
                        swePerson.Text = personText;
                        swePerson.BirthYear = person.year;
                        swePerson.PageUrl = personUrl;
                        swePerson.ImageUrl = "";

                        if (person.pages[0]?.thumbnail?.source != null)
                        {
                            swePerson.ImageUrl = person.pages[0].thumbnail.source;
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

                    if (!PersonHasBoringBirthDate(swePerson) && swePerson.ImageUrl != "")
                        swePersons.Add(swePerson);
                }
            }
            return swePersons;
        }
        public List<SweUser> GetMoreSweBirths(dynamic? allSweBirths)
        {

            List<SweUser> swePersons = new();

            foreach (var person in allSweBirths)
            {
                string personTextControl = person.text;

                if (personTextControl.Contains("svensk", StringComparison.OrdinalIgnoreCase) && person.pages.Count != null && person.pages.Count != 0)
                {

                    SweUser swePerson = new SweUser();
                    try
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


                        swePerson.Name = personSplitted[0];
                        swePerson.Heading = personHeading;
                        swePerson.Text = personText;
                        swePerson.BirthYear = person.year;
                        swePerson.PageUrl = personUrl;
                        swePerson.ImageUrl = "";

                    }
                    catch (Exception e)
                    {
                        if (e.StackTrace != null)
                        {
                            Utils.AddToErrorlog(e.StackTrace);
                            Console.WriteLine(e.StackTrace);
                        }
                    }

                    swePersons.Add(swePerson);
                }
            }
            return swePersons;
        }
        public TodaysEvent GetEvent(dynamic? eventList)
        {
            //This doesnt have the desired extractlength.
            //It will be returned if no good is found.
            TodaysEvent eventWithLength = new TodaysEvent();

            int currentHeadingLength = 0;

            foreach (var item in eventList)
            {
                try
                {
                    int pageCount = item.pages.Count;

                    string headingText = item.text;
                    int headingLength = headingText.Length;

                    var picNumbOne = item.pages[0].thumbnail;
                    // string? picNumbOne = item.pages[0].thumbnail?.source;

                    string? picNumbTwo = item.pages.Count > 1 ? item.pages[1]?.thumbnail?.source : null;

                    if (picNumbOne == null || picNumbTwo == null)
                    {
                        continue;
                    }
                    picNumbOne = item.pages[0].thumbnail.source;

                    //Om nuvarande artikel är längre än den vi redan sparat och
                    //Så ska den sparas istället
                    if (currentHeadingLength < headingLength)
                    {
                        //Heading length måste vara lång
                        string pageHeading = item.text;
                        currentHeadingLength = pageHeading.Length;

                        string text = item.text;
                        eventWithLength.Heading = Utils.AddParagrahDivision(text);
                        eventWithLength.Year = item.year;

                        eventWithLength.FirstArticleTitle = NormalizeString(item.pages[0].title.ToString());
                        eventWithLength.FirstArticleExtract = item.pages[0].extract;
                        eventWithLength.FirstArticleImageUrl = item.pages[0].thumbnail.source;
                        eventWithLength.FirstArticlePageUrl = item.pages[0].content_urls.desktop.page;

                        eventWithLength.SecondArticleTitle = NormalizeString(item.pages[1].title.ToString());
                        eventWithLength.SecondArticleExtract = item.pages[1].extract;
                        eventWithLength.SecondArticleImageUrl = item.pages[1].thumbnail.source;
                        eventWithLength.SecondArticlePageUrl = item.pages[1].content_urls.desktop.page;

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
            return eventWithLength;
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