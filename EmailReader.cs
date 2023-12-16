using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendEmailConsoleApp
{
    public class EmailReader
    {
        //Gets all the emails from a file
        public static string[] getEmails()
        {
            var emails = File.ReadAllLines("../emailsHistoryService_All.txt");

            return emails.Where(x => x != "" || x != " ").ToArray();
        }
    }
}