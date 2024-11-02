using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public static void UploadImage(){

        }
    }
}
