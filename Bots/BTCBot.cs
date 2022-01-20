using BombMoney.SmartContracts;
using Discord;
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

        public override void Start()
        {
            base.Start();

            Client.MessageReceived += _client_MessageReceived;
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

            return embed;
        }
    }
}
