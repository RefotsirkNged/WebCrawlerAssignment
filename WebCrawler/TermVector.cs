using System;
using System.Collections.Generic;

namespace WebCrawler
{
    public class TermVector
    {
        public string term;
        private int id;

        //Document (url) and frequenzy
        public List<string> documents;

        public TermVector(string term, int id)
        {
            this.term = term;
            this.id = id;
            documents = new List<string>();
        }

        public int DocFrekensi { get { return documents.Count; } }
        public int ID { get { return id; } }  

        public List<string> Queqe(string queqeString)
        {
            List<string> resualt = new List<string>();

            string[] and = queqeString.Split(new string[] {"and"}, StringSplitOptions.None);

            return resualt;
        }
    }
}
