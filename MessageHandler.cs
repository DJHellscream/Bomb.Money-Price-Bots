using BombMoney.ResponseObjects;
using BombMoney.SmartContracts;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombMoney
{
    public class MessageHandler
    {
        public BombMoneyTreasury Treasury { get; set; }
        public CMCBomb CMCBomb { get; set; }
        public MessageHandler(CMCBomb cmcBomb, BombMoneyTreasury treasury)
        {
            CMCBomb = cmcBomb;
            Treasury = treasury;
        }

        public Embed ProcessMessage(string message)
        {
            EmbedBuilder embed = new EmbedBuilder();
            try
            {
                if (message.ToLower().Equals("wen peg") || message.ToLower().Equals("wen print"))
                {
                    Random rand = new Random();
                    int choice = rand.Next(1, 3);
                    switch (choice)
                    {
                        case 1:
                            embed.AddField("ok bro", "buy more bomb then we can talk <:doxxedbifkn:928415925483487272>", false);
                            break;
                        case 2:
                            embed.AddField("Peek this", "<a:Bombnuke:928267878296338523>", false);
                            embed.ImageUrl = "https://i.imgur.com/zceO5Mx.png";
                            break;
                        default:
                            break;
                    }

                    return embed.Build();
                }
                else if (message.ToLower().Equals("wen lambo"))
                {
                    embed.AddField("Acquiring funds", "Calling dealership....", false);
                    embed.AddField("Offer accepted", "Lambo acquired!", true);
                    embed.ImageUrl = "https://media.giphy.com/media/l46Cw3404vdkvNqAU/giphy.gif";
                    return embed.Build();
                }
                else if (message.ToLower().Equals("lfg"))
                {
                    embed.AddField("Yeah baby!", "LFG", false);
                    embed.ImageUrl = "https://media.giphy.com/media/73oW01Plu9O5HAOdEH/giphy.gif";

                    return embed.Build();
                }
                else if (message.ToLower().Equals("huddleup") || message.ToLower().Equals("huddle up"))
                {
                    embed.AddField("BOMBERS...", "Get in here!", false);
                    embed.ImageUrl = "https://media.giphy.com/media/YBIZvOEg4vp3bav3ki/giphy.gif";

                    return embed.Build();
                }
                else if (message.ToLower().Equals("wen 5b tvl"))
                {
                    embed.AddField("Deal...", "!", false);
                    embed.ImageUrl = "https://i.imgur.com/4z9tkE8.png";

                    return embed.Build();
                }

                if (message.ToLower().StartsWith('?'))
                {
                    if (message.ToLower().Equals("?c"))
                    {
                        embed.AddField(BuildCommandList());
                    }
                    else if (message.ToLower().Equals("?rpc"))
                    {
                        BuildRPCFields(embed);
                    }
                    else if (message.ToLower().Equals("?stats") || message.ToLower().Equals("?s"))
                    {
                        BuildStatsFields(embed);
                    }

                    if (embed.Fields.Count > 0)
                        return embed.Build();
                }
            }
            catch (Exception ex)
            {
                Logging.WriteToConsole(ex.ToString());
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

        private static void BuildRPCFields(EmbedBuilder embed)
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
            embed.AddField($"Fully Diluted MarketCap: ", CMCBomb.Data.BombInfo.Quote.USD.FullyDilutedMarketCap.ToString("N0"), false);
            embed.AddField($"Circulating Supply: ", Treasury.GetBombCirculatingSupply().ToString("N0"), false);
            embed.AddField($"Treasury Balance: ", Treasury.GetTreasuryBalance().ToString("N0"), false);
        }
    }
}
