using System;
using System.Data.SQLite;

namespace Midis.EyeOfHorus.ClientLibrary.Database
{
    public class OperationsWithDatabase
    {
        public static void DatabaseVersionCheck()
        {
            //string cs = "Data Source=:memory:";
            string cs = @"URI=file:C:\Projects\Git\DmitrijsBruskovskis\Midis.EyeOfHorus\Midis.EyeOfHorus.ClientLibrary\Database\DataBase.db";
            string stm = "SELECT SQLITE_VERSION()";

            using var con = new SQLiteConnection(cs);
            con.Open();

            using var cmd = new SQLiteCommand(stm, con);
            string version = cmd.ExecuteScalar().ToString();

            Console.WriteLine($"SQLite version: {version}");
        }

        public static void DataInsertIntoDatabase()
        {
            string cs = @"URI=file:C:\Projects\Git\DmitrijsBruskovskis\Midis.EyeOfHorus\Midis.EyeOfHorus.ClientLibrary\Database\DataBase.db";

            using var con = new SQLiteConnection(cs);
            con.Open();

            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = "INSERT INTO Cameras(Name, OutputFolder) VALUES('FBD-2357','C:/123/')";
            cmd.ExecuteNonQuery();
        }
        public static void GetDataFromDatabase()
        {
            string cs = @"URI=file:C:\Projects\Git\DmitrijsBruskovskis\Midis.EyeOfHorus\Midis.EyeOfHorus.ClientLibrary\Database\DataBase.db";

            using var con = new SQLiteConnection(cs);
            con.Open();

            string stm = "SELECT * FROM Cameras";

            using var cmd = new SQLiteCommand(stm, con);
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                int result1 = rdr.GetInt32(0);
                string result2 = rdr.GetString(1);
                string result3 = rdr.GetString(2);
            }
        }
        //Function example
        public static void DatabaseCreation()
        {
            string cs = @"URI=file:C:\Projects\Git\DmitrijsBruskovskis\Midis.EyeOfHorus\Midis.EyeOfHorus.ClientLibrary\Database\DataBase.db";

            using var con = new SQLiteConnection(cs);
            con.Open();

            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = "DROP TABLE IF EXISTS cars";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"CREATE TABLE cars(id INTEGER PRIMARY KEY,
                    name TEXT, price INT)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Audi',52642)";
            cmd.ExecuteNonQuery();

            Console.WriteLine("Table cars created");
        }

    }
}
