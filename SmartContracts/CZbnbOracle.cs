using Nethereum.Contracts;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BombMoney.SmartContracts
{
    public class CZbnbOracle : SmartContract
    {
        public string TokenContract { get; set; }

        public CZbnbOracle(string url, string oracleContract, string abi, string tokenContract) : base(url, oracleContract, abi)
        {
            TokenContract = tokenContract;
        }

        public async Task<decimal> TWAPAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("twap");

                var result = await function.CallAsync<BigInteger>(new object[] { TokenContract, 1000000000000000000 });

                return FormatNumberAsDecimal(result, 4, 4);
            }
            catch
            {
                throw;
            }
        }

        public async Task<decimal> ConsultAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("consult");

                var result = await function.CallAsync<BigInteger>(new object[] { TokenContract, 1000000000000000000 });

                return FormatNumberAsDecimal(result, 4, 4);
            }
            catch
            {
                throw;
            }
        }

        public int GetCurrentEpoch()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("getCurrentEpoch");

                var result = function.CallAsync<BigInteger>().ConfigureAwait(false).GetAwaiter().GetResult();

                return (int)result;
            }
            catch
            {
                throw;
            }
        }
    }
}
