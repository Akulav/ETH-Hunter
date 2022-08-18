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
        static void Main(string[] args)
        {
            var con = new SQLiteConnection(Paths.database_connection);
            con.Open();
            InitializeDataSet(con);
            var web3 = new Web3("https://newest-powerful-mansion.discover.quiknode.pro/e3cbef755ee0582620a34cf99e49d9e20f79a353/");
            Generate(web3, con);
        }

        static void Generate(Web3 web3, SQLiteConnection con)
        {
            while (true)
            {
                Thread thread = new(async() =>
                {

                    var mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
                    //Console.WriteLine("The 12 seed words are: " + mnemonic.ToString());

                    var password = "password";
                    var wallet = new Wallet(mnemonic.ToString(), password);
                    var account = wallet.GetAccount(0);

                    var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
                    //Console.WriteLine("Address at Index 0 is: " + account.Address + " with private key:" + account.PrivateKey);
                    //Console.WriteLine("Balance of Ethereum Foundation's account in Ether: " + Web3.Convert.FromWei(balance.Value));
                    if (Web3.Convert.FromWei(balance.Value) == 0) { Console.WriteLine("Verified " + index.ToString() + " Guessed " + guess.ToString()); }
                    else {
                        guess++;
                        Console.WriteLine("Verified " + index.ToString() + " Guessed " + guess.ToString());
                        var data_cmd = new SQLiteCommand(con)
                        {
                            CommandText = $@"INSERT INTO data(key, balance) VALUES ('{account.PrivateKey}', '{balance}')"
                        };
                        data_cmd.ExecuteNonQuery();
                    }

                    index++;
                });
                thread.Start();
                Thread.Sleep(210);
            }
        }

        public static void InitializeDataSet(SQLiteConnection con)
        {
            try
            {
                if (!Directory.Exists(Paths.fileLocation))
                {
                    Directory.CreateDirectory(Paths.fileLocation);
                }

                if (!File.Exists(Paths.database))
                {
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
            catch { }
        }

    }
}




