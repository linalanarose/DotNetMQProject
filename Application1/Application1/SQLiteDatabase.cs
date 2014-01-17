using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

public class SQLiteDatabase
{
    //set this to control maximum size of queue
    private static int max_mCount;
    private static int delay;
    private static int mCount;
    public SQLiteConnection m_dbConnection;
    //constructor
    //creates database, sets message count to 0, creates the table
    public SQLiteDatabase(int max_Count, int delay_reads)
    {
        SQLiteConnection.CreateFile("MessageDatabase.sqlite");
        //make a database
        m_dbConnection = new SQLiteConnection("Data Source=MessageDatabase.sqlite;Version=3;New=True");
        //set counts
        mCount = 0;
        max_mCount = max_Count;
        delay = delay_reads;
        //create table
        m_dbConnection.Open();
        string sql = "CREATE TABLE messages (msgID INT, message VARCHAR(50))";
        executeSQL(sql);
        m_dbConnection.Close();
    }

    //makes a message and inserts it at the next point in the table
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
            //call delete oldest message and try to create again
            deleteOldestMessage();
            createMessage(message);
        }
    }
    //returns the oldest message and deletes it ("delivers")
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

    //deletes the oldest message in the queue
    public void deleteOldestMessage()
    {
        m_dbConnection.Open();
        string sql = "DELETE FROM messages WHERE msgID = 0";
        executeSQL(sql);
        m_dbConnection.Close();
        decrementmsgID();
        mCount--;
    }

    //selects all messages in queue
    public void listMessage()
    {
        m_dbConnection.Open();
        SQLiteCommand command = new SQLiteCommand("SELECT * FROM messages", m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
            Console.WriteLine("msgID: " + reader["msgID"] + "\tMessage: " + reader["message"]);
        m_dbConnection.Close();
    }

    //receive all the messages and clear the database
    //Para: miliseconds between each read
    public String[] receiveAllMsgs(int m_seconds)
    {
        int rowCount = this.numOfMsgs();
        int count = 0;
        String[] ret = new String[rowCount];
        SQLiteCommand command = new SQLiteCommand("SELECT * FROM messages", m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            ret[count] =  reader["message"].ToString();
            count++;
            System.Threading.Thread.Sleep(m_seconds);
        }
        String sql = "DELETE FROM messages";
        executeSQL(sql);
        return ret;
    }

    //moves all messages up in the queue (decrementing their msgID
    private void decrementmsgID()
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
        SQLiteCommand command = new SQLiteCommand("SELECT COUNT(msgID) from messages", m_dbConnection);
        int rowCount = Convert.ToInt32(command.ExecuteScalar());
        return rowCount;
    }
}
