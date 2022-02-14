using BombMoney;
using BombMoney.ResponseObjects;
using BombMoney.SmartContracts;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BombMoney.Bots
{
    public class xBombBot : BotBase
    {
        xBomb xBomb = null;

        public xBombBot(AConfigurationClass config, DiscordSocketClient client, BombMoneyOracle moneyOracle, BombMoneyTreasury moneyTreasury,
            IReadOnlyCollection<SocketGuild> socketGuilds, xBomb xBomb)
            : base(config, client, moneyOracle, moneyTreasury, socketGuilds)
        {
            this.xBomb = xBomb;
        }

        public override void Start()
        {
            _ = AsyncGetTotalxBombStaked();
            _ = AsyncGetExchangeRate();
        }

        private async Task AsyncGetExchangeRate()
        {
            Logging.WriteToConsole("Getting xBomb Exchange Rate");
            while (true)
            {
                try
                {
                    var exRate = xBomb.GetExchangeRate();

                    if (SocketGuilds != null && SocketGuilds.Count > 0)
                        foreach (var guild in SocketGuilds)
                        {
                            var user = guild.GetUser(Client.CurrentUser.Id);

                            await user.ModifyAsync(x =>
                            {
                                x.Nickname = exRate.ToString() + " xBOMB";
                            });
                        }
                    else
                        Logging.WriteToConsole("Bot is not a part of any guilds.");

                    Logging.WriteToConsole(exRate.ToString());
                }
                catch (Exception e)
                {
                    Logging.WriteToConsole(e.ToString());
                }

                //Pause for 10seconds
                await Task.Delay(300000);
            }
        }

        private async Task AsyncGetTotalxBombStaked()
        {
            Logging.WriteToConsole("Getting Total xBomb Staked");
            while (true)
            {
                try
                {
                    var exRate = xBomb.GetExchangeRate();
                    var supplyD = xBomb.GetTotalSupply();

                    var xbombStaked = Decimal.Round(exRate * supplyD, 0);

                    Logging.WriteToConsole($"Total Staked: {xbombStaked}");
                    await Client.SetActivityAsync(new Game("Staked: " + xbombStaked.ToString("N0"), ActivityType.Watching, ActivityProperties.None, null));
                }
                catch (Exception e)
                {
                    Logging.WriteToConsole(e.ToString());
                }

                //Pause for 10seconds
                await Task.Delay(300000);
            }
        }
    }
}
