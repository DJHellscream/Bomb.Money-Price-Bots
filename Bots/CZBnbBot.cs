using BombMoney.Configurations;
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
    public class CZBnbBot : BotBase
    {
        private static readonly string red = "BoardroomPrinterRed";
        private static readonly string green = "BoardroomPrinterGreen";
        private static readonly string zen = "BoardroomPrinterZen";

        public CZBnbBot(TokenConfig config, DiscordSocketClient client, CZbnbOracle moneyOracle, CZbnbTreasury moneyTreasury,
            IReadOnlyCollection<SocketGuild> socketGuilds)
            : base(config, client, moneyOracle, moneyTreasury, socketGuilds)
        {
            Logging.WriteToConsole("Loading CZBnbBot...");
        }

        public override void Start()
        {
            // Start getting price
            base.Start();

            Client.MessageReceived += _client_MessageReceived;
            AsyncVerifyRoles().ConfigureAwait(false);
            _ = AsyncGetLastEpochTWAP();
            _ = AsyncGetTWAP();
        }

        private async Task _client_MessageReceived(SocketMessage arg)
        {
            Embed embed = this.ProcessMessage(arg, out _);

            if (embed != null)
            {
                MessageReference message = new(arg.Id, arg.Channel.Id);
                await arg.Channel.SendMessageAsync(null, false, null, null, null, message, null, null, new Embed[] { embed });
            }
        }

        public override Embed ProcessMessage(SocketMessage arg, out bool authorIsBot)
        {
            Embed embed = base.ProcessMessage(arg, out authorIsBot);

            if (embed == null && !authorIsBot)
            {
                embed = MessageHandler.ProcessMessage(arg.Content);
            }

            return embed;
        }

        private async Task AsyncVerifyRoles()
        {
            List<string> s = new List<string>() { red, green, zen };

            try
            {
                if (SocketGuilds != null && SocketGuilds.Count > 0)
                {
                    foreach (var guild in SocketGuilds)
                    {
                        IEnumerable<SocketRole> roles = guild.Roles.Where(x => x.Name == green || x.Name == red || x.Name == zen);

                        // Check to see if all 3 roles exist -- if they don't delete the ones that do and re-add all 3.
                        if (roles.Count() == s.Count)
                        {
                            Logging.WriteToConsole("Roles already exist");
                            return;
                        }
                        else
                        {
                            Logging.WriteToConsole($"Missing {s.Count - roles.Count()} roles.");
                            foreach (SocketRole role in roles)
                            {
                                await role.DeleteAsync();
                            }

                            foreach (string r in s)
                            {
                                SocketRole print = guild.Roles.SingleOrDefault(x => x.Name == r);
                                if (print == null)
                                {
                                    Color c;
                                    string name;
                                    if (r == red)
                                    {
                                        c = Color.Red;
                                        name = red;
                                    }
                                    else if (r == zen)
                                    {
                                        c = Color.Gold;
                                        name = zen;
                                    }
                                    else
                                    {
                                        name = green;
                                        c = Color.Green;
                                    }

                                    await guild.CreateRoleAsync(name, null, c, false, null);
                                    print = guild.Roles.SingleOrDefault(x => x.Name == name);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.WriteToConsole(e.ToString());
            }
        }

        private async Task AsyncGetTWAP()
        {
            Logging.WriteToConsole("Getting TWAP");
            while (true)
            {
                try
                {
                    var twapD = await ((CZbnbOracle)Oracle).TWAPAsync();

                    Logging.WriteToConsole($"TWAP: {twapD}");
                    await Client.SetActivityAsync(new Game("TWAP: " + twapD, ActivityType.Watching, ActivityProperties.None, null));
                }
                catch (Exception e)
                {
                    Logging.WriteToConsole(e.ToString());
                }

                //Pause for 10seconds
                await Task.Delay(10000);
            }
        }

        private async Task<object> AsyncGetLastEpochTWAP()
        {
            Logging.WriteToConsole("Getting Previous Epoch Price");
            while (true)
            {
                try
                {
                    var consultD = await ((CZbnbTreasury)Treasury).PreviousEpochBombPriceAsync();

                    if (Client.ConnectionState == ConnectionState.Connected)
                    {
                        string newNick = string.Empty;
                        if (SocketGuilds != null && SocketGuilds.Count > 0)
                        {
                            foreach (var guild in SocketGuilds)
                            {
                                // Using try-catches for logic is awful - change at some point
                                ulong redU;
                                ulong greenU;
                                ulong zenU;
                                try
                                {
                                    redU = guild.Roles.SingleOrDefault(x => x.Name == red).Id;
                                    greenU = guild.Roles.SingleOrDefault(x => x.Name == green).Id;
                                    zenU = guild.Roles.SingleOrDefault(x => x.Name == zen).Id;
                                }
                                catch
                                {
                                    await AsyncVerifyRoles();
                                    redU = guild.Roles.SingleOrDefault(x => x.Name == red).Id;
                                    greenU = guild.Roles.SingleOrDefault(x => x.Name == green).Id;
                                    zenU = guild.Roles.SingleOrDefault(x => x.Name == zen).Id;
                                }

                                ulong currentRole;
                                try
                                {
                                    currentRole = guild.CurrentUser.Roles.SingleOrDefault(x => x.Name == red || x.Name == green || x.Name == zen).Id;
                                }
                                catch
                                {
                                    currentRole = zenU;
                                    await guild.CurrentUser.AddRoleAsync(zenU);
                                }

                                if (consultD >= (decimal)1.0 && consultD < (decimal)1.01)
                                {
                                    if (currentRole != zenU)
                                    {
                                        await guild.CurrentUser.RemoveRoleAsync(currentRole);
                                        await guild.CurrentUser.AddRoleAsync(zenU);
                                    }
                                }
                                else if (consultD >= (decimal)1.01)
                                {
                                    if (currentRole != greenU)
                                    {
                                        await guild.CurrentUser.RemoveRoleAsync(currentRole);
                                        await guild.CurrentUser.AddRoleAsync(greenU);
                                    }
                                }
                                else if (consultD < (decimal)1.00)
                                {
                                    if (currentRole != redU)
                                    {
                                        await guild.CurrentUser.RemoveRoleAsync(currentRole);
                                        await guild.CurrentUser.AddRoleAsync(redU);
                                    }
                                }
                            }

                            Logging.WriteToConsole($"PrevPrice: {consultD}");
                        }
                    }

                    //Pause for 10seconds
                    await Task.Delay(10000);
                }
                catch (Exception e)
                {
                    Logging.WriteToConsole(e.ToString());
                }
            }
        }
    }
}