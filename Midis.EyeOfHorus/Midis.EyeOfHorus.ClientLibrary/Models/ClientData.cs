using System.Collections.Generic;
using System.Data.SQLite;
using System.Xml.Serialization;

namespace Midis.EyeOfHorus.ClientLibrary
{
    [XmlRootAttribute("ClientData", Namespace = "http://www.cpandl.com", IsNullable = false)]
    public class ClientData
    {
        public int FrameCount;
        public string ClientKey;
    }

    public class User
    {
        private IEnumerable<KeyValuePair<string, object>> args;

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string Lastname { get; set; }

        public int ExecuteWrite(string query, Dictionary<string, object> args)
        {
            int numberOfRowsAffected;

            //setup the connection to the database
            using (var con = new SQLiteConnection("Data Source=test.s3db"))
            {
                con.Open();

                //open a new command
                using (var cmd = new SQLiteCommand(query, con))
                {
                    //set the arguments given in the query
                    foreach (var pair in args)
                    {
                        cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                    }

                    //execute the query and get the number of row affected
                    numberOfRowsAffected = cmd.ExecuteNonQuery();
                }

                return numberOfRowsAffected;
            }
        }

        private ClientData Execute(string query)
        {
            if (string.IsNullOrEmpty(query.Trim()))
                return null;

            using (var con = new SQLiteConnection("Data Source=test.db"))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(query, con))
                {
                    foreach (KeyValuePair<string, object> entry in args)
                    {
                        cmd.Parameters.AddWithValue(entry.Key, entry.Value);
                    }

                    var da = new SQLiteDataAdapter(cmd);

                    var dt = new ClientData();
                    da.Fill(dt);

                    da.Dispose();
                    return dt;
                }
            }
        }
        private int AddUser(User user)
        {
            const string query = "INSERT INTO User(FirstName, LastName) VALUES(@firstName, @lastName)";

            //here we are setting the parameter values that will be actually 
            //replaced in the query in Execute method
            var args = new Dictionary<string, object>
            {
                {"@firstName", user.FirstName},
                {"@lastName", user.Lastname}
            };

            return ExecuteWrite(query, args);
        }

        private int EditUser(User user)
        {
            const string query = "UPDATE User SET FirstName = @firstName, LastName = @lastName WHERE Id = @id";

            //here we are setting the parameter values that will be actually 
            //replaced in the query in Execute method
            var args = new Dictionary<string, object>
            {
                {"@id", user.Id},
                {"@firstName", user.FirstName},
                {"@lastName", user.Lastname}
            };

            return ExecuteWrite(query, args);
        }
    }

    
}
