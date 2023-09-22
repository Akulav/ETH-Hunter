using ETH_HUNTER;
using System.Data.SQLite;

namespace ETH_Generator.Controllers
{
    public class DatabaseController
    {
        private SQLiteConnection con;
        public DatabaseController()
        {
            InitializeDataSet();
            con = new SQLiteConnection(Paths.database_connection);
            con.Open();
        }

        private void InitializeDataSet()
        {
            if (!Directory.Exists(Paths.fileLocation))
            {
                Directory.CreateDirectory(Paths.fileLocation);
            }

            if (!File.Exists(Paths.database))
            {
                var connection = new SQLiteConnection(Paths.database_connection);
                File.WriteAllText(Paths.database, null);
                connection.Open();
                var data_cmd = new SQLiteCommand(connection)
                {
                    CommandText = @"CREATE TABLE data(key VARCRHAR(250), balance VARCRHAR(250) , address VARCHAR(250))"
                };

                data_cmd.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void InsertAccount(string PrivateKey, string balance, string Address)
        {
            var data_cmd = new SQLiteCommand(con)
            {
                CommandText = $@"INSERT INTO data(key, balance, address) VALUES ('{PrivateKey}', '{balance}', '{Address}')"
            };

            data_cmd.ExecuteNonQuery();
        }

    }
}
