using ETH_Generator.Controllers;
using NBitcoin;
using Nethereum.HdWallet;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Data.SQLite;
using System.Diagnostics;

namespace ETH_HUNTER
{

    internal class Program
    {
        public static int index = 0;
        public static int guess = 0;
        public static int errors = 0;
        public static readonly int delay = 50;
        public static string id = IdController.GetId();

        static void Main()
        {
            DatabaseController dc = new();

            // Open the connection to the database
            using (var con = new SQLiteConnection(Paths.database_connection))
            {
                con.Open();

                // Start clock
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                // Call the execution
                while (true)
                {
                    Generate(dc, stopwatch);
                    Thread.Sleep(delay);
                }
            }
        }

        private static async void Generate(DatabaseController dc, Stopwatch watch)
        {
            async Task ProcessThreadAsync(Web3 web, int threadIndex)
            {
                Mnemonic mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
                string password = "";
                Wallet wallet = new Wallet(mnemonic.ToString(), password);
                Account account = wallet.GetAccount(0);

                try
                {
                    var balance = await web.Eth.GetBalance.SendRequestAsync(account.Address);
                    if (Web3.Convert.FromWei(balance.Value) != 0)
                    {
                        Interlocked.Increment(ref guess);
                        MailController.sendMail("Address found: " + account.PrivateKey, id);
                    }

                    dc.InsertAccount(account.PrivateKey, balance.ToString(), account.Address);

                    Console.Clear();
                    Console.WriteLine($"[Elapsed] {watch.Elapsed} \n[Checked] {index} \n[Guessed] {guess} \n[Address] {account.Address} \n[Private] {account.PrivateKey} \n[Balance] {balance}\n[Problem] {errors / 18} \n======ETH-HUNTER-V1.0======");
                }
                catch
                {
                    Interlocked.Increment(ref errors);
                }

                Interlocked.Increment(ref index);

                if (index % 500000 == 0)
                {
                    MailController.sendMail("Status: Alive" + "\nIndex: " + index + "\nID: " + id, id);
                }
            }

            var tasks = Paths.web3Urls.Select((url, index) => ProcessThreadAsync(new Web3(url), index));

            await Task.WhenAll(tasks);
        }


    }
}