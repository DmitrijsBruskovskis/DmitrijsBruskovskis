using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Face_recognition
{
    class Database
    {
        static void Main()
        {
            string cs = @"URI=file:C:\Users\Jano\Documents\test.db";

            var con = new SQLiteConnection(cs);
            con.Open();

            using var cmd = new SqliteCommand(con);

            cmd.CommandText = "DROP TABLE IF EXISTS cars";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"CREATE TABLE cars(id INTEGER PRIMARY KEY,
                    name TEXT, price INT)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Audi',52642)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Mercedes',57127)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Skoda',9000)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Volvo',29000)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Bentley',350000)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Citroen',21000)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Hummer',41400)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Volkswagen',21600)";
            cmd.ExecuteNonQuery();

            Console.WriteLine("Table cars created");
        }
    }

    internal class SQLiteConnection
    {
        private string cs;

        public SQLiteConnection(string cs)
        {
            this.cs = cs;
        }
    }
}
