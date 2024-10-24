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
        public static void AddToErrorlog(string text)
        {
            string errorPath = @"../Errorlogs.txt";

            var dateNow = DateTime.Now;

            File.AppendAllText(errorPath, $"Error occured: {dateNow} Error: {text} {Environment.NewLine}");
        }
        public static string AddParagrahDivision(string text)
        {

            var splitted = text.Split(". ").ToList();

            for (int i = 0; i < splitted.Count(); i++)
            {
                if (i % 3 == 0 && i != 0)
                {
                    splitted.Insert(i, "<p> </p>");
                }
            }

            return string.Join(". ", splitted);
        }
    }
    public class ListHandler
    {
        public void ClearList(string filePath)
        {
            var emptyList = new List<string>();
            File.WriteAllLines(filePath, emptyList);
        }
        public void RemoveFromList(string filePath, int index)
        {
            var list = File.ReadAllLines(filePath).ToList();
            list.RemoveAt(index);
            File.WriteAllLines(filePath, list);
        }
        public async Task AddSubjectsToList(string filePath, List<string> newSubjects)
        {
            var subjectSkips = File.ReadAllLines(filePath).ToList();
            subjectSkips.AddRange(newSubjects);
            File.WriteAllLines(filePath, subjectSkips);
        }
        public void AddSubjectToList(string filePath, string subject)
        {
            var subjectSkips = File.ReadAllLines(filePath).ToList();
            subjectSkips.Add(subject);
            File.WriteAllLines(filePath, subjectSkips);
        }
        public string GetSubject(string filePath)
        {
            var subjectSkips = File.ReadAllLines(filePath).ToList();
            string subject = subjectSkips.First();
            subjectSkips.RemoveAt(0);
            File.WriteAllLines(filePath, subjectSkips);

            return new string(subject.SkipWhile(x => !char.IsLetter(x)).ToArray());
        }
    }
}