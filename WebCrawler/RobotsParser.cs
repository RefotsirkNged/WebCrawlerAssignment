using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    class RobotsParser
    {

        //TODO: tilføj alternativ med forced TLS
        enum ParserState
        {
            LookingForSegment,
            FoundSegment
        }

        public List<string> ParseRobots(string url)
        {
            using (WebClient client = new WebClient())
            {
                Stream robotStream = client.OpenRead((url.Contains("http") ? "" : "http://") + url + @"/robots.txt");
                StreamReader sr = new StreamReader(robotStream);
                List<string> results = new List<string>();
                ParserState pstate = ParserState.LookingForSegment;


                while (!sr.EndOfStream)
                {
                    string temp = sr.ReadLine();
                    temp = temp.ToLower();
                    switch (pstate)
                    {
                        case ParserState.LookingForSegment:
                            if (temp.Contains("user-agent:") && (temp.Contains("*") || temp.Contains("supremeparser")))
                            {
                                pstate = ParserState.FoundSegment;
                            }
                            break;

                        case ParserState.FoundSegment:
                            if (temp.Contains("disallow:"))
                            {
                                results.Add(temp.Split(':').Last().Trim());
                            }
                            else if (temp == string.Empty)
                                pstate = ParserState.LookingForSegment;
                            break;
                    }
                }


                return results; 
            }
        }


        


    }
}
