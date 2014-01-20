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
<<<<<<< HEAD
           SQLiteDatabase database = new SQLiteDatabase("C:/SQLiteDataBase/receivedMsg.txt", 1000);
           String[] result = database.ReceiveAllMsgs();
           //database.ReceiveAllMsgs();
           Console.WriteLine("Messages saved to receivedMsg.txt! Hit any key to exit");
=======
           SQLiteDatabase database = new SQLiteDatabase("C:/SQLiteDataBase/", 1000);
           database.ReceiveAllMsgs();
           Console.WriteLine("Messages saved to your directory! Hit any key to exit");
>>>>>>> 9bd524d0965d57f025b9db67ac6dc74a3d2e21c3
           Console.ReadKey();
        }
    }
}
