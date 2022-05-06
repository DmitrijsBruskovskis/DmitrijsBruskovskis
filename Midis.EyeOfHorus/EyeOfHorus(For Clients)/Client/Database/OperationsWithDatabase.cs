using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Midis.EyeOfHorus.ClientLibrary.Database
{
    public class OperationsWithDatabase
    {
        public SQLiteConnection con;
        SQLiteDataAdapter da;
        DataSet ds;
        SQLiteCommand cmd;

        public DataSet GetDataSet()
        {
            //Debug Path
            //string databasePath = Path.GetFullPath("../../../../Midis.EyeOfHorus.ClientLibrary/Database/DataBase.db");

            //Release Path
            string databasePath = Path.GetFullPath("./Database/DataBase.db");

            string cs = @"URI=file:" + databasePath;
            con = new SQLiteConnection(cs);
            da = new SQLiteDataAdapter("Select * From Cameras", con);
            ds = new DataSet();
            con.Open();
            da.Fill(ds, "Cameras");
            con.Close();
            return ds;
        }

        public void ExecuteCommand(string commandText)
        {
            try
            {
                cmd = new SQLiteCommand();
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = commandText;
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch
            {

            }       
        }
    }
}
