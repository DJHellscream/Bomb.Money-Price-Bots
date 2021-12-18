using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using CoinGecko;
using CoinGecko.ApiEndPoints;
using CoinGecko.Interfaces;
using CoinGecko.Clients;
using System.IO;

namespace BombPriceBot
{
    internal class Program
    {
        DiscordSocketClient _client;
        bool gotGuild = false;

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var _config = new DiscordSocketConfig() { MessageCacheSize = 100 };
            _client = new DiscordSocketClient(_config);

            _client.Log += Log;

            //  You can assign your bot token to a string, and pass that in to connect.
            //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.

            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            var token = File.ReadAllText("token.txt");
            // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.MessageReceived += MessageReceived;
            _client.GuildAvailable += _client_GuildAvailable;

            _client.Ready += () => { Console.WriteLine("Bot is connected!"); return Task.CompletedTask; };
            await Task.Delay(5000);

            await AsyncGetPrice();

            // Block this task until the program is closed.
            await Task.Delay(-1);
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
                var newNick = await geckoClient.SimpleClient.GetSimplePrice(new string[] { "bomb-money" }, new string[] { "usd" });

                foreach (var guild in wut)
                {
                    var user = guild.GetUser(_client.CurrentUser.Id);
                    await user.ModifyAsync(x =>
                    {
                        x.Nickname = "$" + newNick["bomb-money"]["usd"]?.ToString() + " 💣 BOMB";
                    });
                }

                Console.WriteLine("Bomb price updated to: " + newNick["bomb-money"]["usd"]?.ToString());
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
