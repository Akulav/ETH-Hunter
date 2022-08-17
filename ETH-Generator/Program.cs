using NBitcoin;
using Nethereum.HdWallet;
using Nethereum.Web3;

namespace ETH_Generator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var web3 = new Web3("https://newest-powerful-mansion.discover.quiknode.pro/e3cbef755ee0582620a34cf99e49d9e20f79a353/");

            while (false)
            {
                var mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
                Console.WriteLine("The 12 seed words are: " + mnemonic.ToString());

                var password = "password";
                var wallet = new Wallet(mnemonic.ToString(), password);
                var account = wallet.GetAccount(0);

                var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
                //Console.WriteLine("Address at Index 0 is: " + account.Address + " with private key:" + account.PrivateKey);
                Console.WriteLine("Balance of Ethereum Foundation's account in Ether: " + Web3.Convert.FromWei(balance.Value));
                if (Web3.Convert.FromWei(balance.Value) == 0) { Console.WriteLine("Its a zero wallet"); }
                else { Console.WriteLine("YESSSSS"); }
            }

            Generate(web3);
       
        }

        static void Generate(Web3 web3)
        {
            while (true)
            {
                Thread thread = new(async() =>
                {

                    var mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
                    Console.WriteLine("The 12 seed words are: " + mnemonic.ToString());

                    var password = "password";
                    var wallet = new Wallet(mnemonic.ToString(), password);
                    var account = wallet.GetAccount(0);

                    //var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
                   // Console.WriteLine("Address at Index 0 is: " + account.Address + " with private key:" + account.PrivateKey);
                    //Console.WriteLine("Balance of Ethereum Foundation's account in Ether: " + Web3.Convert.FromWei(balance.Value));
                   // if (Web3.Convert.FromWei(balance.Value) == 0) { Console.WriteLine("Its a zero wallet"); }
                    //else { Console.WriteLine("YESSSSS"); }

                });
                thread.Start();
               // Thread.Sleep(350);
            }
        }

    }
}




