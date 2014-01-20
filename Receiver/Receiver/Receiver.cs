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
           SQLiteDatabase database = new SQLiteDatabase("C:/SQLiteDataBase/receivedMsg.txt", 1000);
           String[] result = database.ReceiveAllMsgs();
        //database.ReceiveAllMsgs();
           Console.WriteLine("Messages saved to receivedMsg.txt! Hit any key to exit");
           Console.ReadKey();
        }
    }
}
