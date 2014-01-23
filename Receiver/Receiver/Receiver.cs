﻿﻿using Database;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SQLite;
using System.Text;
using System.Threading;
using System.IO;

namespace Receiver
{
	 /// <summary>
	 /// The class that receives messages and allows for choice on how to do so
	 /// </summary>
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
        /// </summary>
        /// <param name="maxSize">Size of messages to receive</param>
		  /// <param name="delete">Delete messages on receipt or not</param>
		  private static void ReceiveBySize(int maxSize, bool delete)
		  { 
				ArrayList msgs = database.GetMsgBySize(maxSize);
				int numMsgs = msgs.Count;
				for (int i = 0; i < numMsgs; i++ )
				{
					 if (SaveFile(msgs[i].ToString()))
					 {
						  if (delete)
						  //logic here fails if the database deletes other messages during the saving of messages.
						  //needs redone eventually
						  {
								database.DeleteOldestMessage();
								Console.WriteLine("Deleted successfuly saved message from queue.");
						  }
					 }
					 else //retry this message if fails to save
					 {
						  i--;
						  Console.WriteLine("Message save failed. Trying again.");
					 }
					 Thread.Sleep(mDelay);
				}
		  }
        /// <summary>
        /// Receives each message to a file denoted by the msg ID (delete as you go if save successful)
        /// </summary>
        private static void ReceiveAllMsgs()
        {
            while (database.CheckEmptyTable() == false)
            {
					 ReceiveAMessage();
					 Thread.Sleep(mDelay);
            }
            Console.WriteLine("No More Messages");
        }
		  /// <summary>
		  /// Gets and deletes 1 message from the Database
		  /// </summary>
		  private static void ReceiveAMessage()
		  {
				//if the file save is successful, delete the message
            try
            {
                if (SaveFile(database.GetOldestMessage()))
                {
                    database.DeleteOldestMessage();
                }
            }
            catch (DataBaseEmptyException e)
            {
                Console.WriteLine(e.Message);
            }
		  }

        /// <summary>
        /// Save messages to the path which defined by delPath in configration
        /// </summary>
        /// <param name="msg">Message to save</param>
		  /// <returns>True if successful</returns>
        private static bool SaveFile(string msg)
        {
				try
				{
					 System.IO.File.WriteAllText(delPath + fileNameCount.ToString() + ".xml", msg);
					 Console.WriteLine("Saved file " + fileNameCount + ".xml");
					 fileNameCount++;
					 return true;
				}
				catch
				{
					 Console.WriteLine("File save for " + fileNameCount + ".xml failed. Trying again.");
					 return false;
				}

        }

        private static byte[] Zip(string text)
        {
            byte[] buffer = System.Text.Encoding.Unicode.GetBytes(text);
            MemoryStream ms = new MemoryStream();
            using (System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;
            MemoryStream outStream = new MemoryStream();

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return gzBuffer;
        }

        private static string UnZip(byte[] compressedText)
        {
            byte[] gzBuffer = compressedText;
            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;
                using (System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return System.Text.Encoding.Unicode.GetString(buffer, 0, buffer.Length);
            }
        }
        /// <summary>
        /// Configure parameters of receiver and create instance of database
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
