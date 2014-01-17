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
            Console.Write("Please enter message to send\n");
            SQLiteDatabase database = new SQLiteDatabase();
            while (true)
            {
                var messageText = Console.ReadLine();
                if (string.IsNullOrEmpty(messageText) || messageText == "exit")
                {
                    break;
                }

                database.createMessage(messageText);

                database.m_dbConnection.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM messages", database.m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    Console.WriteLine("msgID: " + reader["msgID"] + "\tMessage: " + reader["message"]);
                database.m_dbConnection.Close();
            }
        }
    }
}
