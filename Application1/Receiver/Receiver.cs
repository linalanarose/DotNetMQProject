﻿using System;
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
    }
}
