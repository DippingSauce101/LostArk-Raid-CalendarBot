using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordLostArkBot.Data;
using DiscordLostArkBot.Notion;
using DiscordLostArkBot.Setting;
using DiscordLostArkBot.Utilities;

namespace DiscordLostArkBot.Discord
{
    public class DiscordBotClient : Singleton<DiscordBotClient>
    {
        private readonly CommandService _commands;

        public DiscordBotClient()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            _commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose
            });

            Client.Log += OnClientLogReceived;
            _commands.Log += OnClientLogReceived;
        }

        public DiscordSocketClient Client { get; }

        public async Task RunClient()
        {
            await Client.LoginAsync(TokenType.Bot, Settings.DiscordBotToken);
            await Client.StartAsync();

            Client.MessageReceived += OnClientMessage;
            Client.MessageDeleted += OnClientMessageDeleted;
            Client.ReactionAdded += OnReactionAdded;
            Client.ReactionRemoved += OnReactionRemoved;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            await Task.Delay(-1);
        }

        private async Task OnClientMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            var raidInfo = DB.Ins.GetRaidInfo(channel.Id, message.Id);
            if (raidInfo == null)
            {
                Console.WriteLine($"Raid info not found for deleted message: {message.Id}");
                return;
            }

            await NotionBotClient.Ins.DeletePage(raidInfo);
        }

        private async Task OnClientMessage(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            var argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('!', ref argPos) ||
                  message.HasMentionPrefix(Client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(Client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(
                context,
                argPos,
                null);
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message,
            ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot) return;
            if (RaidEmoji.IsRaidRoleEmote(reaction.Emote) == false) return;
            Console.WriteLine($"OnReactionAdded({reaction.Emote.Name})");

            var raidInfo = DB.Ins.GetRaidInfo(channel.Id, reaction.MessageId);
            if (raidInfo == null)
            {
                Console.WriteLine("Raid info not found!");
                return;
            }

            var targetRole = RaidEmoji.EmojiStringToRole(reaction.Emote.Name);
            if (raidInfo.IsRoleFull(targetRole)) return;
            var userMessage = await message.GetUserMessageAsync();
            if (raidInfo.UserAlreadySeated(reaction.UserId))
            {
                var currentRole = raidInfo.GetUserRole(reaction.UserId);
                if (targetRole == currentRole)
                    //이유는 모르겠지만 같은 이모지가 두번 추가되고 있네? 그냥 리턴
                    return;

                var emojiToRemove = currentRole == RaidInfo.RaidPlayer.Role.Deal
                    ? new Emoji(RaidEmoji.EmojiSwordCrossed)
                    : new Emoji(RaidEmoji.EmojiShield);
                await userMessage.RemoveReactionAsync(emojiToRemove, reaction.UserId);
            }

            raidInfo.AddOrChangePlayerRole(reaction.UserId, targetRole);
            await RefreshRaidMessage(raidInfo, userMessage);
            await NotionBotClient.Ins.UpdatePage(raidInfo);
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message,
            ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot) return;
            if (RaidEmoji.IsRaidRoleEmote(reaction.Emote) == false) return;
            Console.WriteLine($"OnReactionRemoved({reaction.Emote.Name})");

            var raidInfo = DB.Ins.GetRaidInfo(channel.Id, reaction.MessageId);
            if (raidInfo == null)
            {
                Console.WriteLine("Raid info not found!");
                return;
            }

            var userMessage = await message.GetUserMessageAsync();
            var targetRole = RaidEmoji.EmojiStringToRole(reaction.Emote.Name);
            raidInfo.RemovePlayerRole(reaction.UserId, targetRole);
            await RefreshRaidMessage(raidInfo, userMessage);
            await NotionBotClient.Ins.UpdatePage(raidInfo);
        }

        private async Task RefreshRaidMessage(RaidInfo raidInfo, IUserMessage userMessage)
        {
            await userMessage.ModifyAsync(x =>
            {
                var eb = raidInfo.GetEmbedBuilder();
                x.Embed = eb.Build();
            });
        }

        private Task OnClientLogReceived(LogMessage msg)
        {
            Console.WriteLine(msg.ToString()); //로그 출력
            return Task.CompletedTask;
        }
    }
}