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
		  private static int mMaxSize;
		  private int mSize;
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
		  public SQLiteDatabase(int maxSize)
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
				string sql = "CREATE TABLE IF NOT EXISTS messages (time VARCHAR(50), message VARCHAR(50))";
				ExecuteSQL(sql);
				mSize = (int)mDBFileInfo.Length;
				dbConnection.Close();
				mMaxSize = maxSize;
		  }

        /// <summary>
        /// Constructor: Called by the receiver, checks for an existing database and creates one if necessary.
        /// </summary>
        /// <param name="filePath"> The path to the desired output directory.</param>
        /// <param name="aDelay">The desired delay between message deliveries</param>

        public SQLiteDatabase(String deliveryPath, int delay)
        {
            if (File.Exists(mFilePath) == false)
            {
                SQLiteConnection.CreateFile(mFilePath);
            }
            //make a database or open the existing one
            dbConnection = new SQLiteConnection("Data Source = " + mFilePath + ";Version=3;");
            //create table if not existing
            dbConnection.Open();
            string sql = "CREATE TABLE IF NOT EXISTS messages (time VARCHAR(50), message TEXT)";
            ExecuteSQL(sql);
				mDBFileInfo = new FileInfo(mFilePath);
				mSize = (int)mDBFileInfo.Length;
            dbConnection.Close();

            mDelay = delay;
            mDeliveryPath = deliveryPath;
        }
        #endregion
        #region Methods
        /// <summary>
        /// Makes a message and inserts it at the next point in the table
        /// </summary>
        /// <param name="message">The path to the XML file to be sent</param>
        /// <remarks>
        /// We will have to make the param type generic in the future
        /// </remarks>
<<<<<<< HEAD
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
		  //						  String sql = "INSERT INTO messages (time, message) VALUES (" + DateTime.Now.TimeOfDay.ToString() + ",'" + msg + "')";
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
=======
		  public void CreateMessage(String msgPath)
		  {
				FileInfo msgFileInfo = new FileInfo(msgPath);
				//if the queue isn't "full"
				if (mSize + (int)msgFileInfo.Length < mMaxSize)
				{
					 dbConnection.Open();
					 StreamReader sr = new StreamReader(msgPath);
					 String msg = sr.ReadToEnd();
					 msg = msg.Replace("'", "''");
					 Console.WriteLine(msg);
					 String sql = "INSERT INTO messages (time, message) VALUES (" + DateTime.Now.TimeOfDay.ToString().Replace(':', '-').Replace('.', '-') + ",'" + msg + "')";
					 ExecuteSQL(sql);
					 dbConnection.Close();
					 mSize += (int)msgFileInfo.Length;
				}
				else
				{
					 while (mSize + (int)msgFileInfo.Length > mMaxSize)
					 {
						  DeleteOldestMessage();
					 }
					 CreateMessage(msgPath);
				}
		  }
		  /// <summary>
		  /// Delivers each message to a file denoted by the time it was received
		  /// </summary>
>>>>>>> 9bd524d0965d57f025b9db67ac6dc74a3d2e21c3
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
        
        /// <summary>
        /// Deletes the oldest message in the queue
        /// </summary>
		  private void DeleteOldestMessage()
		  {
				dbConnection.Open();
				string sql = "DELETE FROM messages WHERE ROWID IN (SELECT ROWID FROM messages ORDER BY ROWID ASC LIMIT 1)";
				ExecuteSQL(sql);
				dbConnection.Close();
				mSize = (int)mDBFileInfo.Length;
		  }

		  private string ConvertXMLtoString(string path)
		  {
				XmlDocument xmlFile = new XmlDocument();
				xmlFile.Load(path);
				string xmlString = xmlFile.OuterXml;
				return xmlString;
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
        #endregion
    }
}
        
