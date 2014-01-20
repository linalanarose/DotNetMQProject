using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using System.IO;
namespace Database
{
   /// <summary>
   /// This class is for managing the SQLite based cache instance on the computer.
   /// </summary>
   /// <remarks>
   /// To use you must create a SQLiteDataBase directory in the C drive.
   /// </remarks>
    public class SQLiteDatabase
    {
        private static int max_mCount;
        private static int delay;
        private int mCount;
        private String deliveryPath;
        public SQLiteConnection m_dbConnection;

        /// <summary>
        /// Custructor: called by the sender that creates the database and sets up the max number of messages
        /// for the database. If the database doesn't exist it creates that and a table.
        /// </summary>
        /// <param name="maxMsgs">The cap for how many messages the queue can hold</param>
        public SQLiteDatabase(int maxMsgs)
        {
            if (!File.Exists("C:/SQLiteDataBase/MessageDatabase.sqlite"))
            {
                SQLiteConnection.CreateFile("C:/SQLiteDataBase/MessageDatabase.sqlite");
            }
            //make a database or open the existing one
            m_dbConnection = new SQLiteConnection("Data Source = C:/SQLiteDataBase/MessageDatabase.sqlite;Version=3;");
            //create table if not existing
            m_dbConnection.Open();
            string sql = "CREATE TABLE IF NOT EXISTS messages (msgID INT, message VARCHAR(50))";
            executeSQL(sql);
            mCount = getNumOfMsgs();
            m_dbConnection.Close();
            max_mCount = maxMsgs;
        }
        /// <summary>
        /// Constructor: Called by the receiver, checks for an existing database and creates one if necessary.
        /// </summary>
        /// <param name="filePath"> The path to the desired output file.</param>
        /// <param name="aDelay">The desired delay between message deliveries</param>

        public SQLiteDatabase(String filePath, int aDelay)
        {
            
            if(!File.Exists("C:/SQLiteDataBase/MessageDatabase.sqlite"))
            {
                SQLiteConnection.CreateFile("C:/SQLiteDataBase/MessageDatabase.sqlite");
            }
            //make a database or open the existing one
            m_dbConnection = new SQLiteConnection("Data Source = C:/SQLiteDataBase/MessageDatabase.sqlite;Version=3;");
            //create table if not existing
            m_dbConnection.Open();
            string sql = "CREATE TABLE IF NOT EXISTS messages (msgID INT, message VARCHAR(50))";
            executeSQL(sql);
            mCount = getNumOfMsgs();
            m_dbConnection.Close();

            delay = aDelay;
            deliveryPath = filePath;
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
            m_dbConnection.Open();
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
            m_dbConnection.Close();
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
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }
}