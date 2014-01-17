using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Database;

namespace Receiver
{
    /// <summary>
    /// This class receives messages from the database
    /// </summary>
    public class Receiver
    {
        static void Main(string[] args)
        {
            SQLiteDatabase database = new SQLiteDatabase("r", 1000);
            Console.Write("Messages Received\n");
            database.receiveAllMsgs();
        }
    }
}
