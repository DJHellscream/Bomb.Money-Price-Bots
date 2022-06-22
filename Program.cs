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
using System.Net.WebSockets;
using BombMoney.Configurations;

namespace BombMoney
{
    internal class Program
    {
        IReadOnlyCollection<SocketGuild> _guilds;
        ConfigurationLoader _configurationLoader;
        TokenConfig _tokenConfig;
        DiscordSocketClient _client;
        SmartContract _oracle;
        SmartContract _treasury;
        xBomb _xBomb;
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            try
            {

                _configurationLoader = JsonConvert.DeserializeObject<ConfigurationLoader>(File.ReadAllText("config.json"));
                string tokenToLoad = _configurationLoader.Config;
                _tokenConfig = JsonConvert.DeserializeObject<TokenConfig>(File.ReadAllText("./Configurations/" + tokenToLoad + ".json"));

                _oracle = ChooseOracle();
                _treasury = ChooseTreasury();

                _xBomb = new xBomb(_tokenConfig.BscScanRPC, _tokenConfig.xBOMBCONTRACT, _tokenConfig.xBOMBABI);

                await LoginAndConnect();

                _client.JoinedGuild += _client_JoinedGuild;
                _client.GuildAvailable += _client_GuildAvailable;
                _client.GuildUpdated += _client_GuildUpdated;
                _client.LeftGuild += _client_LeftGuild;

                _client.Ready += () => { Console.WriteLine("Bot is connected!"); return Task.CompletedTask; };

                Logging.WriteToConsole("Waiting for Connection");
                await Task.Delay(7000).ConfigureAwait(false);

                BotBase bot = ChooseBot(tokenToLoad);

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

        private SmartContract ChooseTreasury()
        {
            string config = _configurationLoader.Config.ToLower();
            SmartContract treasury = null;

            if (config == "bomb"
                || config == "xbomb"
                || config == "busm"
                || config == "btc"
                || config == "bshare"
                || config == "czshare"
                || config == "sbomb"
                || config == "phub")
            {
                treasury = new BombMoneyTreasury(_tokenConfig.BscScanRPC, _tokenConfig.TreasuryContract, _tokenConfig.TreasuryABI);
            }
            else if (config == "czbnb")
            {
                treasury = new CZbnbTreasury(_tokenConfig.BscScanRPC, _tokenConfig.TreasuryContract, _tokenConfig.TreasuryABI);
            }
            else if (config == "czbomb")
            {
                treasury = new CZBombTreasury(_tokenConfig.BscScanRPC, _tokenConfig.TreasuryContract, _tokenConfig.TreasuryABI);
            }
            else if (config == "czemp")
            {
                treasury = new CZEmpTreasury(_tokenConfig.BscScanRPC, _tokenConfig.TreasuryContract, _tokenConfig.TreasuryABI);
            }
            else if (config == "czbusd")
            {
                treasury = new CZBusdTreasury(_tokenConfig.BscScanRPC, _tokenConfig.TreasuryContract, _tokenConfig.TreasuryABI);
            }

            return treasury;
        }

        private SmartContract ChooseOracle()
        {
            string config = _configurationLoader.Config.ToLower();
            SmartContract oracle = null;

            if (config == "bomb"
                || config == "xbomb"
                || config == "busm"
                || config == "btc"
                || config == "bshare"
                || config == "czshare"
                || config == "sbomb"
                || config == "phub")
            {
                oracle = new BombMoneyOracle(_tokenConfig.BscScanRPC, _tokenConfig.OracleContract, _tokenConfig.OracleABI, _tokenConfig.TokenContract);
            }
            else if (config == "czbnb")
            {
                oracle = new CZbnbOracle(_tokenConfig.BscScanRPC, _tokenConfig.OracleContract, _tokenConfig.OracleABI, _tokenConfig.TokenContract);
            }
            else if (config == "czbomb")
            {
                oracle = new CZBombOracle(_tokenConfig.BscScanRPC, _tokenConfig.OracleContract, _tokenConfig.OracleABI, _tokenConfig.TokenContract);
            }
            else if (config == "czemp")
            {
                oracle = new CZEmpOracle(_tokenConfig.BscScanRPC, _tokenConfig.OracleContract, _tokenConfig.OracleABI, _tokenConfig.TokenContract);
            }
            else if (config == "czbusd")
            {
                oracle = new CZBusdOracle(_tokenConfig.BscScanRPC, _tokenConfig.OracleContract, _tokenConfig.OracleABI, _tokenConfig.TokenContract);
            }

            return oracle;
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

        private BotBase ChooseBot(string token)
        {
            string s = token.ToLower();
            BotBase bot = null;

            if (s.Equals("bomb"))
            {
                bot = new BombBot(_tokenConfig, _client, (BombMoneyOracle)_oracle, (BombMoneyTreasury)_treasury, _guilds);
            }
            else if (s.Equals("bshare"))
            {
                bot = new BshareBot(_tokenConfig, _client, (BombMoneyOracle)_oracle, (BombMoneyTreasury)_treasury, _guilds);
            }
            else if (s.Equals("btc"))
            {
                bot = new BTCBot(_tokenConfig, _client, (BombMoneyOracle)_oracle, (BombMoneyTreasury)_treasury, _guilds);
            }
            else if (s.Equals("xbomb"))
            {
                bot = new xBombBot(_tokenConfig, _client, (BombMoneyOracle)_oracle, (BombMoneyTreasury)_treasury, _guilds, _xBomb);
            }
            else if (s.Equals("sbomb"))
            {
                bot = new sbombBot(_tokenConfig, _client, (BombMoneyOracle)_oracle, (BombMoneyTreasury)_treasury, _guilds);
            }
            else if (s.Equals("busm"))
            {
                bot = new BUSMBot(_tokenConfig, _client, (BombMoneyOracle)_oracle, (BombMoneyTreasury)_treasury, _guilds);
            }
            else if (s.Equals("czbomb"))
            {
                bot = new CZBombBot(_tokenConfig, _client, (CZBombOracle)_oracle, (CZBombTreasury)_treasury, _guilds);
            }
            else if (s.Equals("czemp"))
            {
                bot = new CZEmpBot(_tokenConfig, _client, (CZEmpOracle)_oracle, (CZEmpTreasury)_treasury, _guilds);
            }
            else if (s.Equals("czbnb"))
            {
                bot = new CZBnbBot(_tokenConfig, _client, (CZbnbOracle)_oracle, (CZbnbTreasury)_treasury, _guilds);
            }
            else if (s.Equals("czshare"))
            {
                bot = new CZShareBot(_tokenConfig, _client, (BombMoneyOracle)_oracle, (BombMoneyTreasury)_treasury, _guilds);
            }
            else if (s.Equals("czbusd"))
            {
                bot = new CZBusdBot(_tokenConfig, _client, (CZBusdOracle)_oracle, (CZBusdTreasury)_treasury, _guilds);
            }
            else if (s.Equals("phub"))
            {
                bot = new phubbot(_tokenConfig, _client, (BombMoneyOracle)_oracle, (BombMoneyTreasury)_treasury, _guilds);
            }

            return bot;
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
