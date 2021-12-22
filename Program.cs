using Discord;
using Discord.WebSocket;
using Nethereum.Web3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BombPriceBot
{
    internal class Program
    {
        IReadOnlyCollection<SocketGuild> _guilds;
        AConfigurationClass _configClass;
        DiscordSocketClient _client;
        BombMoneyOracle _moneyOracle;
        BombMoneyTreasury _moneyTreasury;
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
                //_client.MessageReceived += _client_MessageReceived;

                _client.Ready += () => { Console.WriteLine("Bot is connected!"); return Task.CompletedTask; };
                await Task.Delay(3000);

                _ = AsyncGetPrice();
                if (_configClass.TokenSymbol.Equals("BOMB"))
                    _ = AsyncGetTWAP();

                // Block this task until the program is closed.
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                WriteToConsole(ex.ToString());
            }
        }

        //private async Task _client_MessageReceived(SocketMessage arg)
        //{
        //    WriteToConsole("Message Received. " + arg.Content);
        //    if (arg.Author.IsBot)
        //        return;

        //    if (arg.Content.StartsWith('?'))
        //    {
        //        EmbedBuilder embed = new EmbedBuilder();
        //        embed.AddField("Symbol", "BOMB", true);
        //        embed.ImageUrl = "https://app.bomb.money/bomb1.png";

        //        MessageReference message = new(arg.Id, arg.Channel.Id);
        //        await arg.Channel.SendMessageAsync(null, false, null, null, null, message, null, null, new Embed[] { embed.Build() });
        //    }
        //}j

        private async Task _client_GuildUpdated(SocketGuild arg1, SocketGuild arg2)
        {
            WriteToConsole("Guild Updated.");
            await Task.Run(() => { _guilds = _client.Guilds; });
        }

        private async Task _client_GuildAvailable(SocketGuild arg)
        {
            WriteToConsole("Guild Available.");
            await Task.Run(() => { _guilds = _client.Guilds; });
        }

        private async Task _client_JoinedGuild(SocketGuild arg)
        {
            WriteToConsole("Guild Joined.");
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
                WriteToConsole("Unable to read discord token from token.txt. " +
                    "Please ensure file exists and correct access token is there." +
                    Environment.NewLine + "Exiting in 10seconds.");
                await Task.Delay(10);
                Environment.Exit(0);
            }

            await _client.StartAsync();
        }

        private async Task AsyncGetPrice()
        {
            WriteToConsole($"Getting price from {_configClass.Provider}");

            int timeToWait = _configClass.TimeToUpdatePrice;

            string newNick = "";
            while (true)
            {
                try
                {
                    if (_client.ConnectionState == ConnectionState.Connected)
                    {
                        if (_guilds != null && _guilds.Count > 0)
                            foreach (var guild in _guilds)
                            {
                                var user = guild.GetUser(_client.CurrentUser.Id);
                                switch (_configClass.Provider)
                                {
                                    case Provider.PCS:
                                        newNick = GetPricePCS<PancakeSwapToken>();
                                        break;
                                    case Provider.Moralis:
                                        newNick = GetPriceMoralis();
                                        break;
                                    case Provider.CMC:
                                        newNick = GetPriceCMC();
                                        timeToWait = _configClass.TimeToUpdatePriceCMC;
                                        break;
                                    default:
                                        break;
                                }
                                await user.ModifyAsync(x =>
                                {
                                    x.Nickname = newNick;
                                });
                            }
                        else
                            WriteToConsole("Bot is not a part of any guilds.");
                    }
                }
                catch (Exception e)
                {
                    WriteToConsole(e.ToString());
                }

                await Task.Delay(timeToWait);
            }
        }

        private async Task AsyncGetTWAP()
        {
            WriteToConsole("Getting TWAP");
            while (true)
            {
                try
                {
                    var twap = await _moneyOracle.TWAPAsync();

                    string twapString = twap.ToString().PadLeft(18, '0');
                    Decimal twapD = Decimal.Round(Decimal.Parse(twapString.Insert(4, ".")), 4);

                    WriteToConsole("TWAP: " + twapD);
                    await _client.SetActivityAsync(new Game("TWAP: " + twapD, ActivityType.Watching, ActivityProperties.None, null));
                }
                catch (Exception e)
                {
                    WriteToConsole(e.ToString());
                }

                //Pause for 10seconds
                await Task.Delay(10000);
            }
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
                WriteToConsole(result.ToString());
            }
            else
            {
                WriteToConsole(String.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
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
                    WriteToConsole(result.ToString());
                }
                else
                {
                    result.Append("Unable to parse token price.");
                }
            }
            else
            {
                WriteToConsole(String.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }

            moralisClient.Dispose();

            return result.ToString();
        }

        private string GetPriceCMC()
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

            // Add an Accept header for JSON format.
            cmcClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            cmcClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", _configClass.CMCAPIKey);

            string s = null;
            StringBuilder result = new StringBuilder();
            HttpResponseMessage response = cmcClient.GetAsync(s).Result;
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.

                string dataObjects = response.Content.ReadAsStringAsync().Result;
                CMCQuotesLatest mToken = JsonConvert.DeserializeObject<CMCQuotesLatest>(dataObjects);

                Decimal price = (Decimal)mToken.Data._15876.Quote.USD.Price;
                string image = _configClass.TokenImage.Length > 0 ? $" {_configClass.TokenImage} " : " ";

                result.Append($"${Decimal.Round(price, 2)}{image}{_configClass.TokenSymbol}");
                WriteToConsole(result.ToString());
            }
            else
            {
                WriteToConsole(String.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }

            cmcClient.Dispose();

            return result.ToString();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private void WriteToConsole(String message)
        {
            Console.WriteLine(message);
        }
    }
}
