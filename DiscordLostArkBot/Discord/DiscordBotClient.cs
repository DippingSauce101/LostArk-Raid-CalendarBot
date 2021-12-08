using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordLostArkBot.Constants;
using DiscordLostArkBot.Model.RaidInfo;
using DiscordLostArkBot.Notion;
using DiscordLostArkBot.Service;
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
            //Client.MessageUpdated += test;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        private async Task OnClientMessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> cacheable)
        {
            var channel = await cacheable.GetOrDownloadAsync();
            if (channel == null)
            {
                Console.WriteLine("OnClientMessageDeleted : Retrieving channel failed!!!");
                return;
            }
            var discordRaidInfoKey = new RaidInfo.DiscordKey(channel.Id, message.Id);
            await ServiceHolder.RaidInfo.OnRaidMessageDeleted(discordRaidInfoKey);
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
            Cacheable<IMessageChannel, ulong> cacheable,
            SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot) return;
            if (RaidEmoji.IsCrossEmote(reaction.Emote))
            {
                var channel = await cacheable.GetOrDownloadAsync();
                if (channel == null)
                {
                    Console.WriteLine("OnReactionAdded : Retrieving channel failed!!!");
                    return;
                }
                //Cross Emote 누른 유저가 공대장일 경우에만 메세지 삭제!
                var discordRaidInfoKey = new RaidInfo.DiscordKey(channel.Id, message.Id);
                if (ServiceHolder.RaidInfo.IsUserLeader(discordRaidInfoKey, reaction.UserId))
                {
                    await ServiceHolder.RaidInfo.OnRaidMessageDeleted(discordRaidInfoKey);
                    await channel.DeleteMessageAsync(message.Id);
                }
                else
                {
                    Console.WriteLine($"OnReactionAdded : User {reaction.UserId} is not a leader!");
                }

                return;
            }

            if (RaidEmoji.IsRaidRoleEmote(reaction.Emote))
            {
                var channel = await cacheable.GetOrDownloadAsync();
                if (channel == null)
                {
                    Console.WriteLine("OnReactionAdded : Retrieving channel failed!!!");
                    return;
                }

                var discordRaidInfoKey = new RaidInfo.DiscordKey(channel.Id, reaction.MessageId);
                var targetRole = RaidEmoji.EmojiStringToRole(reaction.Emote.Name);
                if (ServiceHolder.RaidInfo.CanAddPlayer(discordRaidInfoKey, targetRole) == false) return;

                var userMessage = await message.GetUserMessageAsync();
                await ServiceHolder.RaidInfo.RemoveDiscordOldRoleReaction(discordRaidInfoKey, reaction.UserId, userMessage,
                    targetRole);
                ServiceHolder.RaidInfo.AddOrChangePlayerRole(discordRaidInfoKey, reaction.UserId, targetRole);
                await ServiceHolder.RaidInfo.RefreshDiscordRaidMessage(discordRaidInfoKey, userMessage);
                var notionCalendarPageId = ServiceHolder.RaidInfo.GetNotionCalendarPageId(discordRaidInfoKey);
                var notionCalendarPageProperies = ServiceHolder.RaidInfo.GetNotionCalendarPageProperies(discordRaidInfoKey);
                await NotionBotClient.Ins.UpdatePage(notionCalendarPageId, notionCalendarPageProperies);
            }            
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> cacheable,
            SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot) return;
            if (RaidEmoji.IsRaidRoleEmote(reaction.Emote) == false) return;
            var channel = await cacheable.GetOrDownloadAsync();
            if (channel == null)
            {
                Console.WriteLine("OnReactionAdded : Retrieving channel failed!!!");
                return;
            }
            
            var userMessage = await message.GetUserMessageAsync();
            var targetRole = RaidEmoji.EmojiStringToRole(reaction.Emote.Name);

            var discordRaidInfoKey = new RaidInfo.DiscordKey(channel.Id, reaction.MessageId);
            ServiceHolder.RaidInfo.RemovePlayerRole(discordRaidInfoKey, reaction.UserId, targetRole);
            await ServiceHolder.RaidInfo.RefreshDiscordRaidMessage(discordRaidInfoKey, userMessage);
            var notionCalendarPageId = ServiceHolder.RaidInfo.GetNotionCalendarPageId(discordRaidInfoKey);
            var notionCalendarPageProperies = ServiceHolder.RaidInfo.GetNotionCalendarPageProperies(discordRaidInfoKey);
            await NotionBotClient.Ins.UpdatePage(notionCalendarPageId, notionCalendarPageProperies);
        }

        private Task OnClientLogReceived(LogMessage msg)
        {
            Console.WriteLine(msg.ToString()); //로그 출력
            return Task.CompletedTask;
        }
    }
}