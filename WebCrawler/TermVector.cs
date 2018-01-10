using System;
using System.Collections.Generic;

namespace WebCrawler
{
    public class TermVector
    {
        public string term;
        private int id;
        public static int TotalDocuments; //Total amount of documents in the database

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

        //tfidf is for the specific term in a document
        public double tfidf(string document)
        {
            return documents[document] * Idf;
        }

        public double tfStar(string document)
        {
            if (documents[document] == 0)
                return 0;
            else
                return 1 + Math.Log10(documents[document]);
        }

        public double docLeangt
        {
            get
            {
                double addDocTF = 0;
                foreach (int tf in documents.Values)
                    addDocTF += Math.Pow(tf, 2);
                return Math.Sqrt(addDocTF);
            }
        }
    }
}