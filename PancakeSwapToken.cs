using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombPriceBot
{
    internal class PancakeSwapToken
    {
        public TokenData Data { get; set; }
        public string Updated_At { get; set; }
    }

    internal class TokenData
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string Price { get; set; }
        public string Price_BNB { get; set; }
    }
}
