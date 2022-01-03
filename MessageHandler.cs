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
        readonly CMCBomb _cmcBomb;
        public MessageHandler(CMCBomb cmcBomb)
        {
            _cmcBomb = cmcBomb;
        }

        public Embed ProcessMessage(string message)
        {
            EmbedBuilder embed = new EmbedBuilder();

            if (message.StartsWith('?'))
            {
                if (message.Equals("?") || message.Equals("?c"))
                {
                    embed.AddField(BuildCommandList());
                }
                else if (message.Contains("mcap"))
                {
                    embed.AddField("Symbol", "BOMB", true);
                    embed.AddField($"Fully Diluted MarketCap: ", _cmcBomb.Data.BombInfo.Quote.USD.FullyDilutedMarketCap, false);
                }
                else if (message.Contains("rpc"))
                {
                    embed.Title = "bomb.money custom RPC";
                    embed.AddField("Network Name:", "BSC Mainnet (BOMB RPC)", false);
                    embed.AddField("RPC URL:", "https://bsc1.bomb.money", false);
                    embed.AddField("Chain ID:", "56", false);
                    embed.AddField("Currency Symbol:", "BNB", false);
                    embed.AddField("Block Explorer:", "https://bscscan.com", false);
                }
                else
                {
                    embed.AddField("Huh?", "wut dat mean? BUIDL'ING...", false);
                }

                return null;
            }

            return null;
        }

        private EmbedFieldBuilder BuildCommandList()
        {
            EmbedFieldBuilder efb = new EmbedFieldBuilder();
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("mcap - Fully Diluted MarketCap");
            sb.AppendLine("rpc - RPC Node Information");

            efb.Name = "Command List:";
            efb.Value = sb.ToString();
            efb.IsInline = false;

            return efb;
        }
    }
}
