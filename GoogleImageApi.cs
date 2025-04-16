using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HtmlAgilityPack;
using RestSharp;
using Microsoft.Playwright;
namespace HistoryEmailService
{
    public class GoogleImageApi
    {
        public string ApiKey { get; set; }
        public string EngineId { get; set; }
        public string Url { get; set; } = "https://cse.google.com/cse?cx=07aa72db70bc84de6";
        //Returnerna 2 bilder?
        public GoogleImageApi()
        {
            ApiKey = File.ReadAllLines("../HistoryEmailDocs/GoogleApiKeyAndId.txt")[1];
            EngineId = File.ReadAllLines("../HistoryEmailDocs/GoogleApiKeyAndId.txt")[0];
        }
        string exampleSearch = "https://www.google.com/search?q=Vikingarnas+resor+Brittiska+%C3%B6arna+bilder&udm=2";


        public async Task<List<KeyValuePair<string, string>>> GetImageLinksWithClickAsync(string searchQuery)
        {
            List<KeyValuePair<string, string>> imageLinks = new();
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            var trimmedQuery = searchQuery.Trim('"') + " historiska";
            var formattedQuery = string.Join("+", trimmedQuery.Split(' '));

            await page.GotoAsync($"https://www.google.com/search?tbm=isch&q={formattedQuery}");
            await page.WaitForSelectorAsync("img");

            var imageElements = await page.QuerySelectorAllAsync("div[data-attrid='images universal']");

            await RemoveOverlay(page);
            int index = 0;
            while (imageLinks.Count != 2)
            {
                var clicked = imageElements[index].ClickAsync();

                var imtag = await page.WaitForSelectorAsync("img.sFlh5c.FyHeAf.iPVvYb[jsaction]");
                var imgUrl = await imtag.GetAttributeAsync("src");
                var imgText = await imtag.GetAttributeAsync("alt");

                if (imgUrl != null && imgText != null)
                {
                    var kvp = new KeyValuePair<string, string>(imgUrl, imgText);
                    if (!imageLinks.Contains(kvp))
                    {
                        imageLinks.Add(new KeyValuePair<string, string>(imgUrl, imgText));
                    }
                }
                index++;
            }
            await browser.CloseAsync();

            return imageLinks;
        }
        private static async Task<bool> RemoveOverlay(IPage page)
        {
            List<IElementHandle?> elements = new()
            {
                await page.QuerySelectorAsync("button:has-text('Accept All')"),
                await page.QuerySelectorAsync("button:has-text('Godkänn alla')"),
                await page.QuerySelectorAsync("button:has-text('Avvisa alla')")
            };

            foreach (var item in elements)
            {
                if (item != null)
                {
                    await item.ClickAsync();
                    return true;
                }
            }
            return false;
        }
    }
}