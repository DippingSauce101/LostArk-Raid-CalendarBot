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
        bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9' || c == ' ')
                {
                    return false;
                }                   
            }
            
            return true;
        }

        //[Command("4인")]
        //[Summary("4인 레이드 일정 단순화")]
        //public async Task FourRaidSimple(string raidCommandParam)
        //{
        //    string timeStamp_discord = "";
        //    var textArr = raidCommandParam.Split('/');
        //    var textArrLength = textArr.Length;
        //    if (textArrLength > 0)
        //    {
        //        if (IsDigitsOnly(textArr[textArrLength - 1]))
        //        {
        //            timeStamp_discord = textArr[textArrLength - 1];
        //        }
        //    }

        //    //!4인 쿠크노말 4시 /2021 11 25

        //    var kst = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
        //    var kstTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(raidCommandParam.Time), kst);

        //    var raidInfo = RaidInfo.Create(4, RaidInfo.FOUR_RAID_ROLES);
        //    raidInfo.Title = raidCommandParam.Title;
        //    raidInfo.RaidDateTime = kstTime;
        //    //공대장은 이 메세지를 보낸 유저로 자동 셋팅
        //    raidInfo.LeaderDiscordUserId = Context.User.Id;

        //    var titleMessage = raidInfo.GetDiscordTitleMessage();
        //    var eb = raidInfo.GetEmbedBuilder();

        //    var messageSent = await Context.Channel.SendMessageAsync(titleMessage, false, eb.Build());
        //    await messageSent.AddReactionAsync(new Emoji(RaidEmoji.EmojiSwordCrossed));
        //    await messageSent.AddReactionAsync(new Emoji(RaidEmoji.EmojiShield));

        //    Context.Message.dele

        //    raidInfo.DiscordMessageKey = new RaidInfo.DiscordKey(messageSent.Channel.Id, messageSent.Id);
        //    ServiceHolder.RaidInfo.Add(raidInfo);
        //    await NotionBotClient.Ins.CreatePage(raidInfo.DiscordMessageKey, raidInfo.GetNotionPageProperties());
        //}


        /// <summary>
        ///     !4인일정 Title:"쿠크세이튼 노말" Time:"21-11-13 12:00"
        /// </summary>
        [Command("4인")]
        [Summary("4인 레이드 일정 제작")]
        public async Task FourRaid(RaidCommandParameter raidCommandParam)
        {
            var kst = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
            var kstTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(raidCommandParam.시간), kst);

            var raidInfo = RaidInfo.Create(4, Context.Message.Id, RaidInfo.FOUR_RAID_ROLES);
            raidInfo.Title = raidCommandParam.제목;
            raidInfo.RaidDateTime = kstTime;
            //공대장은 이 메세지를 보낸 유저로 자동 셋팅
            raidInfo.LeaderDiscordUserId = Context.User.Id;

            await Context.Message.DeleteAsync();

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
        ///     !8인일정 Title:"비아키스 하드" Time:"21-11-13 12:00"
        /// </summary>
        [Command("8인")]
        [Summary("8인 레이드 일정 제작")]
        public async Task EightRaid(RaidCommandParameter raidCommandParam)
        {
            var kst = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
            var kstTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(raidCommandParam.시간), kst);

            var raidInfo = RaidInfo.Create(4, Context.Message.Id, RaidInfo.EIGHT_RAID_ROLES);
            raidInfo.Title = raidCommandParam.제목;
            raidInfo.RaidDateTime = kstTime;
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