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

            foreach (string key in queries.Keys)
            {
                stemmedQueries.Add(stemmer.stem(key), queries[key]);
            }

            foreach (string key in stemmedQueries.Keys.Where(q => stemmedQueries[q]))
            {
                results.AddRange(db.QueryTerm(key));
                int i = 9;
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
    }
}
