using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

public class SQLiteDatabase
{
    SQLiteConnection m_dbConnection;

	public SQLiteDatabase()
	{
        SQLiteConnection.CreateFile("MyDatabase.sqlite");
	}

    public void connectToDataBase(){
        m_dbConnection = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");
        m_dbConnection.Open();
    }

    public void executeSQL(String sql)
    {
        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        command.ExecuteNonQuery();
    }

    public void deleteOldestMessage()
    {
        string sql = "Delete FROM message ORDER BY order ASC LIMIT 1";
        executeSQL(sql);
        mCount--;
    }



}
