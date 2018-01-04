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

        public Indexer()
        {
            stopWords = File.ReadAllLines("..\\Resources\\StopWords.txt");
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

            document = document.Replace("\n", " ");

            List<string> words = document.Split(null).Where(w => w != string.Empty).ToList();

            foreach (string word in words)
            {
                word.Trim();
            }

            words.RemoveAll(w => stopWords.Contains(w));

            for (int i = 0; i < words.Count(); i++)
            {
                words[i] = stemmer.stem(words[i]);
            }

            db.UpdateOrInsertPair(words, url);

        }

        

        public void WriteToTxt()
        {
        }
    }
}
