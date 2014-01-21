﻿using System;
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
		  private int mDBSize;
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
				string sql = "CREATE TABLE IF NOT EXISTS messages (msgID INTEGER PRIMARY KEY, message VARCHAR(50))";
				ExecuteSQL(sql);
				mDBSize = (int)mDBFileInfo.Length;
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
            string sql = "CREATE TABLE IF NOT EXISTS messages (msgID INT PRIMARY KEY, message TEXT)";
            ExecuteSQL(sql);
				mDBFileInfo = new FileInfo(mFilePath);
				mDBSize = (int)mDBFileInfo.Length;
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
		  public void AddMessage(string msg, int size)
		  {
				if (mDBSize + size < mMaxSize)
				{
					 dbConnection.Open();
					 string sql = "INSERT INTO messages (message) VALUES ('" + msg + "')";
					 ExecuteSQL(sql);
					 dbConnection.Close();
                mDBFileInfo = new FileInfo(mFilePath);
					 mDBSize = (int)mDBFileInfo.Length;
				}
				else
				{
					 while (mDBSize + size > mMaxSize)
					 {
						 DeleteOldestMessage();
                         mDBFileInfo = new FileInfo(mFilePath);
                         mDBSize = (int)mDBFileInfo.Length;
					 }
					 AddMessage(msg, size);
				}
		  }
		  public string getOldestMessage()
		  {
				return "the message";
		  }
        
        /// <summary>
        /// Deletes the oldest message in the queue
        /// </summary>
		  private void DeleteOldestMessage()
		  {
				dbConnection.Open();
                string sql = "DELETE FROM messages WHERE msgID = (SELECT MIN(msgID) FROM messages);";
				ExecuteSQL(sql);
                string vacuum = "VACUUM";
                ExecuteSQL(vacuum);
				dbConnection.Close();
				mDBSize = (int)mDBFileInfo.Length;
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
        