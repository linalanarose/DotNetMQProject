using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
//Future: Make generic type

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

    //Receive all the messages and clear the database
    //para millionseconds wait between each message read
    public String[] receiveAllMsgs(int m_seconds)
    {
        int rowCount = this.numOfMsgs();
        int count = 0;
        String[] ret = new String[rowCount];
        SQLiteCommand command = new SQLiteCommand("SELECT * FROM messages", m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            ret[count] = "" + reader["message"];
            System.Threading.Thread.Sleep(m_seconds);
            count++;
        }
        String sql = "DELETE FROM messages";
        executeSQL(sql);
        return ret;
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

    private int numOfMsgs()
    {
        SQLiteCommand command = new SQLiteCommand("SELECT COUNT(msgID) from messages", database.m_dbConnection);
        int rowCount = Convert.ToInt32(command.ExecuteScalar());
        return rowCount;
    }
}
