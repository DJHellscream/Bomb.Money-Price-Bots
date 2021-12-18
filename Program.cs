using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using CoinGecko;
using CoinGecko.ApiEndPoints;
using CoinGecko.Interfaces;
using CoinGecko.Clients;
using System.IO;
using BscScanner;
using Newtonsoft.Json;
using Nethereum.Web3;
using System.Numerics;

namespace BombPriceBot
{
    internal class Program
    {
        AConfigurationClass _configClass;
        BscScanClient _bscClient;
        DiscordSocketClient _client;
        bool gotGuild = false;

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            try
            {
                var _config = new DiscordSocketConfig() { MessageCacheSize = 100 };
                _client = new DiscordSocketClient(_config);
                _client.Log += Log;

                _configClass = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json"));

                var web3BSC = new Web3("https://bsc-dataseed1.binance.org:443");
                var contract = web3BSC.Eth.GetContract(_configClass.BombABI, _configClass.BombContract);
                var function = contract.GetFunction("totalSupply");

                var res = await function.CallAsync<BigInteger>();

                _bscClient = new BscScanClient(_configClass.BscScanAPIKey);
                await _client.LoginAsync(TokenType.Bot, _configClass.DiscordToken);
                await _client.StartAsync();

                _client.MessageReceived += MessageReceived;
                _client.GuildAvailable += _client_GuildAvailable;

                _client.Ready += () => { Console.WriteLine("Bot is connected!"); return Task.CompletedTask; };
                await Task.Delay(5000);

                await AsyncGetPrice();

                // Block this task until the program is closed.
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }

        private async Task AsyncGetPrice()
        {
            if (!gotGuild)
            {
                Console.WriteLine("Guild available was not received, can't change nickname.");
                return;
            }

            Console.WriteLine("Getting Bomb price");

            ICoinGeckoClient geckoClient = CoinGeckoClient.Instance;
            System.Collections.Generic.IReadOnlyCollection<SocketGuild> wut = _client.Guilds;

            while (true)
            {
                //var newNick = await geckoClient.SimpleClient.GetSimplePrice(new string[] { "bomb-money" }, new string[] { "usd" });
                double balance = await _bscClient.GetTokenCirculatingSupply(_configClass.BombContract);

                foreach (var guild in wut)
                {
                    var user = guild.GetUser(_client.CurrentUser.Id);
                    await user.ModifyAsync(x =>
                    {
                        double test = BscScanner.Extensions.Convert.BscConvert.GweiToBnb(balance);
                        //x.Nickname = "$" + newNick["bomb-money"]["usd"]?.ToString() + " 💣 BOMB";
                        x.Nickname = balance.ToHumanReadable(4);
                    });
                }

                //Console.WriteLine("Bomb price updated to: " + newNick["bomb-money"]["usd"]?.ToString());
                Console.WriteLine("Total Supply updated");
                await Task.Delay(15000);
            }
        }

        private async Task _client_GuildAvailable(SocketGuild arg)
        {
            await Task.Run(() => { gotGuild = true; });
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            if (arg.Author.IsBot)
                return;

            if (arg.Content.ToLower() == "ping")
            {
                MessageReference message = new(arg.Id, arg.Channel.Id);
                await arg.Channel.SendMessageAsync("Pong", false, null, null, null, message);
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
