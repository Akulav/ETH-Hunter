using ETH_Generator.Controllers;
using NBitcoin;
using Nethereum.HdWallet;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Collections.Concurrent;
using System.Data.SQLite;
using System.Diagnostics;

namespace ETH_HUNTER
{
    internal class Program
    {
        public static int index = 0;
        public static int guess = 0;
        public static int errors = 0;
        public static readonly int delay = 0;
        public static string id = IdController.GetId();
        public static string error = "[Errored urls]: " + "\n";
        public static readonly int threadCount = 16; // Number of threads per URL
        public static readonly int suspensionDuration = 10000; // Suspension duration in milliseconds

        private static ConcurrentDictionary<string, DateTime> suspendedUrls = new();
        private static SemaphoreSlim semaphore = new(threadCount);
        private static long lastIndex = 0;
        private static Stopwatch cpsStopwatch = new Stopwatch();

        static void Main()
        {
            DatabaseController dc = new();

            index = dc.getCount();

            // Open the connection to the database
            using (var con = new SQLiteConnection(Paths.database_connection))
            {
                con.Open();

                // Start clock
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                cpsStopwatch.Start();

                _ = Task.Run(() => ConsoleUpdater(stopwatch, dc));

                // Call the execution
                while (true)
                {
                    Generate(dc, stopwatch).Wait();
                    Thread.Sleep(delay);
                }
            }
        }

        private static async Task ConsoleUpdater(Stopwatch watch, DatabaseController dc)
        {
            while (true)
            {
                long checksPerSecond = 0;
                if (cpsStopwatch.ElapsedMilliseconds > 0)
                {
                    checksPerSecond = (index - lastIndex) * 1000 / cpsStopwatch.ElapsedMilliseconds;
                }

                Console.Clear();
                Console.WriteLine($"[Elapsed] {watch.Elapsed} \n[Checked] {index} \n[Guessed] {guess} \n[Problem] {errors / 18}\n[Check/s] {checksPerSecond}\n======ETH-HUNTER-V1.0======");

                if (cpsStopwatch.ElapsedMilliseconds >= 1000)
                {
                    lastIndex = index;
                    cpsStopwatch.Restart();
                }

                await Task.Delay(100); // Update console every second
            }
        }

        private static async Task Generate(DatabaseController dc, Stopwatch watch)
        {
            async Task ProcessThreadAsync(Web3 web, string url)
            {
                while (true)
                {
                    if (suspendedUrls.TryGetValue(url, out var suspensionEnd) && DateTime.UtcNow < suspensionEnd)
                    {
                        await Task.Delay(1000); // Wait before rechecking the suspension
                        continue;
                    }

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

                        long checksPerSecond = 0;
                        if (cpsStopwatch.ElapsedMilliseconds > 0)
                        {
                            checksPerSecond = (index - lastIndex) * 1000 / cpsStopwatch.ElapsedMilliseconds;
                        }

                        //Console.Clear();
                        //Console.WriteLine($"[Elapsed] {watch.Elapsed} \n[Checked] {index} \n[Guessed] {guess} \n[Address] {account.Address} \n[Balance] {balance}\n[Problem] {errors / 18}\n[Checks/s] {checksPerSecond}\n======ETH-HUNTER-V1.0======");

                        if (cpsStopwatch.ElapsedMilliseconds >= 1000)
                        {
                            lastIndex = index;
                            cpsStopwatch.Restart();
                        }
                    }
                    catch
                    {
                        Interlocked.Increment(ref errors);
                        suspendedUrls[url] = DateTime.UtcNow.AddMilliseconds(suspensionDuration);
                        break; // Exit this thread due to an error
                    }

                    Interlocked.Increment(ref index);

                    if (index % 1000000 == 0)
                    {
                        MailController.sendMail("Status: Alive" + "\nIndex: " + index + "\nID: " + id, id);
                    }

                    await Task.Delay(delay); // Delay between attempts
                }
            }

            var tasks = new List<Task>();

            foreach (var url in Paths.web3Urls)
            {
                for (int i = 0; i < threadCount; i++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            await ProcessThreadAsync(new Web3(url), url);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
