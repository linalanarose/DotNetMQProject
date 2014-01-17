using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace Database
{
   /// <summary>
   /// This class is for managing the SQLite based cache instance on the computer.
   /// </summary>
    public class SQLiteDatabase
    {
        private static int max_mCount;
        private static int delay;
        private int mCount;
        public SQLiteConnection m_dbConnection;
        /// <summary>
        /// creates database, sets message count to 0, delay, and max msg count, creates the table
        /// </summary>
        /// <param name="max_Count">The desired maximum messages allowed in the cache</param>
        /// <param name="delay_reads">The delay in milliseconds between message sends</param>
        public SQLiteDatabase(int max_Count, int delay_reads)
        {
            //make a database
            m_dbConnection = new SQLiteConnection("Data Source=MessageDatabase.sqlite;Version=3;");
            //create table
            m_dbConnection.Open();
            string sql = "CREATE TABLE IF NOT EXISTS messages (msgID INT, message VARCHAR(50))";
            executeSQL(sql);
            mCount = getNumOfMsgs();
            m_dbConnection.Close();
            //set counts
            max_mCount = max_Count;
            delay = delay_reads;
        }

        /// <summary>
        /// Makes a message and inserts it at the next point in the table
        /// </summary>
        /// <param name="message">The message to be sent</param>
        /// <remarks>
        /// We will have to make the param type generic in the future
        /// </remarks>
        public void createMessage(String message)
        {
            //if the queue isn't "full"
            if (mCount < max_mCount)
            {
                m_dbConnection.Open();
                string sql = "INSERT INTO messages (msgID, message) VALUES (" + mCount + ", '" + message + "')";
                executeSQL(sql);
                mCount++;
                m_dbConnection.Close();
            }
            else
            {
                //call delete oldest message and try to create again
                deleteOldestMessage();
                createMessage(message);
            }
        }


        /// <summary>
        /// Selects all messages in queue and lists them in console.
        /// </summary>
        public void listMessage()
        {
            m_dbConnection.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM messages", m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                Console.WriteLine("msgID: " + reader["msgID"] + "\tMessage: " + reader["message"]);
            m_dbConnection.Close();
        }
        
        /// <summary>
        /// Deliver all the messages and clear the database
        /// </summary>
        /// <returns>Array of String messages</returns>
        /// <remarks>
        /// This will eventually have to be modified for generic types.
        /// </remarks>
        public String[] receiveAllMsgs()
        {
            int rowCount = this.getNumOfMsgs();
            int count = 0;
            String[] ret = new String[rowCount];
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM messages", m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                ret[count] = reader["message"].ToString();
                count++;
                System.Threading.Thread.Sleep(delay);
            }
            String sql = "DELETE FROM messages";
            executeSQL(sql);
            return ret;
        }

        /// <summary>
        /// Deletes the oldest message in the queue
        /// </summary>
        private void deleteOldestMessage()
        {
            m_dbConnection.Open();
            string sql = "DELETE FROM messages WHERE msgID = 0";
            executeSQL(sql);
            m_dbConnection.Close();
            decrementmsgID();
            mCount--;
        }

        /// <summary>
        /// Moves all messages up in the queue decrementing their msgID
        /// </summary>
        private void decrementmsgID()
        {
            m_dbConnection.Open();
            executeSQL("UPDATE messages SET msgID = msgID - 1");
            m_dbConnection.Close();
        }
        /// <summary>
        /// Simplifies the process of executing SQLite code
        /// </summary>
        /// <param name="sql">The string of code to be executed in SQLite</param>
        private void executeSQL(String sql)
        {
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }
        /// <summary>
        /// Get the number of messages in the cache
        /// </summary>
        /// <returns>The number of messages in the cache</returns>
        private int getNumOfMsgs()
        {
            SQLiteCommand command = new SQLiteCommand("SELECT COUNT(msgID) from messages", m_dbConnection);
            int rowCount = Convert.ToInt32(command.ExecuteScalar());
            return rowCount;
        }
    }
}