using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombMoney.ResponseObjects
{
    internal class MoralisToken
    {
        public NativePrice nativePrice { get; set; }
        public string usdPrice { get; set; }
        public string exchangeAddress { get; set; }
        public string exchangeName { get; set; }
    }

    internal class NativePrice
    {
        public string value { get; set; }
        public string decimals { get; set; }
        public string name { get; set; }
        public string symbol { get; set; }
    }

    //{
    //  "nativePrice":
    //      {
    //          "value":"8919233060706269",
    //          "decimals":18,
    //          "name":"Binance Coin",
    //          "symbol":"BNB"
    //      },
    //  "usdPrice":4.760597645763223,
    //  "exchangeAddress":"0xcA143Ce32Fe78f1f7019d7d551a6402fC5350c73",
    //  "exchangeName":"PancakeSwap v2"
    //}
}
