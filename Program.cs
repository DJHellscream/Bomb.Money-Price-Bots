using Discord;
using Discord.WebSocket;
using Nethereum.Web3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BombPriceBot.SmartContracts;
using System.Timers;
using BombPriceBot.Database;
using System.Diagnostics;

namespace BombPriceBot
{
    internal class Program
    {
        private static readonly string red = "BoardroomPrinterRed";
        private static readonly string green = "BoardroomPrinterGreen";
        private static readonly string zen = "BoardroomPrinterZen";
        private static readonly string boardroomHistory = "boardroom-history";
        IReadOnlyCollection<SocketGuild> _guilds;
        AConfigurationClass _configClass;
        DiscordSocketClient _client;
        BombMoneyOracle _moneyOracle;
        BombMoneyTreasury _moneyTreasury;
        CMCBomb _cmcBomb;
        MessageHandler _handler;
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            try
            {
                _configClass = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json"));
                _moneyOracle = new BombMoneyOracle(_configClass.BscScanRPC, _configClass.OracleContract, _configClass.OracleABI, _configClass.TokenContract);
                _moneyTreasury = new BombMoneyTreasury(_configClass.BscScanRPC, _configClass.TreasuryContract, _configClass.TreasuryABI);

                await LoginAndConnect();

                _client.JoinedGuild += _client_JoinedGuild;
                _client.GuildAvailable += _client_GuildAvailable;
                _client.GuildUpdated += _client_GuildUpdated;

                _client.Ready += () => { Console.WriteLine("Bot is connected!"); return Task.CompletedTask; };
                await Task.Delay(3000);

                _ = AsyncGetPrice();
                if (_configClass.TokenSymbol.Equals("BOMB"))
                {
                    _client.MessageReceived += _client_MessageReceived;
                    await AsyncVerifyRoles();
                    _ = AsyncGetLastEpochTWAP();
                    _ = AsyncGetTWAP();
                    _ = AsyncPollCMCData<CMCBomb>();
                    _handler = new MessageHandler(_cmcBomb, _moneyTreasury);
                }
                else if (_configClass.TokenSymbol.Equals("BSHARE"))
                {
                    _ = StartEpochTimer();
                }

                // Block this task until the program is closed.
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Logging.WriteToConsole(ex.ToString());
            }

            t.Stop();
            _client.Dispose();
        }

        Timer t = null;
        private async Task StartEpochTimer()
        {
            t = new Timer(30500);
            t.Elapsed += new ElapsedEventHandler(UpdateEpochTime);
            t.Start();
            await Task.CompletedTask;
        }

        private void UpdateEpochTime(object sender, ElapsedEventArgs e)
        {
            try
            {
                DateTime day = DateTime.Today;
                int nextEpochHour;
                int currentHour = DateTime.Now.TimeOfDay.Hours;
                if (currentHour >= 18)
                {
                    nextEpochHour = 0;
                    day = DateTime.Today + new TimeSpan(1, 0, 0, 0);
                }
                else
                    nextEpochHour = currentHour + (6 - currentHour % 6);

                TimeSpan timeRemaining = new DateTime(day.Year, day.Month, day.Day, nextEpochHour, 0, 0).Subtract(DateTime.Now);
                string format = @"hh\:mm\:ss";
                string displayString = timeRemaining.ToString(format);

                Logging.WriteToConsole($"Epoch timer: {displayString}");
                _client.SetActivityAsync(new Game($"EPOCH: {displayString}", ActivityType.Watching, ActivityProperties.None, string.Empty));

                RecordEpochData();
            }
            catch (Exception ex)
            {
                Logging.WriteToConsole($"Error updating epoch time: {ex}");
            }
        }

        private async Task AsyncVerifyRoles()
        {
            List<string> s = new List<string>() { red, green, zen };

            try
            {
                if (_guilds != null && _guilds.Count > 0)
                {
                    foreach (var guild in _guilds)
                    {
                        IEnumerable<SocketRole> roles = guild.Roles.Where(x => x.Name == green || x.Name == red || x.Name == zen);

                        // Check to see if all 3 roles exist -- if they don't delete the ones that do and readd all 3.
                        if (roles.Count() != s.Count)
                        {
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

        private async Task _client_MessageReceived(SocketMessage arg)
        {
            Logging.WriteToConsole($"Message Received: {arg.Channel} | {arg.Author} | {arg.Content}");
            if (arg.Author.IsBot)
                return;

            Embed embed = _handler.ProcessMessage(arg.Content);

            if (embed != null)
            {
                MessageReference message = new(arg.Id, arg.Channel.Id);
                await arg.Channel.SendMessageAsync(null, false, null, null, null, message, null, null, new Embed[] { embed });
            }
        }

        private async Task _client_GuildUpdated(SocketGuild arg1, SocketGuild arg2)
        {
            Logging.WriteToConsole($"Guild Updated: {arg1.Name} : {arg2.Name}");
            await Task.Run(() => { _guilds = _client.Guilds; });
        }

        private async Task _client_GuildAvailable(SocketGuild arg)
        {
            Logging.WriteToConsole($"Guild Available: {arg.Name}");
            await Task.Run(() => { _guilds = _client.Guilds; });
        }

        private async Task _client_JoinedGuild(SocketGuild arg)
        {
            Logging.WriteToConsole($"Guild Joined: {arg.Name}");
            await Task.Run(() => { _guilds = _client.Guilds; });
        }

        private async Task LoginAndConnect()
        {
            GatewayIntents gatewayIntents = new GatewayIntents();
            gatewayIntents = GatewayIntents.Guilds | GatewayIntents.DirectMessages | GatewayIntents.GuildMessages;

            var _config = new DiscordSocketConfig() { MessageCacheSize = 100, GatewayIntents = gatewayIntents };
            _client = new DiscordSocketClient(_config);
            _client.Log += Log;

            try
            {
                string discordToken = File.ReadAllText("token.txt");
                await _client.LoginAsync(TokenType.Bot, discordToken);
            }
            catch
            {
                Logging.WriteToConsole("Unable to read discord token from token.txt. " +
                    "Please ensure file exists and correct access token is there." +
                    Environment.NewLine + "Exiting in 10seconds.");
                await Task.Delay(10);
                Environment.Exit(0);
            }

            await _client.StartAsync();
        }

        private async Task AsyncGetPrice()
        {
            Logging.WriteToConsole($"Getting price from {_configClass.Provider}");

            int timeToWait = _configClass.TimeToUpdatePrice;
            while (true)
            {
                try
                {
                    if (_client.ConnectionState == ConnectionState.Connected)
                    {
                        string newNick = String.Empty;
                        switch (_configClass.Provider)
                        {
                            case Provider.PCS:
                                newNick = GetPricePCS<PancakeSwapToken>();
                                break;
                            case Provider.Moralis:
                                newNick = GetPriceMoralis();
                                break;
                            case Provider.CMC:
                                newNick = GetBombPriceCMC();
                                break;
                            default:
                                break;
                        }

                        if (_guilds != null && _guilds.Count > 0)
                            foreach (var guild in _guilds)
                            {
                                var user = guild.GetUser(_client.CurrentUser.Id);

                                await user.ModifyAsync(x =>
                                {
                                    x.Nickname = newNick;
                                });
                            }
                        else
                            Logging.WriteToConsole("Bot is not a part of any guilds.");

                        Logging.WriteToConsole(newNick);
                    }
                }
                catch (Exception e)
                {
                    Logging.WriteToConsole(e.ToString());
                }

                await Task.Delay(timeToWait);
            }
        }

        private async Task AsyncGetTWAP()
        {
            Logging.WriteToConsole("Getting TWAP");
            while (true)
            {
                try
                {
                    var twapD = await _moneyOracle.TWAPAsync();

                    Logging.WriteToConsole($"TWAP: {twapD}");
                    await _client.SetActivityAsync(new Game("TWAP: " + twapD, ActivityType.Watching, ActivityProperties.None, null));
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
            Logging.WriteToConsole("Getting Previous Epoch Bomb Price");
            while (true)
            {
                try
                {
                    var consultD = await _moneyTreasury.PreviousEpochBombPriceAsync();

                    if (_client.ConnectionState == ConnectionState.Connected)
                    {
                        string newNick = String.Empty;
                        if (_guilds != null && _guilds.Count > 0)
                        {
                            foreach (var guild in _guilds)
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

                                if (consultD >= (Decimal)1.0 && consultD < (Decimal)1.01)
                                {
                                    if (currentRole != zenU)
                                    {
                                        await guild.CurrentUser.RemoveRoleAsync(currentRole);
                                        await guild.CurrentUser.AddRoleAsync(zenU);
                                    }
                                }
                                else if (consultD >= (Decimal)1.01)
                                {
                                    if (currentRole != greenU)
                                    {
                                        await guild.CurrentUser.RemoveRoleAsync(currentRole);
                                        await guild.CurrentUser.AddRoleAsync(greenU);
                                    }
                                }
                                else if (consultD < (Decimal)1.00)
                                {
                                    if (currentRole != redU)
                                    {
                                        await guild.CurrentUser.RemoveRoleAsync(currentRole);
                                        await guild.CurrentUser.AddRoleAsync(redU);
                                    }
                                }
                            }

                            Logging.WriteToConsole($"PrevBombPrice: {consultD}");
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

        private async void RecordEpochData()
        {
            try
            {
                // Waiting 5 seconds
                await Task.Delay(5000);
                BoardroomDatum newRecord = BoardroomDatum.RecordBoardRoomData(_moneyOracle.GetCurrentEpoch() - 1, _moneyTreasury.PreviousEpochBombPrice(), null);

                if (newRecord != null)
                {
                    if (_guilds != null && _guilds.Count > 0)
                    {
                        foreach (var guild in _guilds)
                        {
                            // follow the AsyncVerifyRoles method but for channels instead.
                            SocketGuildChannel channel = guild.Channels.FirstOrDefault(x => x.Name == boardroomHistory);

                            if (channel != null)
                            {
                                Color c = Color.Gold;
                                var chnl = _client.GetChannel(channel.Id) as IMessageChannel;

                                if (newRecord.TWAP >= (Decimal)1.01)
                                    c = Color.Green;
                                else if (newRecord.TWAP < (Decimal)1)
                                    c = Color.Red;

                                EmbedBuilder embed = new()
                                {
                                    Title = $"Epoch {newRecord.Epoch}"
                                };
                                embed.AddField("TWAP:", newRecord.TWAP, true);
                                embed.Timestamp = newRecord.Created;
                                embed.WithColor(c);
                                //embed.ThumbnailUrl = "https://app.bomb.money/bomb1.png";

                                await chnl.SendMessageAsync(null, false, embed.Build(), null, null, null, null, null, null);
                            }
                            else
                                Console.WriteLine($"Channel '{boardroomHistory}' not found.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.WriteToConsole(e.ToString());
            }
        }

        private async Task AsyncPollCMCData<T>() where T : CMCBomb
        {
            var URL = new UriBuilder("https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest");
            //var URL = new UriBuilder("https://pro-api.coinmarketcap.com/v1/cryptocurrency/map");
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString.Add("id", _configClass.CMCTokenID);
            queryString.Add("convert", "usd");
            URL.Query = queryString.ToString();

            HttpClient cmcClient = new()
            {
                BaseAddress = new Uri(URL.ToString())
            };

            try
            {
                // Add an Accept header for JSON format.
                cmcClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                cmcClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", _configClass.CMCAPIKey);

                string s = null;
                HttpResponseMessage response = cmcClient.GetAsync(s).Result;
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body.

                    string dataObjects = response.Content.ReadAsStringAsync().Result;
                    _cmcBomb = JsonConvert.DeserializeObject<T>(dataObjects);
                }
                else
                {
                    Logging.WriteToConsole(String.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
                }
            }
            catch (Exception ex)
            {
                Logging.WriteToConsole(ex.ToString());
            }
            finally
            {
                cmcClient.Dispose();
            }

            await Task.Delay(_configClass.TimeToUpdatePriceCMC);
        }

        private string GetPricePCS<T>() where T : Token
        {
            HttpClient client = new()
            {
                BaseAddress = new Uri("https://api.pancakeswap.info/api/v2/tokens/" + _configClass.TokenContract)
            };

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            string s = null;
            StringBuilder result = new StringBuilder();

            HttpResponseMessage response = client.GetAsync(s).Result;
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                string dataObjects = response.Content.ReadAsStringAsync().Result;
                T pcsToken = JsonConvert.DeserializeObject<T>(dataObjects);//💣

                string image = _configClass.TokenImage.Length > 0 ? $" {_configClass.TokenImage} " : " ";

                result.Append($"${Decimal.Round(Decimal.Parse(pcsToken.Data.Price), 2)}{image}{pcsToken.Data.Symbol}");
            }
            else
            {
                Logging.WriteToConsole(String.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }

            client.Dispose();

            return result.ToString();
        }

        private string GetPriceMoralis()
        {
            HttpClient moralisClient = new()
            {
                BaseAddress = new Uri("https://deep-index.moralis.io/api/v2/erc20/0x522348779dcb2911539e76a1042aa922f9c47ee3/price?chain=bsc&providerUrl=https%3A%2F%2Fspeedy-nodes-nyc.moralis.io%2F94c4ef9e66d4f133db78b8c1%2Fbsc%2Fmainnet%2F&exchange=0xcA143Ce32Fe78f1f7019d7d551a6402fC5350c73")
            };

            // Add an Accept header for JSON format.
            moralisClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            moralisClient.DefaultRequestHeaders.Add("X-API-Key", _configClass.MoralisAPIKey);

            string s = null;
            StringBuilder result = new StringBuilder();
            HttpResponseMessage response = moralisClient.GetAsync(s).Result;
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.

                string dataObjects = response.Content.ReadAsStringAsync().Result;
                MoralisToken mToken = JsonConvert.DeserializeObject<MoralisToken>(dataObjects);

                Decimal priceAsDouble;
                if (Decimal.TryParse(mToken.usdPrice, out priceAsDouble))
                {
                    string image = _configClass.TokenImage.Length > 0 ? $" {_configClass.TokenImage} " : " ";

                    result.Append($"${Decimal.Round(priceAsDouble, 2)}{image}{_configClass.TokenSymbol}");
                }
                else
                {
                    result.Append("Unable to parse token price.");
                }
            }
            else
            {
                Logging.WriteToConsole(String.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }

            moralisClient.Dispose();

            return result.ToString();
        }

        private string GetBombPriceCMC()
        {
            StringBuilder result = new StringBuilder();

            Decimal price = (Decimal)_cmcBomb.Data.BombInfo.Quote.USD.Price;
            string image = _configClass.TokenImage.Length > 0 ? $" {_configClass.TokenImage} " : " ";

            result.Append($"${Decimal.Round(price, 2)}{image}{_configClass.TokenSymbol}");

            return result.ToString();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
