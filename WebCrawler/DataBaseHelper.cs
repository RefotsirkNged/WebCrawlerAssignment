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

        private enum DBStatus
        {
            NotExist = 1,
            Exist
        }

        private SQLiteConnection m_dbConnection;

        public DatabaseHelper()
        {
            //TODO: add some kind of check to see if the file already exists? or is that already handled
            if (!File.Exists(Directory.GetCurrentDirectory() +  "//TermDatabase.sqlite"))
            {
                SQLiteConnection.CreateFile(DatabaseName);
                m_dbConnection = new SQLiteConnection("Data Source=" + DatabaseName + ";Version=3;");
                m_dbConnection.Open();

                string sqlTable1 = "create table " + Terms + " (" + termIDCol + " INTEGER PRIMARY KEY AUTOINCREMENT , " + termCol + " TEXT)";
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


        public void UpdateOrInsertPair(string term, string docID)
        {
            m_dbConnection.Open();

            int termID = 0;
            DBStatus termStatus = GetTermDBStatus(term);

            switch (termStatus)
            {
                case DBStatus.NotExist:
                    int newTermID = InsertTermInDb(term);
                    UpdateOrInsertDoc(docID, newTermID); 
                    break;

                case DBStatus.Exist:
                    //We do not need to update the term in this case just the document
                    termID = GetTermID(term);
                    UpdateOrInsertDoc(docID, termID);
                    break;
                default:
                    Console.WriteLine("Something went wrong");
                    break;
            }

            m_dbConnection.Close();
        }

        private int InsertTermInDb(string term)
        {
            int termID = -1;
            //Insert the term, get the termID out, to use when inserting the doc
            using (SQLiteCommand command = new SQLiteCommand("Insert into " + Terms + "( " + termCol + " ) Values ( '" + term + "' )", m_dbConnection))
            {
                command.ExecuteNonQuery();
            }
            using (SQLiteCommand command = new SQLiteCommand("select * From " + Terms + " WHERE " + termCol + " = \"" + term + "\"", m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    termID = reader.GetInt32(0);
                }
            }
            return termID;
        }

        private void UpdateOrInsertDoc(string docID, int termID)
        {
            DBStatus docStatus = GetDocDBStatus(docID);

            switch (docStatus)
            {
                case DBStatus.NotExist:
                    using (SQLiteCommand command = new SQLiteCommand("Insert into " + Documents + "( " + docIDCol + " , " + docTermFKCol + " , " + docTermCountCol + " ) Values ( \"" + docID + "\" , " + termID + " , " + 1 + " )", m_dbConnection))
                    {
                        command.ExecuteNonQuery();
                    }
                    break;

                case DBStatus.Exist:
                    using (SQLiteCommand command = new SQLiteCommand("UPDATE " + Documents + " SET " + docTermCountCol + " = " + docTermCountCol + " + 1 WHERE " + docIDCol + " = '" + docID + "' AND " + termIDCol + " = " + termID, m_dbConnection))
                    {
                        command.ExecuteNonQuery();
                    }
                    break;
            }
        }

        private int GetTermID(string term)
        {
            int termID = -1;
            using (SQLiteCommand command = new SQLiteCommand("select * From " + Terms + " WHERE " + termCol + " = \"" + term + "\"", m_dbConnection))
            {
                SQLiteDataReader reader = command.ExecuteReader();
                termID = reader.GetInt32(0);
            }
            return termID;
        }
        private DBStatus GetTermDBStatus(string term)
        {
            DBStatus termStatus;
            string readDocID = "select * From " + Terms + " WHERE " + termCol + " = \"" + term + "\"";

            using (SQLiteCommand command = new SQLiteCommand(readDocID, m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    var temp = reader.StepCount;
                    if (temp <= 1)
                    {
                        termStatus = DBStatus.NotExist;
                    }
                    else
                        termStatus = DBStatus.Exist;
                }
            }
            return termStatus;
        }
        private DBStatus GetDocDBStatus(string docID)
        {
            DBStatus docStatus;
            string readDocStatus = "select * From " + Documents + " WHERE " + docIDCol + " =\"" + docID + "\"";
            using (SQLiteCommand command = new SQLiteCommand(readDocStatus, m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    docStatus = (DBStatus) reader.StepCount;
                }
            }
            return docStatus;
        }



        public void InsertTerm(string term, string docId)
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
        }

        public int[] getTermCount(string docID)
        {
            SQLiteCommand command = new SQLiteCommand("select * From " + Documents + " WHERE " + docIDCol + " = \"" + docID + "\"", m_dbConnection);
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

            using (SQLiteCommand command = new SQLiteCommand("SELECT " + termIDCol + " FROM " + Terms + " WHERE " + termCol + " = '" + term + "'", m_dbConnection))
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


            using (SQLiteCommand command = new SQLiteCommand("SELECT " + docIDCol + " FROM " + Documents + " WHERE " + docTermFKCol + " = '" + termID + "'", m_dbConnection))
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
