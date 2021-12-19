using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BombPriceBot
{
    internal class Program
    {
        AConfigurationClass _configClass;
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

            Console.WriteLine("Getting price");

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.pancakeswap.info/api/v2/tokens/" + _configClass.TokenContract);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            System.Collections.Generic.IReadOnlyCollection<SocketGuild> guilds = _client.Guilds;

            string newNick = "";
            while (true)
            {
                try
                {
                    foreach (var guild in guilds)
                    {
                        var user = guild.GetUser(_client.CurrentUser.Id);
                        await user.ModifyAsync(x =>
                        {
                            newNick = GetPricePCS(client);
                            x.Nickname = newNick;
                        });
                    }

                    await Task.Delay(15000);
                }
                catch (Exception e)
                {
                    Console.Write(e.ToString());
                    break;
                }
            }

            client.Dispose();
        }

        private async Task _client_GuildAvailable(SocketGuild arg)
        {
            await Task.Run(() => { gotGuild = true; });
        }

        private string GetPricePCS(HttpClient httpClient)
        {
            string s = null;
            StringBuilder result = new StringBuilder();
            HttpClient client = httpClient;

            HttpResponseMessage response = client.GetAsync(s).Result;
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                string dataObjects = response.Content.ReadAsStringAsync().Result;
                PancakeSwapToken bombToken = JsonConvert.DeserializeObject<PancakeSwapToken>(dataObjects);
                result.Append("$" + Decimal.Round(Decimal.Parse(bombToken.Data.Price), 2) + " 💣 " + bombToken.Data.Symbol);
                Console.WriteLine(result.ToString());
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return result.ToString();
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
