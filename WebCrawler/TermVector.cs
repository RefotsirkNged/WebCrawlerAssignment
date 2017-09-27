using System;
using System.Collections.Generic;

namespace WebCrawler
{
    public class TermVector
    {
        public string term;
        private int ID;

        //Document (url) and frequenzy
        private Dictionary<string, int> documents;

        public TermVector(string term)
        {
            this.term = term;
            documents = new Dictionary<string, int>();
        }

        public Dictionary<string, int> Documents
        {
            get { return documents; }
        }

        public void AddDocument(string document)
		{
            if (documents.ContainsKey(document))
                documents[document]++;
            else
                documents[document] = 1;
		}
    
    }
}
