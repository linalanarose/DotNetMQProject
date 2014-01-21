using Database;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.IO;

namespace Sender
{
    /// <summary>
    /// This class promotes a console for user to send messages
    /// </summary>
    class Sender
    {
        static void Main(string[] args)
        {
            Console.Write("Please enter message to send or type 'exit' to quit\n");
            //creates a new sqlitedatabase
            SQLiteDatabase database = new SQLiteDatabase(100000);
            //while the user is in the system entering messages
            String[] directoryFiles = Directory.GetFiles("C:/SQLiteDataBase","*.xml");
            DirectoryInfo directoryFileInfo = new DirectoryInfo("C:/SQLiteDataBase");
            FileInfo[] fileNames = directoryFileInfo.GetFiles("*.xml");
            foreach(FileInfo names in fileNames)
            {
                database.CreateMessage("C:/SQLiteDataBase/" + names.Name);
            }
            foreach (FileInfo names in fileNames)
            {
                System.IO.Directory.Move("C:/SQLiteDataBase/" + names.Name, "C:/SQLiteDataBaseArchive/" + names.Name);
            }
            
        }
    }
}
