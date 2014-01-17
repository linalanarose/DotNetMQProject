using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Receiver
{
    public class Receiver
    {
        SQLiteDatabase database;
        public Receiver()
        {
            database = new SQLiteDatabase();
        }

        public String[] receiveAllMsgs()
        {
            int rowCount = this.numOfMsgs();
            int count = 0;
            String[] ret = new String[rowCount];
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM messages", database.m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()){
                ret[count] = "" + reader["message"];
                count++;
            }
            return ret;
        }

        private int numOfMsgs(){
            SQLiteCommand command = new SQLiteCommand("SELECT COUNT(msgID) from messages", database.m_dbConnection);
            int rowCount = Convert.ToInt32(command.ExecuteScalar());
            return rowCount;
        }
    }
}
