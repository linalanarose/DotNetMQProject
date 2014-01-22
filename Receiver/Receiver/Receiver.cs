﻿﻿using Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;

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
            Console.Write("Started receving messages\n");
            while (true)
            {
                ReceiveAllMsgs();
					 //ReceiveBySize(delSize, true);
                Thread.Sleep(10000);
            }         
            Console.ReadKey();
        }

        /// <summary>
        /// Receive certain size of messages from beginning of database
        /// Does NOT delete
        /// </summary>
        /// <param name="maxSize">Size of messages to receive</param>

		  /// <param name="delete">Delete messages on receipt or not</param>
		  private static void ReceiveBySize(int maxSize, bool delete)
		  { 
				ArrayList msgs = database.GetMsgBySize(maxSize);
				int numMsgs = msgs.Count;
				foreach (string msg in msgs)
				{
					 SaveFile(msg);
					 Thread.Sleep(mDelay);
				}
				//logic here fails if the database changes during the saving of messages.
				//needs redone eventually
				if (delete)
				{
					 for (int i=0; i<numMsgs; i++)
					 {
						  database.DeleteOldestMessage();
					 }
				}
		  }
        /// <summary>
        /// Receives each message to a file denoted by the time it was received (delete as you go)
        /// </summary>
        /// <param name="delete">True if delete on Receipt of messages. Not currently implemented</param>
        private static void ReceiveAllMsgs()
        {
            while (database.CheckEmptyTable() == false)
            {
                SaveFile(database.GetOldestMessage());
                database.DeleteOldestMessage();
                Thread.Sleep(mDelay);
            }
            Console.WriteLine("Messages saved to your directory!");
        }

        /// <summary>
        /// Save messages to the path which defined by delPath in configration
        /// </summary>
        /// <param name="msg">Message to save</param>
        private static void SaveFile(string msg)
        {
            System.IO.File.WriteAllText(delPath + fileNameCount.ToString() + ".xml", msg);
            Console.WriteLine("Saved file " + fileNameCount + ".xml");
            fileNameCount++;
        }

        /// <summary>
        /// Configurate parameters of receiver and create instance of database
        /// </summary>
        private static void Configure()
        {
            //get info from config file
            var ReceiverDatabaseCommunication = ConfigurationManager.GetSection("ReceiverDatabaseCommunication") as NameValueCollection;
            delPath = ReceiverDatabaseCommunication["deliveryPath"].ToString();
            mDelay = Int32.Parse(ReceiverDatabaseCommunication["delay"]);
            delSize = Int32.Parse(ReceiverDatabaseCommunication["delSize"]);
            database = new SQLiteDatabase(delPath);
            //set current fileNameCount
            try
            {
                database.dbConnection.Open();
                SQLiteCommand cmd = new SQLiteCommand("SELECT msgID FROM messages WHERE msgID = (SELECT MIN(msgID) FROM messages)", database.dbConnection);
                int minMsgID = Convert.ToInt32(cmd.ExecuteScalar());
                fileNameCount = minMsgID;
            }
            finally
            {
                database.dbConnection.Close();
            }
        }
    }
}
