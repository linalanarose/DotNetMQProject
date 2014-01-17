using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Data.SQLite;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Please enter message to send or type 'exit' to quit\n");
            //creates a new sqlitedatabase
            SQLiteDatabase database = new SQLiteDatabase();
            //while the user is in the system entering messages
            while (true)
            {
                //make sure the message isn't null or exit
                var messageText = Console.ReadLine();
                if (string.IsNullOrEmpty(messageText) || messageText == "exit")
                {
                    break;
                }
                //create message from console input and add to table
                database.createMessage(messageText);
                //TEMPORARY open the database and list all current messages
                database.listMessage();
            }
        }
    }
}
