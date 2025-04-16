using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
            return File.ReadAllLines("../HistoryEmailDocs/ImgurPass.txt").ToArray();
        }
        public string UploadImage(byte[] bytes, string title, string description)
        {
            try
            {
                var client = new RestClient("https://api.imgur.com/3/image");
                var request = new RestRequest("https://api.imgur.com/3/image", Method.Post);

                request.AddHeader("accept", "application/json");
                request.AddHeader("authorization", $"Bearer {AccessToken}");

                string base64Image = Convert.ToBase64String(bytes);

                request.AddParameter("image", base64Image);
                request.AddParameter("title", title);
                request.AddParameter("description", description);

                var response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    string imageUrl = jsonResponse.data.link;
                    Console.WriteLine("Image uploaded successfully. URL: " + imageUrl);
                    return imageUrl;
                }
                else
                {
                    Console.WriteLine($"Failed to upload image: {response.ErrorMessage}");
                    return "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                return "";
            }
        }


    }
}
