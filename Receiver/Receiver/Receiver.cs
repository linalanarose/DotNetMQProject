﻿using Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Receiver
{
    class Receiver
    {
		  static SQLiteDatabase database;
		  static int mDelay;
		  static int delSize;
	 	  static string delPath = string.Empty;
		  static int fileNameCount;

        static void Main(string[] args)	  
        {
				Configure();
				Console.Write("Start receving messages\n");
				ReceiveAllMsgs();
                //ReceiveBySize(delSize);
				Console.WriteLine("Messages saved to your directory! Hit any key to exit");
				Console.ReadKey();
        }

        /// <summary>
        /// Receive certain size of messages from beginning of database
        /// </summary>
        /// <param name="maxSize">Size of messages to receive</param>
		  private static void ReceiveBySize(int maxSize)
		  {
				ArrayList msgs = database.GetMsgBySize(maxSize);
				foreach (string msg in msgs)
				{
					 //add REAL saving function
					 SaveFile(msg);
					 int fileName = fileNameCount -1;
					 Console.WriteLine("Saved file " + fileName + ".xml");
				}
		  }
		  /// <summary>
		  /// Receives each message to a file denoted by the time it was received and clean the database
		  /// </summary>
		  private static void ReceiveAllMsgs()
		  {
				try
				{
					 database.dbConnection.Open();
                     //Read everything from database
					 SQLiteCommand command = new SQLiteCommand("SELECT * FROM messages", database.dbConnection);
					 SQLiteDataReader reader = command.ExecuteReader();
                     //Handle messages, write message content into console, and write content into file on disk
					 while (reader.Read())
					 {
						  Console.WriteLine("Message: " + reader["message"]);
                          string writePath = delPath + reader["msgID"].ToString() + ".xml";
                          System.IO.File.WriteAllText(writePath, reader["message"].ToString());
                          //Delay between each message
						  System.Threading.Thread.Sleep(mDelay);
					 }

                     //Clear the database
                     SQLiteCommand deleteCmd = new SQLiteCommand("DELETE FROM messages", database.dbConnection);
                     deleteCmd.ExecuteNonQuery();
                     //Vacuum the sqlite database to free up space
                     SQLiteCommand vacuCmd = new SQLiteCommand("VACUUM", database.dbConnection);
                     vacuCmd.ExecuteNonQuery();
				}
				finally 
				{
					 database.dbConnection.Close(); 
				}				
		  }

        /// <summary>
        /// Save messages to the path which defined by delPath in configration
        /// </summary>
        /// <param name="msg">Message to save</param>
		  private static void SaveFile(string msg)
		  {
				System.IO.File.WriteAllText(delPath + fileNameCount.ToString() + ".xml", msg);
				fileNameCount++;
		  }

        /// <summary>
        /// Configurate parameters of receiver and create instance of database
        /// </summary>
		  private static void Configure()
		  {				
				var ReceiverDatabaseCommunication = ConfigurationManager.GetSection("ReceiverDatabaseCommunication") as NameValueCollection;
				delPath = ReceiverDatabaseCommunication["deliveryPath"].ToString();
				mDelay = Int32.Parse(ReceiverDatabaseCommunication["delay"]);
				delSize = Int32.Parse(ReceiverDatabaseCommunication["delSize"]);
                database = new SQLiteDatabase(delPath);
		  }
    }
}
