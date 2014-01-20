using Database;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Sender
{
    /// <summary>
    /// This class promotes a console for user to send messages
    /// </summary>
    class Sender
    {
        static void Main(string[] args)
        {
            Console.Write("Please enter the path to an XML file to send or type 'exit' to quit\n");
            //creates a new sqlitedatabase
            SQLiteDatabase database = new SQLiteDatabase(10000);
            //while the user is in the system entering messages
            while (true)
            {
                //make sure the message isn't null or exit
					 var msgPath = Console.ReadLine();

                if (string.IsNullOrEmpty(msgPath) || msgPath == "exit")
                {
                    break;
                }

                //create message from console input and add to table
					 database.CreateMessage(msgPath);                
            }
        }
    }
}
