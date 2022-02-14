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
    public class BombMoneyOracle : SmartContract
    {
        public string TokenContract { get; set; }

        public BombMoneyOracle(string url, string oracleContract, string abi, string tokenContract) : base(url, oracleContract, abi)
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

    public class BombMoneyTreasury : SmartContract
    {
        public BombMoneyTreasury(string url, string treasuryContract, string abi) : base(url, treasuryContract, abi)
        {

        }

        public async Task<decimal> GetBombPriceAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("getBombPrice");

                var result = await function.CallAsync<BigInteger>();

                return FormatNumberAsDecimal(result, 4, 4);
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
        public async Task<decimal> PreviousEpochBombPriceAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("previousEpochBombPrice");

                var result = await function.CallAsync<BigInteger>();

                return FormatNumberAsDecimal(result, 4, 4);
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
        public decimal PreviousEpochBombPrice()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("previousEpochBombPrice");

                var result = function.CallAsync<BigInteger>().ConfigureAwait(false).GetAwaiter().GetResult();

                return FormatNumberAsDecimal(result, 4, 4);
            }
            catch
            {
                throw;
            }
        }

        public async Task<decimal> GetTreasuryBalanceAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("seigniorageSaved");

                var result = await function.CallAsync<BigInteger>();

                return FormatNumberAsDecimal(result, 6, 0);
            }
            catch
            {
                throw;
            }
        }

        public decimal GetTreasuryBalance()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("seigniorageSaved");

                var result = function.CallAsync<BigInteger>().ConfigureAwait(false).GetAwaiter().GetResult();

                return FormatNumberAsDecimal(result, 6, 0);
            }
            catch
            {
                throw;
            }
        }

        public async Task<decimal> GetBombCirculatingSupplyAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("getBombCirculatingSupply");

                var result = await function.CallAsync<BigInteger>();

                return FormatNumberAsDecimal(result, 6, 0);
            }
            catch
            {
                throw;
            }
        }

        public decimal GetBombCirculatingSupply()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("getBombCirculatingSupply");

                var result = function.CallAsync<BigInteger>().ConfigureAwait(false).GetAwaiter().GetResult();

                return FormatNumberAsDecimal(result, 6, 0);
            }
            catch
            {
                throw;
            }
        }

        public int GetEpoch()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("epoch");

                var result = function.CallAsync<BigInteger>().ConfigureAwait(false).GetAwaiter().GetResult();

                return (int)result;
            }
            catch
            {
                throw;
            }
        }
    }

    public class xBomb : SmartContract
    {
        public xBomb(string url, string xBOMBContract, string abi) : base(url, xBOMBContract, abi)
        { }

        public decimal GetExchangeRate()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("getExchangeRate");

                var result = function.CallAsync<BigInteger>().ConfigureAwait(false).GetAwaiter().GetResult();

                return FormatNumberAsDecimal(result, 1, 4);
            }
            catch
            {
                throw;
            }
        }

        public decimal GetTotalSupply()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("totalSupply");

                var result = function.CallAsync<BigInteger>().ConfigureAwait(false).GetAwaiter().GetResult();

                return FormatNumberAsDecimal(result, 6, 0);
            }
            catch
            {
                throw;
            }
        }
    }

    public abstract class SmartContract
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

        internal static decimal FormatNumberAsDecimal(BigInteger number, int startIndex, int decimals)
        {
            try
            {
                string resultString = number.ToString().PadLeft(18, '0');
                decimal resultD = decimal.Round(decimal.Parse(resultString.Insert(startIndex, ".")), decimals);

                return resultD;
            }
            catch
            {
                throw;
            }
        }
    }
}
