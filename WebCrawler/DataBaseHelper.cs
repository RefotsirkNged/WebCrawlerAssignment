﻿using System;
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

        private const string InvertTerm = "InvertedTerms";
        private const string invertTermCol = "Term";
        private const string invertDocFrikent = "DocFrequnt";

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

                string sqlTable1 = "create table " + Terms + " (" + termIDCol + " INTEGER PRIMARY KEY AUTOINCREMENT , " + termCol + " TEXT UNIQUE)";
                string sqlTable2 = "create table " + Documents + " (" + docIDCol + " TEXT , " + docTermFKCol + " INTEGER , " + docTermCountCol + " INTEGER )";

                SQLiteCommand command = new SQLiteCommand(sqlTable1, m_dbConnection);
                command.ExecuteNonQuery();
                command = new SQLiteCommand(sqlTable2, m_dbConnection);
                command.ExecuteNonQuery();
            }
            else
            {
                m_dbConnection = new SQLiteConnection("Data Source=" + Directory.GetCurrentDirectory() + "\\" + DatabaseName + ";Version=3;");
            }


            m_dbConnection.Close();
        }

        //TODO Need to be tested but should work
        public Dictionary<string, TermVector> CreateInvertedTerms()
        {
            m_dbConnection.Open();
            SQLiteCommand sqlCommand;
            Dictionary<string, TermVector>  invertList = new Dictionary<string, TermVector>();
            using (SQLiteCommand command = new SQLiteCommand("Select " + termIDCol + " , " + termCol + " From " + Terms, m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    for (int i = 0; i < reader.StepCount; i++)
                    {
                        reader.Read();
                        string term = reader.GetString(0);
                        int id = reader.GetInt32(1);
                        invertList.Add(term, new TermVector(term, id));
                    }
                }
            }

           foreach(TermVector elm in invertList.Values)
           {
                using (SQLiteCommand command = new SQLiteCommand("Select " + docIDCol + " From " + Documents + " Where " + docTermFKCol + " = " + elm.ID, m_dbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        for (int i = 0; i < reader.StepCount; i++)
                        {
                            reader.Read();
                            string doc = reader.GetString(0);
                            elm.documents.Add(doc);
                        }
                    }
                }
            }

            m_dbConnection.Close();
            return invertList;
        }


        public void UpdateOrInsertPair(List<string> terms, string docID)
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
            foreach(var term in termsList)
            {
                //Insert term into term table and get the current or new id back
                long newTermID = InsertTermInDb(term.Key);
                //Insert Ref from Doc to term and the amount of instances in the doc
                UpdateOrInsertDoc(docID, newTermID, term.Value);
            }
            

            sqlCommand = new SQLiteCommand("end", m_dbConnection);
            sqlCommand.ExecuteNonQuery();
            m_dbConnection.Close();
        }

        private long InsertTermInDb(string term)
        {
            long termID = -1;
            
            using (SQLiteCommand command = new SQLiteCommand("Insert or Ignore into " + Terms + "( " + termCol + " ) Values ( '" + term + "' ); Select " + termIDCol + " FROM " + Terms + " WHERE " + termCol + " = '" + term + "'", m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {

                        reader.Read();
                        termID = reader.GetInt32(0);
                    
                }
            }

            return termID;
        }

        private void UpdateOrInsertDoc(string docID,  long termID, int termCount)
        {

            using (SQLiteCommand command = new SQLiteCommand("Insert into " + Documents + "( " + docIDCol + " , " + docTermFKCol + " , " + docTermCountCol + " ) VALUES ( \"" + docID + "\" , " +  termID + " , " + termCount + " )", m_dbConnection))
            {
                command.ExecuteNonQuery();
            }
        }

       /* private int GetTermID(string term)
        {
            int termID = -1;
            using (SQLiteCommand command = new SQLiteCommand("select " + termIDCol + " From " + Terms + " WHERE " + termCol + " = \"" + term + "\"", m_dbConnection))
            {
                SQLiteDataReader reader = command.ExecuteReader();
                termID = reader.GetInt32(0);
            }
            return termID;
        }

        private DBStatus GetTermDBStatus(string term)
        {
            DBStatus termStatus;
            string readDocID = "select count(*) From " + Terms + " WHERE " + termCol + " = \"" + term + "\"";

            using (SQLiteCommand command = new SQLiteCommand(readDocID, m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    termStatus = reader.HasRows ? DBStatus.NotExist : DBStatus.Exist;
                }
            }
            return termStatus;
        }

        private DBStatus GetDocDBStatus(string docID)
        {
            DBStatus docStatus;
            string readDocStatus = "select count(*) From " + Documents + " WHERE " + docIDCol + " = \"" + docID + "\"";
            using (SQLiteCommand command = new SQLiteCommand(readDocStatus, m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    docStatus = reader.HasRows ? DBStatus.NotExist : DBStatus.Exist;
                }
            }
            return docStatus;
        }



       /* public void InsertTerm(string term, string docId)
        {
            m_dbConnection.Open();
            int termID = 0;
            DBStatus termStatus = GetTermDBStatus(term);
            SQLiteCommand command = new SQLiteCommand("select * From " + Terms + " WHERE " + termCol + " = \"" + term + "\"", m_dbConnection);

            #region Insert/update Term
            SQLiteDataReader reader = command.ExecuteReader();
            command.Dispose();
            if (reader.StepCount == 0)
            {
                command = new SQLiteCommand("Insert into " + Terms + "( " + termCol + " ) Values ( '" + term + "' )", m_dbConnection);
                command.ExecuteNonQuery();
                command.Dispose();
                command = new SQLiteCommand("select * From " + Terms + " WHERE " + termCol + " = \"" + term + "\"", m_dbConnection);
                reader = command.ExecuteReader();
                command.Dispose();
            }

            reader.Read();
            termID = reader.GetInt32(0);
            reader.Dispose();



            command = new SQLiteCommand("select " + docIDCol + " From " + Documents + " WHERE " + docIDCol + " = \"" + docId + "\" AND " + termIDCol + " = " + termID, m_dbConnection);
            reader = command.ExecuteReader();
            command.Dispose();


            #endregion

            if (reader.StepCount == 0)
            {
                command = new SQLiteCommand("Insert into " + Documents + "( " + docIDCol + " , " + docTermFKCol + " , " + docTermCountCol + " ) Values ( \"" + docId + "\" , " + termID + " , " + 1 + " )", m_dbConnection);
                command.ExecuteNonQuery();
            }
            else
            {
                command = new SQLiteCommand("UPDATE " + Documents + " SET " + docTermCountCol + " = " + docTermCountCol + " + 1 WHERE " + docIDCol + " = '" + docId + "' AND " + termIDCol + " = " + termID, m_dbConnection);
                command.ExecuteNonQuery();
            }
            reader.Dispose();
            command.Dispose();
            m_dbConnection.Close();
        }*/

        /*public int[] getTermCount(string docID)
        {
            SQLiteCommand command = new SQLiteCommand("select * From " + Documents + " WHERE " + docIDCol + " = \"" + docID + "\"", m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            int[] count = new int[reader.StepCount];
            for (int i = 0; i < reader.StepCount; i++)
            {
                reader.Read();
                count[i] = reader.GetInt32(2);
            }

            m_dbConnection.Close();
            return count;
        }*/

        public List<string> QueryTerm(string term)
        {
            List<string> results = new List<string>();
            int termID = -1;

            m_dbConnection.Open();

            using (SQLiteCommand command = new SQLiteCommand("SELECT " + termIDCol + " FROM " + Terms + " WHERE " + termCol + " = '" + term + "'", m_dbConnection))
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


            using (SQLiteCommand command = new SQLiteCommand("SELECT " + docIDCol + " FROM " + Documents + " WHERE " + docTermFKCol + " = " + termID , m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows) {
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

       /* public void CloseDatabase()
        {
            m_dbConnection.Close();
        }*/
    }
}
