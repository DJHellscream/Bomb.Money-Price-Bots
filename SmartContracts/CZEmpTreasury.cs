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
    public class CZEmpTreasury : SmartContract
    {
        public CZEmpTreasury(string url, string treasuryContract, string abi) : base(url, treasuryContract, abi)
        {

        }

        public async Task<decimal> GetBombPriceAsync()
        {
            try
            {
                var contract = Client.Eth.GetContract(ABI, ContractAddress);
                var function = contract.GetFunction("getTokenPrice");

                var result = await function.CallAsync<BigInteger>();

                return FormatNumberAsDecimal(result, 1, 4);
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
                var function = contract.GetFunction("previousEpochTokenPrice");

                var result = await function.CallAsync<BigInteger>();

                return FormatNumberAsDecimal(result, 1, 4);
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
                var function = contract.GetFunction("previousEpochTokenPrice");

                var result = function.CallAsync<BigInteger>().ConfigureAwait(false).GetAwaiter().GetResult();

                return FormatNumberAsDecimal(result, 1, 4);
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
                var function = contract.GetFunction("getTokenCirculatingSupply");

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
                var function = contract.GetFunction("getTokenCirculatingSupply");

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
}
