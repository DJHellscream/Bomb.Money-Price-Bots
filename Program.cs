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
using System.Timers;
using System.Diagnostics;
using BombMoney.Bots;
using BombMoney.ResponseObjects;
using BombMoney.SmartContracts;
using BombMoney.Database;

namespace BombMoney
{
    internal class Program
    {
        IReadOnlyCollection<SocketGuild> _guilds;
        AConfigurationClass _configClass;
        DiscordSocketClient _client;
        BombMoneyOracle _moneyOracle;
        BombMoneyTreasury _moneyTreasury;
        xBomb _xBomb;
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            try
            {
                _configClass = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json"));
                _moneyOracle = new BombMoneyOracle(_configClass.BscScanRPC, _configClass.OracleContract, _configClass.OracleABI, _configClass.TokenContract);
                _moneyTreasury = new BombMoneyTreasury(_configClass.BscScanRPC, _configClass.TreasuryContract, _configClass.TreasuryABI);
                _xBomb = new xBomb(_configClass.BscScanRPC, _configClass.xBOMBCONTRACT, _configClass.xBOMBABI);

                await LoginAndConnect();

                _client.JoinedGuild += _client_JoinedGuild;
                _client.GuildAvailable += _client_GuildAvailable;
                _client.GuildUpdated += _client_GuildUpdated;
                _client.LeftGuild += _client_LeftGuild;
                
                _client.Ready += () => { Console.WriteLine("Bot is connected!"); return Task.CompletedTask; };

                Logging.WriteToConsole("Waiting for Connection");
                await Task.Delay(7000).ConfigureAwait(false);

                BotBase bot = null;

                if (_configClass.TokenSymbol.Equals("BOMB"))
                {
                    bot = new BombBot(_configClass, _client, _moneyOracle, _moneyTreasury, _guilds);
                }
                else if (_configClass.TokenSymbol.Equals("BSHARE"))
                {
                    bot = new BshareBot(_configClass, _client, _moneyOracle, _moneyTreasury, _guilds);
                }
                else if (_configClass.TokenSymbol.Equals("BTC"))
                {
                    bot = new BTCBot(_configClass, _client, _moneyOracle, _moneyTreasury, _guilds);
                }
                else if(_configClass.TokenSymbol.Equals("xBOMB"))
                {
                    bot = new xBombBot(_configClass, _client, _moneyOracle, _moneyTreasury, _guilds, _xBomb);
                }

                bot.Start();

                // Block this task until the program is closed.
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Logging.WriteToConsole(ex.ToString());
            }

            _client.Dispose();
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

        private async Task _client_LeftGuild(SocketGuild arg)
        {
            Logging.WriteToConsole("Guild Updated.");
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

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
