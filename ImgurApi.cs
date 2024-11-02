using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;

namespace HistoryEmailService
{
    public class ImgurApi
    {
        //Will be
        public string AccessToken { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public ImgurApi()
        {
            var secrets = GetAccessCodes();
            AccessToken = secrets[0];
            ClientId = secrets[1];
            ClientSecret = secrets[2];
        }
        private string[] GetAccessCodes()
        {
            //0 = AccessToken, 1 = ClientID, 2 Client Secret
            return File.ReadAllLines("../ImgurPass").ToArray();
        }
        public static void UploadImage(byte[] bytes)
        {
            var options = new RestClientOptions("https://api.openai.com/v1/images/generations");
            
            var request = new RestRequest("https://api.openai.com/v1/images/generations", Method.Post);
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", $"Bearer {OpenAiKey}");
        }
    }
}
