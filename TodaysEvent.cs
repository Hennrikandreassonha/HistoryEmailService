using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendEmailConsoleApp
{
    public class TodaysEvent
    {
        public string Heading { get; set; } = null!;
        public string Year { get; set; } = null!;
        //The extract is the text
        public string FirstArticleTitle { get; set; } = null!;
        public string FirstArticleExtract { get; set; } = null!;
        public string FirstArticleImageUrl { get; set; } = null!;
        public string FirstArticlePageUrl { get; set; } = null!;

        //A related article.
        public string SecondArticleTitle { get; set; } = null!;
        public string SecondArticleExtract { get; set; } = null!;
        public string SecondArticleImageUrl { get; set; } = null!;
        public string SecondArticlePageUrl { get; set; } = null!;

    }

}