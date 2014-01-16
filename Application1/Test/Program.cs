using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Please enter message to send\n");
            while (true)
            {
                var messageText = Console.ReadLine();
                if (string.IsNullOrEmpty(messageText) || messageText == "exit")
                {
                    break;
                }
                SQLiteDatabase database = new SQLiteDatabase();
                database.createMessage(messageText);

                SQLiteCommand command = new SQLiteCommand("SELECT * FROM messages", database.m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    Console.WriteLine("Order: " + reader["order"] + "\tMessage: " + reader["message"]);
            }
        }
    }
}
