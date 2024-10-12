using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;

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
            var test = File.ReadAllLines("../GoogleApiKeyAndId.txt"); 
            ApiKey = File.ReadAllLines("../GoogleApiKeyAndId.txt")[0];
            EngineId = File.ReadAllLines("../GoogleApiKeyAndId.txt")[1];
        }
        public async Task<bool> SendRequest(string searchQuery){
            var searchParams = new
            {
                key = ApiKey,
                cx = EngineId,
                q = searchQuery
            };
            var request = new RestRequest(Url, Method.Get);
            request.AddJsonBody(searchParams);
            var options = new RestClientOptions(Url);
            var client = new RestClient(options);
            var responsee = await client.PostAsync(request);
            Console.WriteLine(responsee.Content);
            return true;
        }
    }
}