﻿using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using DiscordLostArkBot.Constants;
using DiscordLostArkBot.Model.RaidInfo;
using DiscordLostArkBot.Notion;
using DiscordLostArkBot.Service;
using DiscordLostArkBot.Utilities;
using Notion.Client;

namespace DiscordLostArkBot.Discord
{
    public class RaidSchedulerModule : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// 디스코드 스트링을 직접 파싱한다.
        /// </summary>
        private struct RaidCommandParam
        {
            public string Title;
            public DateTime Time;

            public RaidCommandParam(string title, DateTime time)
            {
                Title = title;
                Time = time;
            }
        }

        private RaidCommandParam ParseRaidCommandParam(string paramStr)
        {
            Regex parenRegEx = new Regex(@"\(([^)]*)\)");
            DateTime parsedDateTime = DateTime.Now;
            foreach (Match match in parenRegEx.Matches(paramStr))
            {
                var dateTimeStr = match.Value.Substring(1, match.Value.Length - 2);
                if (ParseToDateTime(dateTimeStr, out parsedDateTime))
                {
                    break;
                }
            }

            parsedDateTime = parsedDateTime.KstToUtc();
            return new RaidCommandParam(paramStr, parsedDateTime);
        }

        private bool ParseToDateTime(string str, out DateTime parsedDateTime)
        {
            if (DateTime.TryParse(str, out var parsed))
            {
                parsedDateTime = parsed;
                return true;
            }
            else
            {
                parsedDateTime = DateTime.Now;
                return false;
            }
        }

        /// <summary>
        /// !4인 제목:"쿠크세이튼 노말" 시간:"21/10/13"
        /// !4인 제목:"쿠크세이튼 노말" 시간:"21/10/13 13:00"
        /// </summary>
        [Command("4인")]
        [Summary("4인 레이드 일정 제작")]
        public async Task FourRaid([Remainder]string paramStr)
        {
            await AddRaid(paramStr, RaidInfo.FourRaidRoles);
        }

        /// <summary>
        ///     !8인 제목:"비아키스 하드" 시간:"21-11-13 12:00"
        /// </summary>
        [Command("8인")]
        [Summary("8인 레이드 일정 제작")]
        public async Task EightRaid([Remainder]string paramStr)
        {
            await AddRaid(paramStr, RaidInfo.EightRaidRoles);
        }

        public async Task AddRaid(string paramStr, RaidInfo.RaidPlayer.Role[] roles)
        {
            //만약 스레드 채널일 경우 명령어 거절!
            if (Context.Channel is IThreadChannel)
            {
                await Context.Channel.SendMessageAsync("여기는 스레드 채널이에요!!! " +
                                                       "일정 추가는 스레드 채널에서 진행할 수 없습니다!!");
                return;
            }

            var parsedParam = ParseRaidCommandParam(paramStr);

            //참고 - 메세지 Id는 디스코드 앱 전체에서 유니크함이 (거의)보장됨.
            //https://discord.com/developers/docs/reference#snowflakes 
            var raidInfo = RaidInfo.Create(roles.Length, Context.Message.Id, roles);
            raidInfo.Title = parsedParam.Title;
            raidInfo.RaidDateTimeUtc = parsedParam.Time;

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