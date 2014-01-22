﻿using Database;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace Sender
{
    /// <summary>
    /// This class promotes a console for user to send messages
    /// </summary>
    class Sender
    {
		  static int mDBSize;
		  static SQLiteDatabase database;
          static void Main(string[] args)
          {
            Configure();
            Console.Write("Please enter the path to an XML file to send or type 'exit' to quit\n");

            //while the user is in the system entering messages
            while (true)
            {
                //make sure the message isn't null or exit
                var msgPath = Console.ReadLine();
                if (string.IsNullOrEmpty(msgPath) || msgPath == "exit")
                {
                    break;
                }
                //create message from msg path and add to table
					 SendMessage(msgPath);
            }
              //Following lines are for performance testing
              /*
            static void Main(string[] args)
            {
              Configure();
              Console.Write("Please enter the path to an XML file to send or type 'exit' to quit\n");
              var minutes = 1;
              var start = DateTime.UtcNow;
              var endTime = start.AddMinutes(minutes);
              //while the user is in the system entering messages
              while (true)
              {
                  TimeSpan remainingTime = endTime - DateTime.UtcNow;
                  //make sure the message isn't null or exit
                  var msgPath = Console.ReadLine();
                  if (string.IsNullOrEmpty(msgPath) || msgPath == "exit")
                  {
                      break;
                  }
                  //create message from msg path and add to table]
                  if (remainingTime > TimeSpan.Zero)
                      SendMessage("C:/SQLiteDataBase/1.xml");
                  else
                      break;
              }
          }
          */
        }

        /// <summary>
        /// Send messages from the specified path
        /// </summary>
        /// <param name="msgPath">Path of XML file to send</param>
		  private static void SendMessage(string msgPath)
		  {
				StreamReader sr = new StreamReader(msgPath);
				string msg = sr.ReadToEnd();
                //Sqlite does not recongize apostrophe
				msg = msg.Replace("'", "''");
				FileInfo msgFileInfo = new FileInfo(msgPath);
				int size = (int)msgFileInfo.Length;
				database.AddMessage(msg, size);
		  }

        /// <summary>
        /// Configure the sender and create instance of database.
        /// </summary>
		  private static void Configure()
		  {
				var SenderDatabaseCommunication = ConfigurationManager.GetSection("SenderDatabaseCommunication") as NameValueCollection;
				mDBSize = Int32.Parse(SenderDatabaseCommunication["maxsize"]);
				database = new SQLiteDatabase(mDBSize);
		  }
    }
}
