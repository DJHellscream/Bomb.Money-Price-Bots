using BombPriceBot.SmartContracts;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombPriceBot
{
    internal class MessageHandler
    {
        readonly BombMoneyTreasury _treasury;
        readonly CMCBomb _cmcBomb;
        public MessageHandler(CMCBomb cmcBomb, BombMoneyTreasury treasury)
        {
            _cmcBomb = cmcBomb;
            _treasury = treasury;
        }

        public Embed ProcessMessage(string message)
        {
            EmbedBuilder embed = new EmbedBuilder();
            try
            {
                if (message.StartsWith('?'))
                {
                    if (message.Equals("?c"))
                    {
                        embed.AddField(BuildCommandList());
                    }
                    else if (message.Equals("?rpc"))
                    {
                        BuildRPCFields(embed);
                    }
                    else if (message.Equals("?stats") || message.Equals("?s"))
                    {
                        BuildStatsFields(embed);
                    }
                    else if (message.Length > 1)
                    {
                        embed.AddField("Huh?", "wut dat mean? BUIDL'ING...", false);
                    }

                    if (embed.Fields.Count > 0)
                        return embed.Build();
                }
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }

            return null;
        }

        private static EmbedFieldBuilder BuildCommandList()
        {
            EmbedFieldBuilder efb = new EmbedFieldBuilder();
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("mcap - Fully Diluted MarketCap");
            sb.AppendLine("rpc - RPC Node Information");
            sb.AppendLine("stats or s - Bomb.money statistics");

            efb.Name = "Command List:";
            efb.Value = sb.ToString();
            efb.IsInline = false;

            return efb;
        }

        private void BuildRPCFields(EmbedBuilder embed)
        {
            embed.Title = "bomb.money custom RPC";
            embed.AddField("Network Name:", "BSC Mainnet (BOMB RPC)", false);
            embed.AddField("RPC URL:", "https://bsc1.bomb.money", false);
            embed.AddField("Chain ID:", "56", false);
            embed.AddField("Currency Symbol:", "BNB", false);
            embed.AddField("Block Explorer:", "https://bscscan.com", false);
        }

        private void BuildStatsFields(EmbedBuilder embed)
        {
            embed.AddField("Symbol", "BOMB", true);
            embed.AddField($"Fully Diluted MarketCap: ", _cmcBomb.Data.BombInfo.Quote.USD.FullyDilutedMarketCap.ToString("N0"), false);
            embed.AddField($"Circulating Supply: ", _treasury.GetBombCirculatingSupply().ToString("N0"), false);
            embed.AddField($"Treasury Balance: ", _treasury.GetTreasuryBalance().ToString("N0"), false);
        }

        public void WriteToConsole(String message)
        {
            Console.WriteLine(message);
        }
    }
}
