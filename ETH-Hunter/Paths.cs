﻿namespace ETH_HUNTER
{
    public static class Paths
    {
        public static readonly string fileLocation = "C:\\ETH\\";
        public static readonly string database = "c:\\ETH\\data.db";
        public static readonly string id = "c:\\ETH\\id.txt";
        public static readonly string database_connection = @"URI=file:C:\ETH\data.db";
        public static readonly string[] web3Urls = {
            "https://ethereum.publicnode.com",
            "https://nodes.mewapi.io/rpc/eth", //this errors
            "https://cloudflare-eth.com/",
            "https://rpc.flashbots.net/", //this errors
            "https://rpc.ankr.com/eth", //this errors
            "https://eth-mainnet.public.blastapi.io"
        };
    }
}
