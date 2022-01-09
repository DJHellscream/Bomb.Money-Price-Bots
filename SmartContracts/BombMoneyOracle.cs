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

        public async Task<Decimal> TWAPAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("twap");

                var result = await function.CallAsync<BigInteger>(new object[] { TokenContract, 1000000000000000000 });

                string twapString = result.ToString().PadLeft(18, '0');
                Decimal resultD = Decimal.Round(Decimal.Parse(twapString.Insert(4, ".")), 4);

                return resultD;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Decimal> ConsultAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("consult");

                var result = await function.CallAsync<BigInteger>(new object[] { TokenContract, 1000000000000000000 });

                string resultString = result.ToString().PadLeft(18, '0');
                Decimal resultD = Decimal.Round(Decimal.Parse(resultString.Insert(4, ".")), 4);

                return resultD;
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

        public async Task<Decimal> GetBombPriceAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("getBombPrice");

                var result = await function.CallAsync<BigInteger>();

                string resultString = result.ToString().PadLeft(18, '0');
                Decimal resultD = Decimal.Round(Decimal.Parse(resultString.Insert(4, ".")), 4);

                return resultD;
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// Gets the TWAP value that the epoch ended at. Used to determine what role to assign to the BombPriceBot
        /// </summary>
        /// <returns></returns>
        public async Task<Decimal> PreviousEpochBombPriceAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("previousEpochBombPrice");

                var result = await function.CallAsync<BigInteger>();

                string resultString = result.ToString().PadLeft(18, '0');
                Decimal resultD = Decimal.Round(Decimal.Parse(resultString.Insert(4, ".")), 4);

                return resultD;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Decimal> GetTreasuryBalanceAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("seigniorageSaved");

                var result = await function.CallAsync<BigInteger>();

                string resultString = result.ToString().PadLeft(18, '0');
                Decimal resultD = Decimal.Round(Decimal.Parse(resultString.Insert(6, ".")), 6);

                return resultD;
            }
            catch
            {
                throw;
            }
        }

        public Decimal GetTreasuryBalance()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("seigniorageSaved");

                var result = function.CallAsync<BigInteger>().ConfigureAwait(false).GetAwaiter().GetResult();

                string resultString = result.ToString().PadLeft(18, '0');
                Decimal resultD = Decimal.Round(Decimal.Parse(resultString.Insert(6, ".")), 0);

                return resultD;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Decimal> GetBombCirculatingSupplyAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("getBombCirculatingSupply");

                var result = await function.CallAsync<BigInteger>();

                string resultString = result.ToString().PadLeft(18, '0');
                Decimal resultD = Decimal.Round(Decimal.Parse(resultString.Insert(6, ".")), 0);

                return resultD;
            }
            catch
            {
                throw;
            }
        }

        public Decimal GetBombCirculatingSupply()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("getBombCirculatingSupply");

                var result = function.CallAsync<BigInteger>().ConfigureAwait(false).GetAwaiter().GetResult();

                string resultString = result.ToString().PadLeft(18, '0');
                Decimal resultD = Decimal.Round(Decimal.Parse(resultString.Insert(6, ".")), 0);

                return resultD;
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
