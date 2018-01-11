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
            Dictionary<string, TermVector> invertedList = db.CreateInvertedTerms();
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
                /*List<string> temp = new List<string>();
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
                }*/

                if (invertedList.ContainsKey(key))
                    results.AddRange(invertedList[key].documents.Keys);
            }

            foreach (string key in stemmedQueries.Keys.Where(q => !stemmedQueries[q]))
            {
                if (invertedList.ContainsKey(key))
                    foreach (string document in invertedList[key].documents.Keys)
                    {
                        results.Remove(document);
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
            foreach (string elm in startResualt.documents.Keys)
                resualt.Add(elm);
            for(int i = 2; i < splitQueqe.Length; i++)
            {
                
                if(splitQueqe[i-1] == "And")
                {
                    invertList.TryGetValue(splitQueqe[i], out startResualt);
                    foreach (string elm in startResualt.documents.Keys)
                    {
                        if (resualt.Contains(elm))
                            newResualt.Add(elm);
                    }
                    resualt = newResualt;
                }
                else if(splitQueqe[i - 1] == "Or")
                {
                    foreach (string elm in startResualt.documents.Keys)
                    {
                        newResualt.Add(elm);
                    }
                }
                else
                    Console.Out.WriteLine("Error in BinaryQueqe not matching binary command");
                
            }

            return resualt;
        }

        public List<string> TFIDFQuery(Dictionary<string, TermVector> invertList, string stringQuery)
        {
            Dictionary<string, double> temp = new Dictionary<string, double>();
            HashSet<string> result = new HashSet<string>();
            string[] querySplit = stringQuery.Split(' ');

            foreach (string query in querySplit)
            {
                foreach (KeyValuePair<string, int> doc in invertList[query].documents)
                {
                    if (temp.Keys.Contains(doc.Key))
                        temp[doc.Key] = invertList[query].tfidf(doc.Key);
                    else
                        temp.Add(doc.Key, invertList[query].tfidf(doc.Key));
                }
            }

            return temp.Keys.OrderByDescending(k => temp[k]).ToList<string>();
        }

        public List<string> VectorQuery(Dictionary<string, TermVector> invertList, string stringQuery)
        {
            HashSet<string> result = new HashSet<string>();
            string[] querySplit = stringQuery.Split(' ');
            Dictionary<string, double[]> documentVectorPairs = new Dictionary<string, double[]>();
            PorterStemmer stemmer = new PorterStemmer();
            for (int i = 0; i < querySplit.Length; i++)
                querySplit[i] = stemmer.stem(querySplit[i]);

            for (int i = 0; i < querySplit.Length; i++)
            {
                if (!invertList.Keys.Contains(querySplit[i]))
                    continue;
                foreach (KeyValuePair<string, int> doc in invertList[querySplit[i]].documents)
                {
                    if (documentVectorPairs.Keys.Contains(doc.Key))
                    {
                        documentVectorPairs[doc.Key][i] = invertList[querySplit[i]].tfidf(doc.Key);
                    }
                        
                    else
                    {
                        documentVectorPairs.Add(doc.Key, new double[querySplit.Length]);
                        documentVectorPairs[doc.Key][i] = invertList[querySplit[i]].tfidf(doc.Key);
                    }
                        
                }
            }


            return documentVectorPairs.Keys.OrderByDescending(k => documentVectorPairs[k]).ToList<string>();
        }

        public static Dictionary<string, double> CosineScore(Dictionary<string, TermVector> index, string query)
        {
            Dictionary<string, double> results = new Dictionary<string, double>();
            string[] querySplit = query.Split(' ');
            Dictionary<string, double[]> documentVectorPairs = new Dictionary<string, double[]>();
            PorterStemmer stemmer = new PorterStemmer();
            for (int i = 0; i < querySplit.Length; i++)
                querySplit[i] = stemmer.stem(querySplit[i]);


            Dictionary<string, double> Scores = new Dictionary<string, double>();

            foreach (string term in querySplit)
            {
                if (!index.ContainsKey(term)) //if the index does not contain the term, we can disregard it
                {
                    continue;
                }
                TermVector termVector = index[term]; //get the termvector for the term from the index
                var WeightofTermInQuery = termVector.docLeangt;
                var PostingListForTerm = termVector.documents;

                foreach (KeyValuePair<string, int> pairDocTermfreq in PostingListForTerm)
                {
                    if (results.Keys.Contains(pairDocTermfreq.Key))
                    {
                        results[pairDocTermfreq.Key] += (termVector.tfStar(pairDocTermfreq.Key) / WeightofTermInQuery);
                    }
                    else
                    {
                        results.Add(pairDocTermfreq.Key, (termVector.tfidf(pairDocTermfreq.Key) / WeightofTermInQuery));
                    }

                }
            }


            return results;
        }

        public static Dictionary<string, double> CosineAndPageRankScore(Dictionary<string, TermVector> index, string query)
        {
            Dictionary<string, double> results = CosineScore(index,query);
           
            DatabaseHelper helper = new DatabaseHelper();
            Dictionary<string, double> pageRanks = helper.getPageRank();
            double pageRankWeight = 2;

            foreach (string doc in pageRanks.Keys)
            {
                if (results.Keys.Contains(doc))
                {
                    results[doc] += pageRankWeight * pageRanks[doc];
                }
            }

            return results;
        }



    }
}
