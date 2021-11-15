using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordLostArkBot.Data;
using DiscordLostArkBot.Notion;

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

            var raidInfo = new FourRaidInfo();
            raidInfo.Title = raidCommandParam.Title;
            raidInfo.DateTime = kstTime;

            var eb = raidInfo.GetEmbedBuilder();
            var messageSent = await Context.Channel.SendMessageAsync("", false, eb.Build());
            await messageSent.AddReactionAsync(new Emoji(RaidEmoji.EmojiSwordCrossed));
            await messageSent.AddReactionAsync(new Emoji(RaidEmoji.EmojiShield));

            raidInfo.ChannelId = messageSent.Channel.Id;
            raidInfo.MessageId = messageSent.Id;
            DB.Ins.AddRaidInfo(raidInfo);
            await NotionBotClient.Ins.CreatePage(raidInfo);
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
            
            var raidInfo = new EightRaidInfo();
            raidInfo.Title = raidCommandParam.Title;
            //유저가 입력한 DateTime은 UTC Time으로 입력이 들어오는듯.
            //한국 시간으로 입력->UTC 타임
            raidInfo.DateTime = kstTime;

            var eb = raidInfo.GetEmbedBuilder();
            var messageSent = await Context.Channel.SendMessageAsync("", false, eb.Build());
            await messageSent.AddReactionAsync(new Emoji(RaidEmoji.EmojiSwordCrossed));
            await messageSent.AddReactionAsync(new Emoji(RaidEmoji.EmojiShield));

            raidInfo.ChannelId = messageSent.Channel.Id;
            raidInfo.MessageId = messageSent.Id;
            DB.Ins.AddRaidInfo(raidInfo);
            await NotionBotClient.Ins.CreatePage(raidInfo);
        }
    }
}