using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebCrawler
{
    public class Indexer
    {
        private string[] stopWords;
        private Dictionary<string, TermVector> terms;

        public Indexer()
        {
            stopWords = File.ReadAllLines("..\\Resources\\StopWords.txt");
            terms = new Dictionary<string, TermVector>();
        }

        public int TermsCount
        {
            get { return terms.Count; }
        }

        public void IndexDocument(string url, string document)
        {
            PorterStemmer stemmer = new PorterStemmer();
            List<string> symbols = File.ReadAllLines("..\\Resources\\Symbols.txt").ToList();
            DatabaseHelper db = new DatabaseHelper();

            foreach (string symbol in symbols)
            {
                document = document.Replace(symbol, "");
            }

            List<string> words = document.Split(' ').Where(w => w != string.Empty).ToList();

            foreach (string word in stopWords)
            {
                words.Remove(word);
            }

            for (int i = 0; i < words.Count(); i++)
            {
                words[i] = stemmer.stem(words[i]);
            }

            foreach (string word in words)
            {
                //if(terms.ContainsKey(word)){
                //    terms[word].AddDocument(url);
                //}
                //else
                //{
                //    terms.Add(word, new TermVector(word));
                //    terms[word].AddDocument(url);
                //}

                db.UpdateOrInsertPair(word, url);
            }
        }

        

        public void WriteToTxt()
        {
        }
    }
}
