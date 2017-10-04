﻿using System;
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
            Spiderman sr = new Spiderman(seed);
            while (true)
            {
                Console.WriteLine("Would you like to load an index [1] and go from there, or crawl from a new seed [2], or crawl from the default seeds [3]");
				Console.WriteLine("Or write [4] to make a query.");

				var response = Console.ReadLine();
                if (response == "1")
                {
                    Console.WriteLine("not used.");
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

                        sr = new Spiderman(seeds);
                        sr.StartCrawl();
                    }
                }
                else if(response == "3")
                {
                    sr.StartCrawl();
                    sr.actualIndex.WriteToTxt();
                    Console.WriteLine("Result saved to root folder");
                }
				else if (response == "4")
				{
                    Dictionary<string, bool> words = new Dictionary<string, bool>();
                    Console.WriteLine("Write the words you would like to query (Eks: duck AND bird AND NOT chicken)");
                    string query = Console.ReadLine().Replace(" AND ", ";");


                    foreach (string word in query.Split(';'))
                    {
                        if(word.Split(' ').Count() > 1 && word.Split(' ')[0].Trim() == "NOT")
                        {
                            words.Add(word.Split(' ')[1].Trim(), false);
                        }
                        else
                        {
                            words.Add(word.Split(' ')[0].Trim(), false);
                        }
                    }

                    foreach (string result in Querier.Query(words))
                    {
                        Console.WriteLine(result);
                    }
                }
                else
                    Console.WriteLine("That was not a proper response!");

            }
        }
    }
}
