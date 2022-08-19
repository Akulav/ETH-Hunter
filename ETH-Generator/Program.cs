using NBitcoin;
using Nethereum.HdWallet;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Data.SQLite;
using System.Diagnostics;

namespace ETH_Generator
{

    internal class Program
    {
        public static int index = 0;
        public static int guess = 0;
        public static readonly int delay = 80;

        static void Main()
        {
            //Create the db if not created yet
            InitializeDataSet();

            //Open the connection to the database
            var con = new SQLiteConnection(Paths.database_connection);
            con.Open();

            //Make connection to the blockchain
            var web3 = new Web3("https://newest-powerful-mansion.discover.quiknode.pro/e3cbef755ee0582620a34cf99e49d9e20f79a353/");

            //Start clock
            Stopwatch stopwatch = new();
            stopwatch.Start();

            //Call the execution
            Generate(web3, con, stopwatch);
        }

        static async void Generate(Web3 web3, SQLiteConnection con, Stopwatch watch)
        {

            while (true)
            {
                Thread thread = new(async () =>
                {
                    Mnemonic mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
                    string password = "";
                    Wallet wallet = new Wallet(mnemonic.ToString(), password);
                    Account account = wallet.GetAccount(0);

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
                    Console.WriteLine("[Elapsed] " + watch.Elapsed + " \n[Checked] " + index.ToString() + " \n[Guessed] " + guess.ToString() + " \n[Address] " + account.Address + " \n[Private] " + account.PrivateKey + " \n[Balance] " + balance + "\n----------------");
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




