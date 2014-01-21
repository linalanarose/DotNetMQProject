using Database;
using System;
using System.Collections.Generic;
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
		  SQLiteDatabase database = new SQLiteDatabase(10000);
        static void Main(string[] args)
        {
            Console.Write("Please enter the path to an XML file to send or type 'exit' to quit\n");
            //creates a new sqlitedatabase
            //while the user is in the system entering messages
            while (true)
            {
                //make sure the message isn't null or exit
					 var msgPath = Console.ReadLine();

                if (string.IsNullOrEmpty(msgPath) || msgPath == "exit")
                {
                    break;
                }
					 //check that the path works before calling SendMessage

                //create message from msg path and add to table
					 SendMessage(msgPath);             
            }
        }
		  private void SendMessage(string msgPath)
		  {
				StreamReader sr = new StreamReader(msgPath);
				string msg = sr.ReadToEnd();
				msg = msg.Replace("'", "''");				

				FileInfo msgFileInfo = new FileInfo(msgPath);
				int size = (int)msgFileInfo.Length;

				database.AddMessage(msg, size);
		  }
    }
}
