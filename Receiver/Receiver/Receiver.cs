﻿using Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Receiver
{
    class Receiver
    {
		  string mDeliveryPath;
		  int mDelay;

		  string delPath = string.Empty;
		  var ReceiverDatabaseCommunication = ConfigurationManager.GetSection("ReceiverDatabaseCommunication") as NameValueCollection;
		  delPath = ReceiverDatabaseCommunication["deliveryPath"].toString();
		  SQLiteDatabase database = new SQLiteDatabase(delPath, 1000);
        static void Main(string[] args)
        {
           Console.Write("Start receving messages\n");
			  ReceiveAllMsgs();
           Console.WriteLine("Messages saved to your directory! Hit any key to exit");
           Console.ReadKey();
        }
		  /// <summary>
		  /// Receives each message to a file denoted by the time it was received
		  /// </summary>
		  private void ReceiveAllMsgs()
		  {
				database.dbConnection.Open();
				SQLiteCommand command = new SQLiteCommand("SELECT * FROM messages", database.dbConnection);
				SQLiteDataReader reader = command.ExecuteReader();
				while (reader.Read())
				{
					 Console.WriteLine("Message: " + reader["message"]);
					 System.IO.File.WriteAllText(mDeliveryPath + reader["time"].ToString() + ".xml", reader["message"].ToString());
					 System.Threading.Thread.Sleep(mDelay);
				}
				database.dbConnection.Close();
				System.IO.File.Delete(mFilePath);
		  }
    }
}
