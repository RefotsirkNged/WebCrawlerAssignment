using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    class Program
    {


        static void Main(string[] args)
        {
            List<string> seed = new List<string>
            {
                "https://www.icrc.org",
                "https://www.amnesty.org/"


            };
            SupremeCrawler sr = new SupremeCrawler(seed);
            while (true)
            {
                Console.WriteLine("Would you like to load an index [1] and go from there, or crawl from a new seed [2], or crawl from the default seeds [3]");
                var response = Console.ReadLine();
                if (response == "1")
                {
                    Console.WriteLine("Please place the desired index in the root folder of this .exe, and type in the filename:");
                    string filename = Console.ReadLine();
                    sr.ReadIndexFromXml("..//" + filename + ".index");
                    sr.StartCrawl();
                }
                else if (response == "2")
                {
                    Console.WriteLine("Please type in the pages you would like to use as seed/type in x to stop and move on to crawling:");
                    List<string> seeds = new List<string>();
                    string seedResponse = "";
                    while(seedResponse != "x")
                    {
                        seedResponse = Console.ReadLine();
                        if (seedResponse != "x")
                        {
                            seeds.Add(seedResponse);
                            Console.WriteLine("seed added");
                        }

                        else if (seeds.Count <= 0)
                        {
                            Console.WriteLine("You didnt type in any seeds!");
                            seedResponse = "";
                        }

                        else
                            Console.WriteLine("Beginning crawl...");

                        sr = new SupremeCrawler(seeds);
                        sr.StartCrawl();
                    }
                }
                else if(response == "3")
                {
                    sr.StartCrawl();
                    sr.WriteIndexToXml("defaultSeedIndex");
                    Console.WriteLine("Result saved to root folder");
                }
                else
                    Console.WriteLine("That was not a proper response!");

            }


            sr.StartCrawl();

            





            Console.ReadLine();
        }
    }
}
