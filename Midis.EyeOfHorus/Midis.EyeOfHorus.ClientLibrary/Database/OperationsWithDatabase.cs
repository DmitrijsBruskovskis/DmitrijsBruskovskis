using System;
using System.Data.SQLite;

namespace Midis.EyeOfHorus.ClientLibrary.Database
{
    public class OperationsWithDatabase
    {
        public static void DatabaseVersionCheck()
        {
            string cs = @"URI=file:C:\Projects\Git\DmitrijsBruskovskis\Midis.EyeOfHorus\Midis.EyeOfHorus.ClientLibrary\Database\DataBase.db";
            string stm = "SELECT SQLITE_VERSION()";

            using var con = new SQLiteConnection(cs);
            con.Open();

            using var cmd = new SQLiteCommand(stm, con);
            string version = cmd.ExecuteScalar().ToString();

            Console.WriteLine($"SQLite version: {version}");
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
