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

				}
		  }
		  /// <summary>
		  /// Receives each message to a file denoted by the time it was received and clean the database
		  /// </summary>
		  private static void ReceiveAllMsgs()
		  {
				ArrayList messages = new ArrayList();
				while (database.CheckEmptyTable()==false)
				{
					 messages.Add(database.GetOldestMessage());
					 database.DeleteOldestMessage();
				}
				foreach (string msg in messages)
				{
					 SaveFile(msg);
				}		
		  }

        /// <summary>
        /// Save messages to the path which defined by delPath in configration
        /// </summary>
        /// <param name="msg">Message to save</param>
		  private static void SaveFile(string msg)
		  {
				//assumes the message has been deleted
				database.dbConnection.Open();
				SQLiteCommand cmd = new SQLiteCommand("SELECT msgID FROM message WHERE msgID = (SELECT MIN(msgID) FROM messages)");
				int minMsgID = Convert.ToInt32(cmd.ExecuteReader())-1;
				System.IO.File.WriteAllText(delPath + minMsgID.ToString() + ".xml", msg);
				Console.WriteLine("Saved file " + minMsgID + ".xml");
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
