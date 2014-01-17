using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

public class SQLiteDatabase
{
    //set this to control size of queue
    private static int max_mCount = 3;

    private static int mCount;
    public SQLiteConnection m_dbConnection;
    public SQLiteDatabase()
    {
        SQLiteConnection.CreateFile("MessageDatabase.sqlite");
        //make a database
        m_dbConnection = new SQLiteConnection("Data Source=MessageDatabase.sqlite;Version=3;New=True");
        //set count
        mCount = 0;
        //create table
        m_dbConnection.Open();
        string sql = "CREATE TABLE messages (msgID INT, message VARCHAR(50))";
        executeSQL(sql);
        m_dbConnection.Close();
    }

    public void createMessage(String message)
    {
        //if the queue isn't "full"
        if (mCount < max_mCount)
        {
            m_dbConnection.Open();
            string sql = "INSERT INTO messages (msgID, message) VALUES (" + mCount + ", '" + message + "')";
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
        string sql = "SELECT message FROM messages WHERE msgID = 0";
        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        String msg = (String)reader["message"];
        m_dbConnection.Close();
        deleteOldestMessage();
        return msg;
    }

    public void deleteOldestMessage()
    {
        m_dbConnection.Open();
        string sql = "DELETE FROM messages WHERE msgID = 0";
        executeSQL(sql);
        m_dbConnection.Close();
        decrementOrder();
        mCount--;
    }

    public void listMessage()
    {
        m_dbConnection.Open();
        executeSQL("SELECT * FROM messages");
        m_dbConnection.Close();
    }

    private void decrementOrder()
    {
        m_dbConnection.Open();
        executeSQL("UPDATE messages SET msgID = msgID - 1");
        m_dbConnection.Close();
    }

    private void executeSQL(String sql)
    {
        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        command.ExecuteNonQuery();
    }

}
