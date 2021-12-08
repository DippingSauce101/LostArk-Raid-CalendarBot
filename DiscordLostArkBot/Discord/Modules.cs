using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using DiscordLostArkBot.Constants;
using DiscordLostArkBot.Model.RaidInfo;
using DiscordLostArkBot.Notion;
using DiscordLostArkBot.Service;

namespace DiscordLostArkBot.Discord
{
    [NamedArgumentType]
    public class RaidCommandParameter
    {
        public string 제목 { get; set; }

        /// <summary>
        ///     YY-MM-DD HH:SS
        ///     ex) 21-11-13 12:00
        ///     DateTime으로 받아버리면 한국시간으로 입력해도 UTC Time으로 해석해버림. 일단 string으로 받고 Parse하도록 수정
        /// </summary>
        public string 시간 { get; set; }
    }

    public class RaidSchedulerModule : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// !4인 제목:"쿠크세이튼 노말" 시간:"21/10/13"
        /// !4인 제목:"쿠크세이튼 노말" 시간:"21/10/13 13:00"
        /// </summary>
        [Command("4인")]
        [Summary("4인 레이드 일정 제작")]
        public async Task FourRaid(RaidCommandParameter raidCommandParam)
        {
            await AddRaid(raidCommandParam, RaidInfo.FOUR_RAID_ROLES);
        }

        /// <summary>
        ///     !8인일정 Title:"비아키스 하드" Time:"21-11-13 12:00"
        /// </summary>
        [Command("8인")]
        [Summary("8인 레이드 일정 제작")]
        public async Task EightRaid(RaidCommandParameter raidCommandParam)
        {
            await AddRaid(raidCommandParam, RaidInfo.EIGHT_RAID_ROLES);
        }

        public async Task AddRaid(RaidCommandParameter raidCommandParam, RaidInfo.RaidPlayer.Role[] roles)
        {
            var kst = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
            var kstTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(raidCommandParam.시간), kst);

            var raidInfo = RaidInfo.Create(roles.Length, Context.Message.Id, roles);
            raidInfo.Title = raidCommandParam.제목;
            raidInfo.RaidDateTime = kstTime;

            //공대장은 이 메세지를 보낸 유저로 자동 셋팅
            raidInfo.LeaderDiscordUserId = Context.User.Id;

            //명령어 메세지 삭제
            await Context.Message.DeleteAsync();

            var titleMessage = raidInfo.GetDiscordTitleMessage();
            var eb = raidInfo.GetEmbedBuilder();
            var cb = raidInfo.GetComponentBuilder();
            var messageSent = await Context.Channel.SendMessageAsync(titleMessage, false, eb.Build(), component: cb.Build());
            await messageSent.AddReactionAsync(new Emoji(RaidEmoji.EmojiSwordCrossed));
            await messageSent.AddReactionAsync(new Emoji(RaidEmoji.EmojiShield));
            await messageSent.AddReactionAsync(new Emoji(RaidEmoji.EmojiCross));

            //메세지 스레드 생성
            await CreateThread(raidInfo, messageSent);

            raidInfo.DiscordMessageKey = new RaidInfo.DiscordKey(messageSent.Channel.Id, messageSent.Id);
            ServiceHolder.RaidInfo.Add(raidInfo);
            await NotionBotClient.Ins.CreatePage(raidInfo.DiscordMessageKey, raidInfo.GetNotionPageProperties());
        }

        private async Task CreateThread(RaidInfo raidInfo, RestUserMessage messageSent)
        {
            if (messageSent.Channel is ITextChannel)
            {
                var textChannel = (messageSent.Channel as ITextChannel);
                try
                {
                    var messageThread = await textChannel.CreateThreadAsync(raidInfo.Title, ThreadType.PublicThread,
                        ThreadArchiveDuration.OneDay,
                        messageSent);
                    raidInfo.DiscordMessageThreadId = messageThread.Id;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                
            }
        }
    }
}