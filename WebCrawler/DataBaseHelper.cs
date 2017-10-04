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

        private const string DocTalbe = "DocTable";
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
            string sqlTable2 = "create table " + DocTalbe + " (" + DocCol1 + " TEXT , " + DocCol2 + " INTEGER , " + DocCol3 + " INTEGER )";

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
            if(reader.StepCount == 0)
            {
                command = new SQLiteCommand("Insert into " + TermTable + "( " + termCol2 + " ) Values ( '" + term + "' )", m_dbConnection);
                command.ExecuteNonQuery();
                command = new SQLiteCommand("select * From " + TermTable + " WHERE " + termCol2 + " = \"" + term + "\"", m_dbConnection);
                reader = command.ExecuteReader();
            }
            reader.Read();
            termID = reader.GetInt32(0);
            
                

            command = new SQLiteCommand("select " + DocCol1 + " From " + DocTalbe + " WHERE " + DocCol1 + " = \"" + docId + "\" AND " + termCol1 + " = "+termID, m_dbConnection);
            reader = command.ExecuteReader();
            if (reader.StepCount == 0)
            {
                command = new SQLiteCommand("Insert into " + DocTalbe + "( " + DocCol1 + " , " + DocCol2 + " , " + DocCol3 + " ) Values ( \"" + docId + "\" , " + termID + " , " + 1 + " )", m_dbConnection);
                command.ExecuteNonQuery();
            }
            else
            {
                command = new SQLiteCommand("UPDATE " + DocTalbe + " SET " + DocCol3 + " = " + DocCol3 + " + 1 WHERE " + DocCol1 + " = \" AND " + termCol1 + " = " + termID, m_dbConnection);
                command.ExecuteNonQuery();
            }

            m_dbConnection.Close();
        }

        public int[] getTermCount(string docID)
        {
            SQLiteCommand command = new SQLiteCommand("select * From " + DocTalbe + " WHERE " + DocCol1 + " = \"" + docID + "\"", m_dbConnection);

            m_dbConnection.Open();
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

        public void CloseDatabase()
        {
            m_dbConnection.Close();
        }
    }
}
