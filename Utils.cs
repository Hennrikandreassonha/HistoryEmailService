using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendEmailConsoleApp
{
    public class Utils
    {
        public static string GetCurrentDate(bool includeYear = false)
        {
            DateTime currentDate = DateTime.Now;

            if (includeYear)
                return currentDate.ToString("yyy/MM/dd");

            return currentDate.ToString("MM/dd");
        }
        public static string GetCurrentTime(bool includeSeconds = false, bool onlyMinutes = false)
        {
            DateTime currentTime = DateTime.Now;

            if (includeSeconds)
                return currentTime.ToString("HH:mm:ss");
            else if (onlyMinutes)
            {
                return currentTime.ToString("HH");

            }
            return currentTime.ToString("HH:mm");
        }
    }
}