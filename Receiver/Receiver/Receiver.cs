using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Receiver
{
    class Receiver
    {
        static void Main(string[] args)
        {
           Console.Write("Start receving messages\n");
            //opens SQLiteDatabase file and retrieves messages
           SQLiteDatabase database = new SQLiteDatabase("C:/SQLiteDataBase/", 1000);
           database.ReceiveAllMsgs();
           Console.WriteLine("Messages saved to your directory! Hit any key to exit");
           Console.ReadKey();
        }
    }
}
