using NBitcoin;
using Nethereum.HdWallet;
using Nethereum.Web3;
using System.Data.SQLite;

namespace ETH_Generator
{

    internal class Program
    {
        public static int index = 0;
        public static int guess = 0;
        public static readonly int delay = 120;

        static void Main(string[] args)
        {
            //Create the db if not created yet
            InitializeDataSet();

            //Open the connection to the database
            var con = new SQLiteConnection(Paths.database_connection);
            con.Open();

            //Make connection to the blockchain
            var web3 = new Web3("YOUR WEB3 SERVER");

            //Call the execution
            Generate(web3, con);
        }

        static async void Generate(Web3 web3, SQLiteConnection con)
        {

            while (true)
            {
                Thread thread = new(async () =>
                {
                    var mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
                    var password = "password";
                    var wallet = new Wallet(mnemonic.ToString(), password);
                    var account = wallet.GetAccount(0);

                    var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address);

                    if (Web3.Convert.FromWei(balance.Value) != 0)
                    {
                        guess++;
                        Console.WriteLine("FUCK YES");
                        var data_cmd = new SQLiteCommand(con)
                        {
                            CommandText = $@"INSERT INTO data(key, balance) VALUES ('{account.PrivateKey}', '{balance}')"
                        };
                        data_cmd.ExecuteNonQuery();
                    }
                    Console.WriteLine("[Checked] " + index.ToString() + " \n[Guessed] " + guess.ToString() + " \n[Address] " + account.Address + " \n[Private] " + account.PrivateKey + " \n[Balance] " + balance + "\n----------------");
                    index++;
                });
                thread.Start();
                Thread.Sleep(delay);
            }
        }

        public static void InitializeDataSet()
        {
            if (!Directory.Exists(Paths.fileLocation))
            {
                Directory.CreateDirectory(Paths.fileLocation);
            }

            if (!File.Exists(Paths.database))
            {
                var con = new SQLiteConnection(Paths.database_connection);
                File.WriteAllText(Paths.database, null);
                con.Open();
                var data_cmd = new SQLiteCommand(con)
                {
                    CommandText = @"CREATE TABLE data(key VARCRHAR(250), balance VARCRHAR(250))"
                };

                data_cmd.ExecuteNonQuery();
                con.Close();
            }
        }
    }
}




