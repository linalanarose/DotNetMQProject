using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

public class SQLiteDatabase
{
    //set this to control size of queue
    private static int max_mCount = 10;

    private static int mCount;
    SQLiteConnection m_dbConnection;
    public SQLiteDatabase()
    {
        SQLiteConnection.CreateFile("MessageDatabase.sqlite");
        //make a database
        m_dbConnection = new SQLiteConnection("Data Source=MessageDatabase.sqlite;Version=3;");
        //set count
        mCount = 0;
        //create table
        m_dbConnection.Open();
        string sql = "create table messages (order int, message varchar())";
        executeSQL(sql);
        m_dbConnection.Close();
    }
    //makes a message and adds it to the sql table
    public void createMessage(String message)
    {
        //if the queue isn't "full"
        if (mCount < max_mCount)
        {
            m_dbConnection.Open();
            string sql = "insert into messages (order, message) values (" + mCount + ", '" + message + "')";
            executeSQL(sql);
            mCount++;
            m_dbConnection.Close();
        }
        else
        {
            //call delete function
            deleteOldestMessage();
            createMessage(message);
        }
    }

    //returns the oldest message and deletes it
    public String pullOldestMessage()
    {
        m_dbConnection.Open();
        string sql = "SELECT message FROM messages ORDER BY order ASC LIMIT 1";
        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        String msg = (String) reader["message"];
        m_dbConnection.Close();
        deleteOldestMessage();
        return msg;
    }
    //deletes oldest message
    public void deleteOldestMessage()
    {
        m_dbConnection.Open();
        string sql = "Delete FROM messages ORDER BY order ASC LIMIT 1";
        executeSQL(sql);
        m_dbConnection.Close();
        decrementOrder();
        mCount--;
    }

    //lists all messages
    public void listMessage()
    {
        m_dbConnection.Open();
        executeSQL("SELECT * FROM message");
        m_dbConnection.Close();
    }

    //moves all messages up in queue
    private void decrementOrder()
    {
        m_dbConnection.Open();
        executeSQL("UPDATE messages");
        executeSQL("SET order = order - 1");
        m_dbConnection.Close();
    }
    //simplifies the sql command process
    private void executeSQL(String sql)
    {
        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        command.ExecuteNonQuery();
    }

}
