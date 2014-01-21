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
	 	  static string delPath = string.Empty;

        static void Main(string[] args)	  
        {
				Configure();
				Console.Write("Start receving messages\n");
				//ReceiveAllMsgs();
                ReceiveBySize(10000);
				Console.WriteLine("Messages saved to your directory! Hit any key to exit");
				Console.ReadKey();
        }
		  private static void ReceiveBySize(int maxSize)
		  {
				ArrayList msgs = database.GetMsgBySize(maxSize);
				foreach (string msg in msgs)
				{
					 Console.WriteLine(msg);
                     Console.WriteLine();
				}
		  }
		  /// <summary>
		  /// Receives each message to a file denoted by the time it was received
		  /// </summary>
		  private static void ReceiveAllMsgs()
		  {
				try
				{
					 database.dbConnection.Open();
					 SQLiteCommand command = new SQLiteCommand("SELECT * FROM messages", database.dbConnection);
					 SQLiteDataReader reader = command.ExecuteReader();
					 while (reader.Read())
					 {
						  Console.WriteLine("Message: " + reader["message"]);
                          System.IO.File.WriteAllText(delPath + reader["time"].ToString() + ".xml", reader["message"].ToString());
						  System.Threading.Thread.Sleep(mDelay);
					 }
				}
				finally 
				{
					 database.dbConnection.Close(); 
				}				
		  }
		  private static void Configure()
		  {				
				var ReceiverDatabaseCommunication = ConfigurationManager.GetSection("ReceiverDatabaseCommunication") as NameValueCollection;
				delPath = ReceiverDatabaseCommunication["deliveryPath"].ToString();
				mDelay = Int32.Parse(ReceiverDatabaseCommunication["delay"]);
                database = new SQLiteDatabase(delPath);
		  }
    }
}
