using Nethereum.Contracts;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BombPriceBot.SmartContracts
{
    internal class BombMoneyOracle : SmartContract
    {
        public string TokenContract { get; set; }

        public BombMoneyOracle(string url, string oracleContract, string abi, string tokenContract) : base(url, oracleContract, abi)
        {
            TokenContract = tokenContract;
        }

        public async Task<BigInteger> TWAPAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("twap");

                return await function.CallAsync<BigInteger>(new object[] { TokenContract, 1000000000000000000 });
            }
            catch
            {
                throw;
            }
        }

        public async Task<BigInteger> ConsultAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("consult");

                return await function.CallAsync<BigInteger>(new object[] { TokenContract, 1000000000000000000 });
            }
            catch
            {
                throw;
            }
        }
    }

    internal class BombMoneyTreasury : SmartContract
    {
        public BombMoneyTreasury(string url, string treasuryContract, string abi) : base(url, treasuryContract, abi)
        {

        }

        public async Task<BigInteger> GetBombPriceAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("getBombPrice");

                return await function.CallAsync<BigInteger>();
            }
            catch
            {
                throw;
            }
        }

        public async Task<BigInteger> PreviousEpochBombPriceAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("previousEpochBombPrice");

                return await function.CallAsync<BigInteger>();
            }
            catch
            {
                throw;
            }
        }
    }

    internal abstract class SmartContract
    {
        public Web3 Client { get; set; }

        public string ContractAddress { get; set; }

        public string ABI { get; set; }

        public SmartContract(string url, string contract, string abi)
        {
            Client = new Web3(url);
            ContractAddress = contract;
            ABI = abi;
        }
    }
}
