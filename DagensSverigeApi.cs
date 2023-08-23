using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SendEmailConsoleApp
{
    public class DagensSverigeApi
    {
        public class SubscribedUser
        {
            public string Email { get; set; }
        }
        string baseUrl = "https://localhost:7164/EmailService";
        private readonly HttpClient _httpClient;
        public DagensSverigeApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<string> GetSubscribers()
        {
            var requestResult = _httpClient.GetAsync($"{baseUrl}/getsubscribers").Result;

            List<string> userList = new();

            if (requestResult.IsSuccessStatusCode)
            {
                string? responseBody = requestResult.Content.ReadAsStringAsync().Result;

                JArray jsonArray = JArray.Parse(responseBody);

                foreach (JObject jsonObject in jsonArray)
                {
                    string? email = jsonObject["email"]?.ToString();
                    
                    if(email != null){
                        userList.Add(email);
                    }
                }
            }
            return userList;
        }
    }
}