using System;
using System.Collections.Generic;

namespace WebCrawler
{
    public class TermVector
    {
        public string term;
        private int id;
        public static int TotalDocuments;

        //Document (url) and frequenzy
        public Dictionary<string, int> documents;

        public TermVector(string term, int id)
        {
            this.term = term;
            this.id = id;
            documents = new Dictionary<string, int>();
            DatabaseHelper hlper = new DatabaseHelper();
            if (TotalDocuments == 0)
            {
                TotalDocuments = hlper.TotalDocuments();
            }
        }

        public int DocFrequency
        {
            get { return documents.Count; }
        }

        public int ID
        {
            get { return id; }
        }

        public double Idf
        {
            get { return Math.Log10(TotalDocuments / DocFrequency); }
        }

        public double tfidf
        {
            get { return math}
        }
    }
}