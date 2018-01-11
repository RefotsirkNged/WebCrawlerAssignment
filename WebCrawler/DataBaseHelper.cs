using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class DatabaseHelper
    {
        private const string DatabaseName = "TermDatabase.sqlite";

        private const string Terms = "Terms";
        private const string termIDCol = "TermID";
        private const string termCol = "Term";

        private const string Documents = "Documents";
        private const string docIDCol = "ID";
        private const string docTermFKCol = termIDCol;
        private const string docTermCountCol = "TermCount";

        private const string DocumentsIndex = "DocumentIndex";
        private const string docIndexID = "DocID";
        private const string docDomainName = "DocDomainName";
        private const string docPageRank = "PageRank";


        private const string DocLinks = "DocLinks";
        private const string startDoc = "StartDoc";
        private const string endDoc = "EndDoc";

        private enum DBStatus
        {
            NotExist,
            Exist
        }

        private SQLiteConnection m_dbConnection;

        public DatabaseHelper()
        {
            if (!File.Exists(Directory.GetCurrentDirectory() + "//TermDatabase.sqlite"))
            {
                SQLiteConnection.CreateFile(DatabaseName);
                m_dbConnection = new SQLiteConnection("Data Source=" + DatabaseName + ";Version=3;");
                m_dbConnection.Open();

                string sqlTable1 = "create table " + Terms + " (" + termIDCol +
                                   " INTEGER PRIMARY KEY AUTOINCREMENT , " + termCol + " TEXT UNIQUE)";
                string sqlTable2 = "create table " + Documents + " (" + docIDCol + " TEXT , " + docTermFKCol +
                                   " INTEGER , " + docTermCountCol + " INTEGER )";
                string sqlTable3 = "create table " + DocLinks + " (" + startDoc + " TEXT , " + endDoc +
                                   " TEXT )";
                string sqlTable4 = "create table " + DocumentsIndex + " (" + docIndexID +
                                   " INTEGER PRIMARY KEY AUTOINCREMENT , " + docDomainName + " TEXT UNIQUE, " + docPageRank + " DOUBLE )";

                SQLiteCommand command = new SQLiteCommand(sqlTable1, m_dbConnection);
                command.ExecuteNonQuery();
                command = new SQLiteCommand(sqlTable2, m_dbConnection);
                command.ExecuteNonQuery();
                command = new SQLiteCommand(sqlTable3, m_dbConnection);
                command.ExecuteNonQuery();
                command = new SQLiteCommand(sqlTable4, m_dbConnection);
                command.ExecuteNonQuery();
            }
            else
            {
                m_dbConnection = new SQLiteConnection("Data Source=" + Directory.GetCurrentDirectory() + "\\" +
                                                      DatabaseName + ";Version=3;");
            }


            m_dbConnection.Close();
        }

        //TODO Need to be tested but should work
        public Dictionary<string, TermVector> CreateInvertedTerms()
        {
            m_dbConnection.Open();
            SQLiteCommand sqlCommand;
            Dictionary<string, TermVector> invertList = new Dictionary<string, TermVector>();
            using (SQLiteCommand command = new SQLiteCommand("Select * From " + Terms, m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string term = reader.GetString(1);
                        int id = reader.GetInt32(0);
                        invertList.Add(term, new TermVector(term, id));
                    }
                }
            }

            foreach (TermVector elm in invertList.Values)
            {
                using (SQLiteCommand command =
                    new SQLiteCommand("Select * From " + Documents + " Where " + docTermFKCol + " = " + elm.ID,
                        m_dbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string doc = reader.GetString(0);
                            int termFrequency = reader.GetInt32(2);
                            elm.documents.Add(doc, termFrequency);
                        }
                    }
                }
            }

            m_dbConnection.Close();
            return invertList;
        }


        public void UpdateOrInsertPair(List<string> terms, string docID, Dictionary<string, string> links)
        {
            m_dbConnection.Open();
            SQLiteCommand sqlCommand;
            sqlCommand = new SQLiteCommand("begin", m_dbConnection);
            sqlCommand.ExecuteNonQuery();
            Dictionary<string, int> termsList = new Dictionary<string, int>();


            foreach (string term in terms)
            {
                if (!termsList.ContainsKey(term))
                {
                    termsList.Add(term, 0);
                }
                termsList[term] += 1;
            }
            foreach (var term in termsList)
            {
                //Insert term into term table and get the current or new id back
                long newTermID = InsertTermInDb(term.Key);
                //Insert Ref from Doc to term and the amount of instances in the doc
                UpdateOrInsertDoc(docID, newTermID, term.Value);
            }

            insertDocLinks(docID, links.Keys.ToList<string>());
            insertDockument(docID);

            sqlCommand = new SQLiteCommand("end", m_dbConnection);
            sqlCommand.ExecuteNonQuery();
            m_dbConnection.Close();
        }

        private void insertDocLinks(string startLink, List<string> endLinks)
        {
            foreach(string endlink in endLinks)
            {
                using (SQLiteCommand command =
                new SQLiteCommand(
                    "Insert into " + DocLinks + " ( " + startDoc + " , " + endDoc + " ) VALUES ( '" + startLink + "' , '" + endlink + "' )", m_dbConnection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void getTransitionProbabilityMatrix()
        {

            int totalDoc = TotalDocuments();
            m_dbConnection.Open();
            double[,] TransProbMatrix = new double[totalDoc, totalDoc];
            for (int i = 0; i < totalDoc; i++)
            {
                for (int j = 0; j < totalDoc; j++)
                {
                    TransProbMatrix[i, j] = 0;
                }
            }
            //Select DocID, endID from (SELECT StartDoc ,DocID AS endID FROM 'DocLinks', 'DocumentIndex' where EndDoc =  DocDomainName) , 'DocumentIndex' where StartDoc = DocDomainName;
            using (SQLiteCommand command =
            new SQLiteCommand("Select " + docIndexID + ", endID From ( SELECT StartDoc, DocID AS endID FROM " + DocLinks + " , " + DocumentsIndex + " where EndDoc = DocDomainName) , " + DocumentsIndex + " Where " + startDoc + " = " + docDomainName, m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TransProbMatrix[reader.GetInt32(0) - 1, reader.GetInt32(1) - 1] = 1;
                    }
                }
            }
            for (int i = 0; i < totalDoc; i++)
            {
                double totalLinks = 0;
                for (int j = 0; j < totalDoc; j++)
                {
                    totalLinks += TransProbMatrix[i, j];
                }
                if (totalLinks == 0)
                    continue;
                for (int j = 0; j < totalDoc; j++)
                {
                    TransProbMatrix[i, j] /= totalLinks;
                }
                
            }
            for (int i = 0; i < totalDoc; i++)
            {
                double totalLinks = 0;
                for (int j = 0; j < totalDoc; j++)
                {
                    totalLinks += TransProbMatrix[i, j];
                    TransProbMatrix[i, j] *= 0.9f;
                    
                }
                
                for (int j = 0; j < totalDoc; j++)
                {
                    if (totalLinks == 0)
                        TransProbMatrix[i, j] += (1f / (double)totalDoc);
                    else
                        TransProbMatrix[i, j] += (1f / (double)totalDoc) * 0.1f;
                }
            }
            for (int i = 0; i < totalDoc; i++)
            {
                double total = 0;
                for (int j = 0; j < totalDoc; j++)
                {
                    total += TransProbMatrix[i, j];
                }
            }
            m_dbConnection.Close();
            calcutateRandomSurferPageRank(TransProbMatrix);
        }

        private void calcutateRandomSurferPageRank(double[,] TransProbMatrix)
        {

            int totalDoc = TotalDocuments();
            int steps = 50;
            double[] pageRank = new double[totalDoc];
            
            Random rnd = new Random();
            pageRank[rnd.Next(0, totalDoc)] = 1;
            for(int i = 0; i < steps; i++)
            {
                double[] newPageRank = new double[totalDoc];
                for (int j = 0; j < totalDoc; j++)
                {
                    for (int k = 0; k < totalDoc; k++)
                    {
                        newPageRank[k] += TransProbMatrix[j, k] * pageRank[j];
                    }
                }
                pageRank = newPageRank;
            }
            insertPageRank(pageRank);
        }

        private void insertPageRank(double[] pageRank)
        {

            m_dbConnection.Open();
            SQLiteCommand sqlCommand;
            sqlCommand = new SQLiteCommand("begin", m_dbConnection);
            sqlCommand.ExecuteNonQuery();
            for(int i = 0; i < pageRank.Length; i++)
            {
                string commandString = "Update " + DocumentsIndex + " SET " + docPageRank + " = " + pageRank[i] + " WHERE " + docIndexID + " = " + (i + 1);
                commandString = commandString.Replace(",", ".");
                using (SQLiteCommand command =
                new SQLiteCommand(
                    commandString, m_dbConnection))
                {
                    command.ExecuteNonQuery();
                }
            }
            

            sqlCommand = new SQLiteCommand("end", m_dbConnection);
            sqlCommand.ExecuteNonQuery();
            m_dbConnection.Close();
        }

        private long InsertTermInDb(string term)
        {
            long termID = -1;

            using (SQLiteCommand command =
                new SQLiteCommand("SELECT TermID FROM " + Terms + " where Term = '" + term + "'", m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetInt32(0);
                    }
                }
            }
            using (SQLiteCommand command =
                new SQLiteCommand(
                    "Insert into " + Terms + "( " + termCol + " ) Values ( '" + term + "' ); Select " + termIDCol +
                    " FROM " + Terms + " WHERE " + termCol + " = '" + term + "'", m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    termID = reader.GetInt32(0);
                }
            }

            return termID;
        }

        public Dictionary<string, double> getPageRank()
        {
            Dictionary<string, double> pageRanks = new Dictionary<string, double>();
            using (SQLiteCommand command = new SQLiteCommand("SELECT " + docDomainName + " , " + docPageRank + " FROM " + DocumentsIndex, m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pageRanks.Add(reader.GetString(0), reader.GetDouble(1));
                    }
                }
            }
            return pageRanks;
        }
        private void UpdateOrInsertDoc(string docID, long termID, int termCount)
        {
            using (SQLiteCommand command =
                new SQLiteCommand(
                    "Insert into " + Documents + "( " + docIDCol + " , " + docTermFKCol + " , " + docTermCountCol +
                    " ) VALUES ( \"" + docID + "\" , " + termID + " , " + termCount + " )", m_dbConnection))
            {
                command.ExecuteNonQuery();
            }
        }


        private void insertDockument(string docID)
        {
            using (SQLiteCommand command =
                new SQLiteCommand(
                    "Insert into " + DocumentsIndex + "(  " + docDomainName + " ) VALUES (  '" + docID + "' )", m_dbConnection))
            {
                command.ExecuteNonQuery();
            }
        }

        public List<string> QueryTerm(string term)
        {
            List<string> results = new List<string>();
            int termID = -1;

            m_dbConnection.Open();

            using (SQLiteCommand command =
                new SQLiteCommand("SELECT " + termIDCol + " FROM " + Terms + " WHERE " + termCol + " = '" + term + "'",
                    m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    for (int i = 0; i < reader.StepCount; i++)
                    {
                        reader.Read();
                        termID = reader.GetInt32(0);
                    }
                }
            }


            using (SQLiteCommand command =
                new SQLiteCommand(
                    "SELECT " + docIDCol + " FROM " + Documents + " WHERE " + docTermFKCol + " = " + termID,
                    m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            results.Add(reader.GetString(0));
                        }
                    }
                }
            }

            m_dbConnection.Close();

            return results;
        }

        public int TotalDocuments()
        {
            m_dbConnection.Open();
            int result;
            using (SQLiteCommand command = new SQLiteCommand("Select count(*) from (Select count( "+docIndexID+" ) From "+DocumentsIndex+ " group by " + docIndexID + " )", m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    result = reader.GetInt32(0);
                }
            }
            m_dbConnection.Close();
            return result;
        }

        public int TermFrequency(string term)
        {
            m_dbConnection.Open();
            return 1;
        }
    }
}