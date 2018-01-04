using System;
using System.Collections.Generic;

namespace WebCrawler
{
    public class TermVector
    {
        public string term;
        private int id;

        //Document (url) and frequenzy
        public HashSet<string> documents;

        public TermVector(string term, int id)
        {
            this.term = term;
            this.id = id;
            documents = new HashSet<string>();
        }

        public int DocFrekensi { get { return documents.Count; } }
        public int ID { get { return id; } }  

        
    }
}
