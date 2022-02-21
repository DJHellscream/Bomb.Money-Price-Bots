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

namespace BombMoney.Bots
{
    public abstract class BotBase
    {
        CMCBomb _cmcBomb;
        public AConfigurationClass Config { get; set; }
        public DiscordSocketClient Client { get; set; }
        public BombMoneyOracle MoneyOracle { get; set; }
        public BombMoneyTreasury MoneyTreasury { get; set; }
        public CMCBomb CMCBomb
        {
            get { return _cmcBomb; }
            set
            {
                _cmcBomb = value;
                MessageHandler.CMCBomb = value;
            }
        }
        public IReadOnlyCollection<SocketGuild> SocketGuilds { get; set; }
        public MessageHandler MessageHandler { get; set; }

        public BotBase(AConfigurationClass config,
            DiscordSocketClient client,
            BombMoneyOracle moneyOracle,
            BombMoneyTreasury moneyTreasury,
            IReadOnlyCollection<SocketGuild> socketGuilds)
        {
            Config = config;
            Client = client;
            MoneyOracle = moneyOracle;
            MoneyTreasury = moneyTreasury;
            SocketGuilds = socketGuilds;
            MessageHandler = new MessageHandler(CMCBomb, MoneyTreasury);
        }

        public virtual void Start()
        {
            _ = AsyncGetPrice();
        }

        public virtual Embed ProcessMessage(SocketMessage arg, out bool authorIsBot)
        {
            authorIsBot = false;
            Embed embed = null;
            string author = arg.Author.Username + "#" + arg.Author.Discriminator;
            Logging.WriteToConsole($"Message Received: {arg.Channel} | {author} | {arg.Content}");

            if (arg.Author.IsBot)
            {
                authorIsBot = true;
                return null;
            }
            else if (arg.Content.StartsWith("!cp"))
            {
                if (author == "bifkn#5627")
                {
                    try
                    {
                        string newProvider = arg.Content.Split(" ")[1];
                        string prevProvider = Enum.GetName<Provider>(Config.Provider);
                        Config.Provider = Enum.Parse<Provider>(newProvider);
                        Logging.WriteToConsole($"Updating Provider from {prevProvider} to {newProvider}");
                    }
                    catch
                    {
                        EmbedBuilder embedBuilder = new EmbedBuilder();
                        embedBuilder.Title = "You typed it wrong, dummy <:doxxedbifkn:928415925483487272>";
                        embed = embedBuilder.Build();
                    }
                }
                else
                {
                    EmbedBuilder embedBuilder = new EmbedBuilder();
                    embedBuilder.Title = "You are not the bot lord. Imposters will be destroyed! <a:Bombnuke:928267878296338523>";

                    embed = embedBuilder.Build();
                }
            }

            return embed;
        }

        private async Task AsyncGetPrice()
        {
            Logging.WriteToConsole($"Getting price from {Config.Provider}");

            int timeToWait = Config.TimeToUpdatePrice;
            while (true)
            {
                try
                {
                    if (Client.ConnectionState == ConnectionState.Connected)
                    {
                        string newNick = string.Empty;
                        switch (Config.Provider)
                        {
                            case Provider.PCS:
                                newNick = GetPricePCS<PancakeSwapToken>();
                                break;
                            case Provider.MRS:
                                newNick = GetPriceMoralis();
                                break;
                            case Provider.CMC:
                                newNick = GetBombPriceCMC();
                                break;
                            default:
                                break;
                        }

                        if (SocketGuilds != null && SocketGuilds.Count > 0)
                            foreach (var guild in SocketGuilds)
                            {
                                var user = guild.GetUser(Client.CurrentUser.Id);

                                await user.ModifyAsync(x =>
                                {
                                    x.Nickname = newNick;
                                });
                            }
                        else
                            Logging.WriteToConsole("Bot is not a part of any guilds.");

                        Logging.WriteToConsole($"{newNick} - {Config.Provider}");
                    }
                }
                catch (Exception e)
                {
                    Logging.WriteToConsole(e.ToString());
                }

                await Task.Delay(timeToWait);
            }
        }

        private string GetPricePCS<T>() where T : Token
        {
            HttpClient client = new()
            {
                BaseAddress = new Uri("https://api.pancakeswap.info/api/v2/tokens/" + Config.TokenContract)
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

                string image = Config.TokenImage.Length > 0 ? $" {Config.TokenImage} " : " ";

                result.Append($"${decimal.Round(decimal.Parse(pcsToken.Data.Price), 2)}{image}{pcsToken.Data.Symbol}");
            }
            else
            {
                Logging.WriteToConsole(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }

            client.Dispose();

            return result.ToString();
        }

        private string GetPriceMoralis()
        {
            HttpClient moralisClient = new()
            {
                BaseAddress = new Uri($"https://deep-index.moralis.io/api/v2/erc20/{Config.TokenContract}/price?chain=bsc&providerUrl=https%3A%2F%2Fspeedy-nodes-nyc.moralis.io%2F94c4ef9e66d4f133db78b8c1%2Fbsc%2Fmainnet%2F&exchange=0xcA143Ce32Fe78f1f7019d7d551a6402fC5350c73")
            };

            // Add an Accept header for JSON format.
            moralisClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            moralisClient.DefaultRequestHeaders.Add("X-API-Key", Config.MoralisAPIKey);

            string s = null;
            StringBuilder result = new StringBuilder();
            HttpResponseMessage response = moralisClient.GetAsync(s).Result;
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.

                string dataObjects = response.Content.ReadAsStringAsync().Result;
                MoralisToken mToken = JsonConvert.DeserializeObject<MoralisToken>(dataObjects);

                decimal priceAsDouble;
                if (decimal.TryParse(mToken.usdPrice, out priceAsDouble))
                {
                    string image = Config.TokenImage.Length > 0 ? $" {Config.TokenImage} " : " ";

                    result.Append($"${decimal.Round(priceAsDouble, 2)}{image}{Config.TokenSymbol}");
                }
                else
                {
                    result.Append("Unable to parse token price.");
                }
            }
            else
            {
                Logging.WriteToConsole(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }

            moralisClient.Dispose();

            return result.ToString();
        }

        private string GetBombPriceCMC()
        {
            StringBuilder result = new StringBuilder();

            decimal price = (decimal)CMCBomb.Data.BombInfo.Quote.USD.Price;
            string image = Config.TokenImage.Length > 0 ? $" {Config.TokenImage} " : " ";

            result.Append($"${decimal.Round(price, 2)}{image}{Config.TokenSymbol}");

            return result.ToString();
        }
    }
}
