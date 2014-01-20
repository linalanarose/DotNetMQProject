using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using System.Xml;

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

        private static int mDelay;
        private static int mMaxCount;
        private int mCount;
        //private static int mMaxSize;
        //private int mSize;
        private String mDeliveryPath;
        private FileInfo mDBFileInfo;
        private static String mFilePath = "C:/SQLiteDataBase/MessageDatabase.sqlite";
        public SQLiteConnection dbConnection;

        #region Constructors
        /// <summary>
        /// Constructor: called by the sender that creates the database and sets up the max number of messages
        /// for the database. If the database doesn't exist it creates that and a table.
        /// </summary>
        /// <param name="maxMsgs">The cap for how many messages the queue can hold</param>
        public SQLiteDatabase(int maxMsgs)
        {

            if (File.Exists(mFilePath) == false)
            {
                SQLiteConnection.CreateFile(mFilePath);
            }
            //make a database or open the existing one
            dbConnection = new SQLiteConnection("Data Source = " + mFilePath + ";Version=3;");
            mDBFileInfo = new FileInfo(mFilePath);
            //create table if not existing
            dbConnection.Open();
            string sql = "CREATE TABLE IF NOT EXISTS messages (msgID INT, message VARCHAR(50))";
            ExecuteSQL(sql);
            mCount = GetNumOfMsgs();
            dbConnection.Close();
            mMaxCount = maxMsgs;
        }

        //public SQLiteDatabase(int maxSize) 
        //{
        //    if (File.Exists(mFilePath) == false)
        //    {
        //        SQLiteConnection.CreateFile(mFilePath);
        //    }
        //    //make a database or open the existing one
        //    dbConnection = new SQLiteConnection("Data Source = " + mFilePath + ";Version=3;");
        //    mDBFileInfo = new FileInfo(mFilePath);
        //    //create table if not existing
        //    dbConnection.Open();
        //    string sql = "CREATE TABLE IF NOT EXISTS messages (time VARCHAR(50), message VARCHAR(50))";
        //    ExecuteSQL(sql);
        //    mSize = (int) mDBFile.Length;
        //    dbConnection.Close();
        //    mMaxSize = maxSize;
        //}

        /// <summary>
        /// Constructor: Called by the receiver, checks for an existing database and creates one if necessary.
        /// </summary>
        /// <param name="filePath"> The path to the desired output file.</param>
        /// <param name="aDelay">The desired delay between message deliveries</param>

        public SQLiteDatabase(String filePath, int delay)
        {
            if (File.Exists(mFilePath) == false)
            {
                SQLiteConnection.CreateFile(mFilePath);
            }
            //make a database or open the existing one
            dbConnection = new SQLiteConnection("Data Source = " + mFilePath + ";Version=3;");
            //create table if not existing
            dbConnection.Open();
            string sql = "CREATE TABLE IF NOT EXISTS messages (msgID INT, message VARCHAR(50))";
            ExecuteSQL(sql);
            mCount = GetNumOfMsgs();
            dbConnection.Close();

            mDelay = delay;
            mDeliveryPath = filePath;
        }
        #endregion
        #region Methods
        /// <summary>
        /// Makes a message and inserts it at the next point in the table
        /// </summary>
        /// <param name="message">The message to be sent</param>
        /// <remarks>
        /// We will have to make the param type generic in the future
        /// </remarks>
        public void CreateMessage(String message)
        {
            //if the queue isn't "full"
            if (mCount < mMaxCount)
            {
                dbConnection.Open();
                String sql = "INSERT INTO messages (msgID, message) VALUES (" + mCount + ", '" + message + "')";
                ExecuteSQL(sql);
                mCount++;
                dbConnection.Close();
            }
            else
            {
                //call delete oldest message and try to create again
                DeleteOldestMessage();
                CreateMessage(message);
            }
        }

		  //public void CreateMessage(String msgPath)
		  //{
		  //	 FileInfo msgFileInfo = new FileInfo(msgPath);
		  //	 //if the queue isn't "full"
		  //	 if (mSize + (int)msgFileInfo.Length < mMaxSize)
		  //	 {
		  //		  dbConnection.Open();
		  //		  using (StreamReader reader = new StreamReader(msgPath, false))
		  //		  {
		  //				string msg = "";
		  //				while ((msg = reader.ReadLine()) != null)
		  //				{
		  //					 msg = msg.Trim();
		  //					 if (string.IsNullOrEmpty(msg) == false)
		  //					 {
		  //						  String sql = "INSERT INTO messages (time, message) VALUES (" + mCount + ",'" + msg + "')";
		  //					 }
		  //				}
		  //		  }
		  //		  mSize += (int)msgFileInfo.Length;
		  //	 }
		  //	 else
		  //	 {
		  //		  while (mSize + (int)msgFileInfo.Length > mMaxSize)
		  //		  {
		  //				DeleteOldestMessage();
		  //		  }
		  //		  CreateMessage();
		  //	 }
		  //}

        /// <summary>
        /// Selects all messages in queue and lists them in console.
        /// </summary>
        public void ListMessage()
        {
            dbConnection.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM messages", dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                Console.WriteLine("msgID: " + reader["msgID"] + "\tMessage: " + reader["message"]);
            dbConnection.Close();
        }

   /*
        public void ReceiveAllMsgs()
        {
            dbConnection.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM messages", dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine("Message: " + reader["message"]);
                System.IO.File.WriteAllText(mDeliveryPath + reader["time"].ToString() + ".xml", reader["message"].ToString());
                System.Threading.Thread.Sleep(mDelay);
            }
            String sql = "DELETE FROM messages";
            ExecuteSQL(sql);
            dbConnection.Close();
        }
    */
        
        /// <summary>
        /// Deliver all the messages and clear the database
        /// </summary>
        /// <returns>Array of String messages</returns>
        /// <remarks>
        /// This will eventually have to be modified for generic types.
        /// </remarks>
        public String[] ReceiveAllMsgs()
        {
            dbConnection.Open();
            int rowCount = this.GetNumOfMsgs();
            int count = 0;
            String[] ret = new String[rowCount];
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM messages", dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine("Message: " + reader["message"]);
                ret[count] = reader["message"].ToString();
                count++;
                System.Threading.Thread.Sleep(mDelay);
            }
            System.IO.File.WriteAllLines(mDeliveryPath, ret);
            String sql = "DELETE FROM messages";
            ExecuteSQL(sql);
            dbConnection.Close();
            return ret;
        }
        
        /// <summary>
        /// Deletes the oldest message in the queue
        /// </summary>
        private void DeleteOldestMessage()
        {
            dbConnection.Open();
            string sql = "DELETE FROM messages WHERE msgID = 0";
            ExecuteSQL(sql);
            dbConnection.Close();
            DecrementMsgID();
            mCount--;
        }

		  //private void DeleteOldestMessage()
		  //{
		  //	 dbConnection.Open();
		  //	 string sql = "DELETE FROM messages WHERE ROWID IN (SELECT ROWID FROM messages ORDER BY ROWID ASC LIMIT 1)";
		  //	 ExecuteSQL(sql);
		  //	 dbConnection.Close();
		  //	 mSize = mDBFileInfo.Length;
		  //}

        /// <summary>
        /// Moves all messages up in the queue decrementing their msgID
        /// </summary>
        private void DecrementMsgID()
        {
            dbConnection.Open();
            ExecuteSQL("UPDATE messages SET msgID = msgID - 1");
            dbConnection.Close();
        }
        /// <summary>
        /// Simplifies the process of executing SQLite code
        /// </summary>
        /// <param name="sql">The string of code to be executed in SQLite</param>
        private void ExecuteSQL(String sql)
        {
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();
        }
        /// <summary>
        /// Get the number of messages in the cache
        /// </summary>
        /// <returns>The number of messages in the cache</returns>
        private int GetNumOfMsgs()
        {
            SQLiteCommand command = new SQLiteCommand("SELECT COUNT(msgID) from messages", dbConnection);
            return Convert.ToInt32(command.ExecuteScalar());
        }
        #endregion
    }
}
        