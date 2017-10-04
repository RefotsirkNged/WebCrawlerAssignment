using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace WebCrawler
{
    class DatabaseHelper
    {
        private const string DatabaseName = "TermDatabase.sqlite";

        private const string TermTable = "TermTable";
        private const string termCol1 = "TermID";
        private const string termCol2 = "Term";

        private const string DocTable = "DocTable";
        private const string DocCol1 = "ID";
        private const string DocCol2 = termCol1;
        private const string DocCol3 = "TermCount";

        private SQLiteConnection m_dbConnection;

        public DatabaseHelper()
        {
            SQLiteConnection.CreateFile(DatabaseName);

            m_dbConnection = new SQLiteConnection("Data Source=" + DatabaseName + ";Version=3;");
            m_dbConnection.Open();

            string sqlTable1 = "create table " + TermTable + " (" + termCol1 + " INTEGER PRIMARY KEY AUTOINCREMENT , " + termCol2 + " TEXT)";
            string sqlTable2 = "create table " + DocTable + " (" + DocCol1 + " TEXT , " + DocCol2 + " INTEGER , " + DocCol3 + " INTEGER )";

            SQLiteCommand command = new SQLiteCommand(sqlTable1, m_dbConnection);
            command.ExecuteNonQuery();
            command = new SQLiteCommand(sqlTable2, m_dbConnection);
            command.ExecuteNonQuery();

            m_dbConnection.Close();
        }

        public void InsertTerm(string term, string docId)
        {
            int termID = 0;
            SQLiteCommand command = new SQLiteCommand("select * From " + TermTable + " WHERE " + termCol2 + " = \"" + term + "\"", m_dbConnection);

            m_dbConnection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            command.Dispose();
            if(reader.StepCount == 0)
            {
                command = new SQLiteCommand("Insert into " + TermTable + "( " + termCol2 + " ) Values ( '" + term + "' )", m_dbConnection);
                command.ExecuteNonQuery();
                command.Dispose();
                command = new SQLiteCommand("select * From " + TermTable + " WHERE " + termCol2 + " = \"" + term + "\"", m_dbConnection);
                reader = command.ExecuteReader();
                command.Dispose();
            }

            reader.Read();
            termID = reader.GetInt32(0);
            reader.Dispose();



            command = new SQLiteCommand("select " + DocCol1 + " From " + DocTable + " WHERE " + DocCol1 + " = \"" + docId + "\" AND " + termCol1 + " = "+termID, m_dbConnection);
            reader = command.ExecuteReader();
            command.Dispose();
            if (reader.StepCount == 0)
            {
                command = new SQLiteCommand("Insert into " + DocTable + "( " + DocCol1 + " , " + DocCol2 + " , " + DocCol3 + " ) Values ( \"" + docId + "\" , " + termID + " , " + 1 + " )", m_dbConnection);
                command.ExecuteNonQuery();
            }
            else
            {
                command = new SQLiteCommand("UPDATE " + DocTable + " SET " + DocCol3 + " = " + DocCol3 + " + 1 WHERE " + DocCol1 + " = '" + docId + "' AND " + termCol1 + " = " + termID, m_dbConnection);
                command.ExecuteNonQuery();
            }
            reader.Dispose();
            command.Dispose();
            m_dbConnection.Close();
            Console.WriteLine(termID + docId);
        }

        public int[] getTermCount(string docID)
        {
            SQLiteCommand command = new SQLiteCommand("select * From " + DocTable + " WHERE " + DocCol1 + " = \"" + docID + "\"", m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            int[] count = new int[reader.StepCount];
            for(int i = 0; i < reader.StepCount; i++)
            {
                reader.Read();
                count[i] = reader.GetInt32(2);
            }

            m_dbConnection.Close();
            return count;
        }

        public List<string> QueryTerm(string term)
        {
            List<string> results = new List<string>();
            int termID = -1;

            m_dbConnection.Open();

            using (SQLiteCommand command = new SQLiteCommand("SELECT " + termCol1 + " FROM " + TermTable + " WHERE " + termCol2 + " = '" + term + "'", m_dbConnection))
            {
                using(SQLiteDataReader reader = command.ExecuteReader())
                {
					for (int i = 0; i < reader.StepCount; i++)
					{
						reader.Read();
						termID = reader.GetInt32(0);
					}
                }
            }


            using (SQLiteCommand command = new SQLiteCommand("SELECT " + DocCol1 + " FROM " + DocTable + " WHERE " + DocCol2 + " = '" + termID + "'", m_dbConnection))
			{
				using (SQLiteDataReader reader = command.ExecuteReader())
				{
					for (int i = 0; i < reader.StepCount; i++)
					{
						reader.Read();
                        results.Add(reader.GetString(0));
					}
				}
			}

			m_dbConnection.Close();

            return results;
        } 

        public void CloseDatabase()
        {
            m_dbConnection.Close();
        }
    }
}
