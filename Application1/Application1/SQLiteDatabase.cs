﻿using System;
using System.Collections;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Collections.Specialized;
using System.Configuration;

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
		  private static int mMaxSize;
		  private int mDBSize;
		  private static int mLastMsgID;
        private String mDeliveryPath;
        private FileInfo mDBFileInfo;
        private static String mFilePath;
        public SQLiteConnection dbConnection;

        #region Constructors
        /// <summary>
        /// Constructor: called by the sender that creates the database and sets up the max number of messages
        /// for the database. If the database doesn't exist it creates that and a table.
        /// </summary>
        /// <param name="maxMsgs">The cap for how many messages the queue can hold</param>
		  public SQLiteDatabase(int maxSize)
		  {
            Configure();
				if (File.Exists(mFilePath) == false)
				{
					 SQLiteConnection.CreateFile(mFilePath);
				}
				//make a database or open the existing one
				dbConnection = new SQLiteConnection("Data Source = " + mFilePath + ";Version=3;");
				mDBFileInfo = new FileInfo(mFilePath);

				//create table if not existing
            try
            {
                dbConnection.Open();
                //Create messages table if it not exists
                string sql = "CREATE TABLE IF NOT EXISTS messages (msgID INTEGER PRIMARY KEY, message VARCHAR(50), msgSize INT)";
                ExecuteSQL(sql);
                //Create lastID table if it not exists
                string lastIDTable = "CREATE TABLE IF NOT EXISTS lastID(ID INTEGER PRIMARY KEY, lastID INT)";
                ExecuteSQL(lastIDTable);
                //If lastID table does not have a row with ID = 1, insert the row
                string insert = "INSERT OR IGNORE INTO lastID (ID) VALUES (1)";
                ExecuteSQL(insert);
                //Assign the record to LastMsgID
                SQLiteCommand getLastIDCmd = new SQLiteCommand("SELECT lastID FROM lastID WHERE ID = 1", dbConnection);
                mLastMsgID = Convert.ToInt32(getLastIDCmd.ExecuteScalar());
                mDBSize = (int)mDBFileInfo.Length;
            }
            finally
            {
                dbConnection.Close();
            }
            mMaxSize = maxSize;
		  }

        /// <summary>
        /// Constructor: Called by the receiver, checks for an existing database and creates one if necessary.
        /// </summary>
        /// <param name="filePath"> The path to the desired output directory.</param>
        /// <param name="aDelay">The desired delay between message deliveries</param>

        public SQLiteDatabase(String deliveryPath)
        {
            Configure();
            if (File.Exists(mFilePath) == false)
            {
                SQLiteConnection.CreateFile(mFilePath);
            }
            //make a database or open the existing one
            dbConnection = new SQLiteConnection("Data Source = " + mFilePath + ";Version=3;");
            //create table if not existing
            try
            {
                dbConnection.Open();
                //Create messages table if it not exists
                string sql = "CREATE TABLE IF NOT EXISTS messages (msgID INTEGER PRIMARY KEY, message VARCHAR(50), msgSize INT)";
                ExecuteSQL(sql);
                //Create lastID talbe if it not exists
                string lastIDTable = "CREATE TABLE IF NOT EXISTS lastID(ID INTEGER PRIMARY KEY, lastID INT)";
                ExecuteSQL(lastIDTable);
                mDBFileInfo = new FileInfo(mFilePath);
                mDBSize = (int)mDBFileInfo.Length;
            }
            finally
            {
                dbConnection.Close();
            }
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
		  public void AddMessage(string msg, int size)
		  {
            //If message can fit in the allowed file size, add the record
				if (mDBSize + size < mMaxSize)
				{
                try
                {
					     string sql;
                    //if table is empty, insert the row with the id of last deleted record
					     if (CheckEmptyTable())
					     {
						      sql = "INSERT INTO messages (msgID, message, msgSize) VALUES ('" + (mLastMsgID + 1) + "','" + msg + "', '" + size + "')";
					     }
					     else
					     {
						      sql = "INSERT INTO messages (message, msgSize) VALUES ('" + msg + "', '" + size + "')";
					     }
					     dbConnection.Open();
                    ExecuteSQL(sql);
                }
                finally
                {
                    dbConnection.Close();
                }
                mDBFileInfo = new FileInfo(mFilePath);
					 mDBSize = (int)mDBFileInfo.Length;
					 Console.WriteLine("Message added to queue.");
				}
				else
				{
					 int numDeleted = 0;
                //If message can't fit in the allowed file size, 
                //delete first several records until the message can fit
					 while (mDBSize + size > mMaxSize)
					 {
						  DeleteOldestMessage();
                    mDBFileInfo = new FileInfo(mFilePath);
                    mDBSize = (int)mDBFileInfo.Length;
						  numDeleted++;
					 }
					 Console.WriteLine("Deleted " + numDeleted + " messages to make space for your message.");
					 AddMessage(msg, size);
				}
		  }

        /// <summary>
        /// Read the message from the frist row of database
        /// </summary>
        /// <returns>Returns content of the message</returns>
		  public string GetOldestMessage()
		  {
				if (CheckEmptyTable())
				{
					 Console.WriteLine("No Messages!");
				}
            string message = string.Empty;
            try
            {
                dbConnection.Open();                 
					 //get message to deliver
					 string sql = "SELECT message FROM messages WHERE msgID = (SELECT MIN(msgID) FROM messages)";
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                message = reader["message"].ToString();
            }
            finally
            {
                dbConnection.Close();
            }
            return message;
		  }

          /// <summary>
          /// Receive messages by certain size
          /// </summary>
          /// <param name="size">Size of messages in bytes</param>
          public ArrayList GetMsgBySize(int size)
          {
              ArrayList messages = new ArrayList();
              try{
                  int receivedSize = 0;
                  int counter = 0;
						int numRows = GetNumOfRows();
						dbConnection.Open();
                  while (receivedSize < size && counter < numRows)
                  {
                      //Get oldest messages id
                      SQLiteCommand minMsgIDCmd = new SQLiteCommand("SELECT MIN(msgID) FROM messages", dbConnection);
                      int minMsgID = Convert.ToInt32(minMsgIDCmd.ExecuteScalar());
                      //Locate current row by adding counter
                      int rowPointer = minMsgID + counter;
                      //Read message
                      SQLiteCommand getMsgCmd = new SQLiteCommand("SELECT message FROM messages WHERE msgID ='" + rowPointer + "'", dbConnection);
                      SQLiteDataReader getMsgReader = getMsgCmd.ExecuteReader();
                      messages.Add(getMsgReader["message"].ToString());
                      //Calculate received message size
                      SQLiteCommand getSizeCmd = new SQLiteCommand("SELECT msgSize FROM messages WHERE msgID ='" + rowPointer + "'", dbConnection);
                      receivedSize += Convert.ToInt32(getSizeCmd.ExecuteScalar());
                      counter++;
                  }
              }
              finally
              {
                  dbConnection.Close();
              }
              return messages;
          }

        /// <summary>
        /// Deletes the oldest message in the queue
        /// </summary>
		  public void DeleteOldestMessage()
		  {
              try
              {
                  dbConnection.Open();
						//check on msg ID
						SQLiteCommand cmd = new SQLiteCommand("SELECT msgID FROM messages WHERE msgID = (SELECT MIN(msgID) FROM messages)", dbConnection);
						int currMsgID = Convert.ToInt32(cmd.ExecuteScalar());
                  //Update last id to database talbe for further use
						mLastMsgID = currMsgID;
                  string lastIDUpdate = "UPDATE lastID SET lastID = '" + mLastMsgID + "' WHERE ID = 1";
                  ExecuteSQL(lastIDUpdate);
                  //Delete messages
                  string sql = "DELETE FROM messages WHERE msgID = (SELECT MIN(msgID) FROM messages);";
                  ExecuteSQL(sql);
                  //Vacuum the database to free up space
                  ExecuteSQL("VACUUM");
              }
              finally
              {
                  dbConnection.Close();
              }
              mDBSize = (int)mDBFileInfo.Length;
		  }

		  /// <summary>
		  /// Check if the table is empty
		  /// </summary>
		  /// <returns>True if empty</returns>
		  public bool CheckEmptyTable()
		  {
				bool empty = false;
				try
				{
					 dbConnection.Open();
					 SQLiteCommand getRowsCmd = new SQLiteCommand("SELECT COUNT(msgID) FROM messages", dbConnection);
					 if (Convert.ToInt32(getRowsCmd.ExecuteScalar()) == 0)
					 {
						  empty = true;
					 }
					 else
					 {
						  empty = false;
					 }
				}
				finally
				{
					 dbConnection.Close();
				}
				return empty;
		  }

        /// <summary>
        /// Reset the database, erase the database content
        /// </summary>
        public void Reset()
        {
            try
            {
                dbConnection.Open();
                ExecuteSQL("UPDATE lastID SET lastID = 0 WHERE ID = 1");
                ExecuteSQL("DELETE FROM messages");
            }
            finally
            {
                dbConnection.Close();
            }
        }

        /// <summary>
        /// Get the number of rows stored in the database
        /// </summary>
        /// <returns>Returns the number of rows in database</returns>
		  private int GetNumOfRows()
		  {
				int numOfRows = 0;
				try
				{
					 dbConnection.Open();
					 SQLiteCommand numOfRowsCmd = new SQLiteCommand("SELECT COUNT(msgID) FROM messages", dbConnection);
					 numOfRows = Convert.ToInt32(numOfRowsCmd.ExecuteScalar());
				}
				finally
				{
					 dbConnection.Close();
				}
				return numOfRows;
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
        /// Read database file path from configuration file
        /// </summary>
        private static void Configure()
        {
            var SQLiteDataBaseConfigu = ConfigurationManager.GetSection("SQLiteDataBaseConfigure") as NameValueCollection;
            mFilePath = SQLiteDataBaseConfigu["FilePath"].ToString();
        }
        #endregion
    }
}
        