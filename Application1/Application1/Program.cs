﻿using System;
using System.Text;
//using MDS.Client;
using System.Data.SQLite;

namespace Application1
{
    static class Program
    {
        //* means it could be turned into a helper function to simplify code

        //add an array to hold messages (in chronological order)
        static void Main()
        {
            //create MDSClient object and connect it

            //Tell user to make message for queue (get message)*
            Console.WriteLine("Write something to send to the server (or queue) and hit enter");
            //handle empty(null) messages and other possible problems

            //get message text (currently from console)
            var text = Console.ReadLine();

            //create message
            //set destination application
            //set message data

            //Loop for adding messages to cache*
            //while (boolean representing server disconnectivity: true if disconnected)
            while (true)
            {
                //check size of message
                //check space in cache (loop until there's space: !full )
                    //if not enough space delete oldest message
                //push to cache
            }

            //loop for sending messages to server*
            //same boolean as before (this loop runs when connected)
            while (false)
            {
                //iterate through messages array
                //send message
                //delay - configurable
            }
        }
    }
}