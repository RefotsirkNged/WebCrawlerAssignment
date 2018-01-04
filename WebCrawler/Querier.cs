using System;
using System.Collections.Generic;
using System.Linq;

namespace WebCrawler
{
    public class Querier
    {
        public Querier()
        {
            
        }

        public static List<string> Query(Dictionary<string, bool> queries, DatabaseHelper db)
        {
            
            List<string> results = new List<string>();
			Dictionary<string, bool> stemmedQueries = new Dictionary<string, bool>();
            PorterStemmer stemmer = new PorterStemmer();
            bool isFirst = false;

            foreach (string key in queries.Keys)
            {
                stemmedQueries.Add(stemmer.stem(key), queries[key]);
            }

            foreach (string key in stemmedQueries.Keys.Where(q => stemmedQueries[q]))
            {
                List<string> temp = new List<string>();
                List<string> queryResults = db.QueryTerm(key);

                if(queryResults.Count() == 0)
                {
                    results.Clear();
                    break;
                }
                else if(!isFirst)
                {
                    isFirst = true;
                    results.AddRange(queryResults);
                }
                else
                {
                    temp.AddRange(db.QueryTerm(key).Where(r => results.Contains(r)));
                    results = temp;
                }
            }

			foreach (string key in stemmedQueries.Keys.Where(q => !stemmedQueries[q]))
			{
				foreach (string term in db.QueryTerm(key))
				{
                    results.Remove(term);
                }
			}

			
            return results;
        }

        //Only support "And" and "Or"
        //TODO should be testes
        public HashSet<string> BinaryQueqe(Dictionary<string, TermVector> invertList, string stringQueqe)
        {
            HashSet<string> resualt = new HashSet<string>();
            HashSet<string> newResualt = new HashSet<string>();
            string[] splitQueqe = stringQueqe.Split(' ');
            TermVector startResualt;
            invertList.TryGetValue(splitQueqe[0], out startResualt);
            foreach (string elm in startResualt.documents)
                resualt.Add(elm);
            for(int i = 2; i < splitQueqe.Length; i++)
            {
                
                if(splitQueqe[i-1] == "And")
                {
                    invertList.TryGetValue(splitQueqe[i], out startResualt);
                    foreach (string elm in startResualt.documents)
                    {
                        if (resualt.Contains(elm))
                            newResualt.Add(elm);
                    }
                    resualt = newResualt;
                }
                else if(splitQueqe[i - 1] == "Or")
                {
                    foreach (string elm in startResualt.documents)
                    {
                        newResualt.Add(elm);
                    }
                }
                else
                    Console.Out.WriteLine("Error in BinaryQueqe not matching binary command");
                
            }

            return resualt;
        }
    }
}
