using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordLostArkBot.Constants;
using DiscordLostArkBot.Model.RaidInfo;
using DiscordLostArkBot.Notion;
using DiscordLostArkBot.Service;

namespace DiscordLostArkBot.Discord
{
    [NamedArgumentType]
    public class RaidCommandParameter
    {
        public string Title { get; set; }

        /// <summary>
        ///     YY-MM-DD HH:SS
        ///     ex) 21-11-13 12:00
        ///     DateTime으로 받아버리면 한국시간으로 입력해도 UTC Time으로 해석해버림. 일단 string으로 받고 Parse하도록 수정
        /// </summary>
        public string Time { get; set; }
    }

    public class RaidSchedulerModule : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        ///     !4인 "쿠크세이튼 노말" "21-11-13 12:00"
        /// </summary>
        [Command("4인")]
        [Summary("4인 레이드 일정 제작")]
        public async Task FourRaid(RaidCommandParameter raidCommandParam)
        {
            var kst = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
            var kstTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(raidCommandParam.Time), kst);

            var raidInfo = RaidInfo.Create(4, RaidInfo.FOUR_RAID_ROLES);
            raidInfo.Title = raidCommandParam.Title;
            raidInfo.DateTime = kstTime;
            //공대장은 이 메세지를 보낸 유저로 자동 셋팅
            raidInfo.LeaderDiscordUserId = Context.User.Id;

            var titleMessage = raidInfo.GetDiscordTitleMessage();
            var eb = raidInfo.GetEmbedBuilder();
            var messageSent = await Context.Channel.SendMessageAsync(titleMessage, false, eb.Build());
            await messageSent.AddReactionAsync(new Emoji(RaidEmoji.EmojiSwordCrossed));
            await messageSent.AddReactionAsync(new Emoji(RaidEmoji.EmojiShield));

            raidInfo.DiscordMessageKey = new RaidInfo.DiscordKey(messageSent.Channel.Id, messageSent.Id);
            ServiceHolder.RaidInfo.Add(raidInfo);
            await NotionBotClient.Ins.CreatePage(raidInfo.DiscordMessageKey, raidInfo.GetNotionPageProperties());
        }

        /// <summary>
        ///     !8인 "비아키스 하드" "21-11-13 12:00"
        /// </summary>
        [Command("8인")]
        [Summary("8인 레이드 일정 제작")]
        public async Task EightRaid(RaidCommandParameter raidCommandParam)
        {
            var kst = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
            var kstTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(raidCommandParam.Time), kst);

            var raidInfo = RaidInfo.Create(8, RaidInfo.EIGHT_RAID_ROLES);
            raidInfo.Title = raidCommandParam.Title;
            raidInfo.DateTime = kstTime;
            //공대장은 이 메세지를 보낸 유저로 자동 셋팅
            raidInfo.LeaderDiscordUserId = Context.User.Id;

            var titleMessage = raidInfo.GetDiscordTitleMessage();
            var eb = raidInfo.GetEmbedBuilder();
            var messageSent = await Context.Channel.SendMessageAsync(titleMessage, false, eb.Build());
            await messageSent.AddReactionAsync(new Emoji(RaidEmoji.EmojiSwordCrossed));
            await messageSent.AddReactionAsync(new Emoji(RaidEmoji.EmojiShield));

            raidInfo.DiscordMessageKey = new RaidInfo.DiscordKey(messageSent.Channel.Id, messageSent.Id);
            ServiceHolder.RaidInfo.Add(raidInfo);
            await NotionBotClient.Ins.CreatePage(raidInfo.DiscordMessageKey, raidInfo.GetNotionPageProperties());
        }
    }
}