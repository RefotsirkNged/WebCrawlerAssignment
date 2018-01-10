using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Text.RegularExpressions;


namespace WebCrawler
{
    class Spiderman
    {
        Queue<string> frontier = new Queue<string>();
        SimilarityAnalyser SimAnalyser;
        public DatabaseHelper dbhelper = new DatabaseHelper();

        //param1: url param2: html

        Dictionary<string, string> index = new Dictionary<string, string>();
        public Indexer actualIndex = new Indexer();
        Dictionary<string, List<string>> robotsIndex = new Dictionary<string, List<string>>();

        List<string> GenerallyDisallowedActions = new List<string>()
        {
            ".css",
            "facebook",
            "twitter",
            "linkedin",
            "mailto",
            "js",
            "rss",
            ".png",
            ".jpg",
            ".gif",
            ".pdf",
            ".jpeg",
            ".mp3",
            ".mp4",
            ".img",
            ".ico",
            ".ru",
            "/ru"
        };


        public Spiderman(List<string> seed)
        {
            foreach (string url in seed)
            {
                frontier.Enqueue(url);
            }
            RobotsParser rb = new RobotsParser();
            foreach (string seedSite in frontier)
            {
                Uri seedUri = new Uri(seedSite);
                robotsIndex.Add(seedUri.Host, rb.ParseRobots(seedSite));
            }
            var tempRbIndex = robotsIndex.ToList();
            for (int j = 0; j < tempRbIndex.Count; j++)
            {
                var result = tempRbIndex[j].Value;
                for (int i = 0; i < tempRbIndex[j].Value.Count(); i++)
                {
                    if (tempRbIndex[j].Value[i].Last() != '/')
                    {
                        result[i] = result[i] + "/";
                    }
                }
                robotsIndex[tempRbIndex[j].Key] = result;
            }
            SimAnalyser = new SimilarityAnalyser();
        }

        public static string StripHTML(string input)
        {
            var regex = new Regex(
                "(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)",
                RegexOptions.Singleline | RegexOptions.IgnoreCase
            );
            string result = regex.Replace(input, String.Empty);
            result = Regex.Replace(result, "<.*?>", string.Empty, RegexOptions.Multiline);
            result = Regex.Replace(result, "xmlns.*?\n", string.Empty);
            result = Regex.Replace(result, "<html.*?\n", "");
            result = Regex.Replace(result, "<iframe.*?", string.Empty);
            result = Regex.Replace(result, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
            return result;
        }

        public void StartCrawl()
        {
            while (frontier.Count > 0)
            {
                string url = frontier.Dequeue();
                Console.WriteLine("Loading :" + url);

                string result = CrawlPage(new Uri(url));
                if (!CheckIfContentAlreadySeen(result))
                {
                    if (!index.ContainsKey(url))
                    {
                        Dictionary<string, string> links = ExtractLinks(result);
                        actualIndex.IndexDocument(url, StripHTML(result), links);
                        foreach (string link in links.Keys)
                        {
                            Uri hostPage = new Uri(url);
                            List<string> disallowedActions = robotsIndex[hostPage.Host].ToList();
                            if (link.Contains("http") && !disallowedActions.Any(x => link.Contains(x)) &&
                                !GenerallyDisallowedActions.Any(x => link.Contains(x)))
                            {
                                AddToFrontier(link);
                            }

                            else if (!disallowedActions.Any(x => link.Contains(x)) &&
                                     !GenerallyDisallowedActions.Any(x => link.Contains(x)))
                                AddToFrontier(url + link);
                        }
                        Console.WriteLine("Finish loading :" + url);
                    }
                    else
                        Console.WriteLine("URL already in index");
                }
                Console.WriteLine(frontier.Count());
                //TODO: insert delay
            }
            Console.WriteLine("done");
        }

        private void AddToFrontier(string url)
        {
            if (!frontier.Contains(url) && !index.ContainsKey(url))
                frontier.Enqueue(url);
        }

        private Dictionary<string, string> ExtractLinks(string page)
        {
            Dictionary<string, string> links = new Dictionary<string, string>();
            using (StringReader reader = new StringReader(page))
            {
                string line = "";
                while (line != null)
                {
                    if (line.Contains("href=\"") && line.Contains("</a>") && !line.Contains("<img"))
                    {
                        StringBuilder sb = new StringBuilder();
                        bool foundQuote = false;
                        char[] stringAsCharArray = line.ToCharArray();
                        string link = "";
                        int ancorStart;
                        bool ancorTextStart = false;
                        for (int i = line.IndexOf("href=\""); i < stringAsCharArray.Length && i >= 0; i++)
                        {

                            if (stringAsCharArray[i] == '"' && !ancorTextStart)
                            {
                                if (foundQuote)
                                {
                                    if (sb.ToString().Contains('?') || sb.ToString().Contains('#') || sb.ToString().Contains('\'') || !sb.ToString().Contains("http") || sb.ToString().StartsWith("/"))
                                        break;
                                    link = sb.ToString();
                                    ancorTextStart = true;
                                }
                                foundQuote = true;
                            }
                            else if (foundQuote && !ancorTextStart)
                            {
                                sb.Append(stringAsCharArray[i]);
                            }
                            if(stringAsCharArray[i] == '>')
                            {
                                if(!links.ContainsKey(link))
                                {
                                    string ancorText = line.Substring(i + 1);
                                    if(ancorText.Contains("</a>"))
                                        ancorText = ancorText.Remove(ancorText.IndexOf("</a>"), ancorText.Length - ancorText.IndexOf("</a>"));
                                    links.Add(link, ancorText);
                                    break;
                                }
                            }

                        }
                    }

                    line = reader.ReadLine();
                }
            }
           
            return links;
        }

        private bool CheckIfContentAlreadySeen(string page)
        {
            if (page != "" && SimAnalyser.IsTextDuplicate(page))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string CrawlPage(Uri url)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    Stream pageStream = wc.OpenRead(url.OriginalString);
                    StreamReader sr = new StreamReader(pageStream);

                    string host = url.Host;
                    if (!robotsIndex.ContainsKey(host))
                    {
                        RobotsParser rb = new RobotsParser();
                        robotsIndex.Add(host, rb.ParseRobots(host));
                    }
                    string result = sr.ReadToEnd();
                    return result;
                }
            }
            catch (WebException ex)
            {
                //var responseCode = ((HttpWebResponse)ex.Response).StatusDescription;
                //Console.WriteLine("Request returned error:" + responseCode);
                Console.WriteLine("Error: " + url + "\n" + ex.Message);
            }
            return "";
        }
    }
}