using BombMoney.SmartContracts;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombMoney.Bots
{
    internal class BTCBot : BotBase
    {
        public BTCBot(AConfigurationClass config, DiscordSocketClient client, BombMoneyOracle moneyOracle, BombMoneyTreasury moneyTreasury, IReadOnlyCollection<SocketGuild> socketGuilds) : base(config, client, moneyOracle, moneyTreasury, socketGuilds)
        {
        }
    }
}
