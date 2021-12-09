using System;
using System.Globalization;
using System.Linq;
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
        #region Raid Command Pameter Logics
        
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

        private bool ParseRaidCommandParam(string paramStr, out RaidCommandParam raidCommandParam)
        {
            var parseSuccessed = ParseParenthesisedDateTimeFromString(paramStr, out var parsedDateTime);
            var title = ParseTitleWithoutDateTime(paramStr);
            raidCommandParam = new RaidCommandParam(title, parsedDateTime);
            return parseSuccessed;
        }
        
        /// <summary>
        /// 중괄호 안에 DateTime 형식의 스트링이 있다면 RegEx로 찾아내서 파싱해 <b>UTC</b> DateTime 리턴.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="parseResult"></param>
        /// <returns></returns>
        private bool ParseParenthesisedDateTimeFromString(string str, out DateTime parseResult)
        {
            Regex parenRegEx = new Regex(@"\(([^)]*)\)");
            DateTime parsedDateTime = DateTime.Now.AddHours(1);
            bool parseSuccessed = false;
            
            foreach (Match match in parenRegEx.Matches(str))
            {
                var dateTimeStr = match.Value.Substring(1, match.Value.Length - 2);
                if (ParseToDateTime(dateTimeStr, out parsedDateTime))
                {
                    parseSuccessed = true;
                    break;
                }
            }

            parseResult = parsedDateTime.KstToUtc();
            return parseSuccessed;
        }

        /// <summary>
        /// 중괄호 안에 DateTime 형식의 스트링이 있다면 제거하고 리턴.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string ParseTitleWithoutDateTime(string str)
        {
            Regex parenRegEx = new Regex(@"\(([^)]*)\)");
            string title = str;
            foreach (Match match in parenRegEx.Matches(str))
            {
                var dateTimeStr = match.Value.Substring(1, match.Value.Length - 2);
                if (ParseToDateTime(dateTimeStr, out var parsedDateTime))
                {
                    title = title.Replace(match.Value, "");
                    break;
                }
            }
            return title;
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
                parsedDateTime = DateTime.Now.AddHours(1);
                return false;
            }
        }
        
        #endregion
        
        [Command("도움")]
        [Summary("도움말")]
        public async Task Help()
        {
            string helpText =  $"실행 가능한 명령어 리스트 및 예시들이에요.\n" +
                               $"!4인 쿠크세이튼 노말 트라이팟 ({DateTime.Now.AddHours(1).ToString(@"yy\/MM\/dd HH:mm")})\n" +
                               $"!8인 비아하드 트라이(1500이하만) ({DateTime.Now.AddHours(1).ToString(@"yy\/MM\/dd HH:mm")})\n";
            await Context.Channel.SendMessageAsync(helpText);
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

        private struct ModifyRaidCommandParam
        {
            public ulong RaidDataId;
            public string Title;
            public DateTime? NewDateTime;

            public ModifyRaidCommandParam(ulong raidDataId, string title, DateTime? newDateTime)
            {
                RaidDataId = raidDataId;
                Title = title;
                NewDateTime = newDateTime;
            }
        }
        
        private bool ParseModifyRaidCommandParam(string paramStr, out ModifyRaidCommandParam modifyRaidCommandParam)
        {
            var raidDataIdParsed = ParseRaidDataId(paramStr, out var parsedRaidDataId);
            //title은 파싱 실패해도 노상관!
            var titleParsed = ParseRaidTitle(paramStr, out var parsedRaidTitle);
            var dateTimeParsed = ParseParenthesisedDateTimeFromString(paramStr, out var parsedDateTime);
            
            modifyRaidCommandParam = new ModifyRaidCommandParam(parsedRaidDataId,
                titleParsed ? parsedRaidTitle : null,
                dateTimeParsed ? parsedDateTime : null);
            return raidDataIdParsed && (dateTimeParsed || titleParsed);
        }

        private bool ParseRaidDataId(string paramStr, out ulong parsedRaidDataId)
        {
            var firstParam = paramStr.Split(' ').First();
            if (string.IsNullOrWhiteSpace(firstParam))
            {
                parsedRaidDataId = 0;
                return false;
            }
            if (ulong.TryParse(firstParam, out parsedRaidDataId))
            {
                return true;
            }

            return false;
        }
        
        private bool ParseRaidTitle(string paramStr, out string parsedTitle)
        {
            var firstParam = paramStr.Split(' ').First();
            if (string.IsNullOrWhiteSpace(firstParam))
            {
                parsedTitle = null;
                return false;
            }

            var titleStart = paramStr.IndexOf(firstParam) + firstParam.Length;
            
            Regex parenRegEx = new Regex(@"\(([^)]*)\)");
            bool dateTimeParsed = false;
            string dateTimeParenthesised = null;
            foreach (Match match in parenRegEx.Matches(paramStr))
            {
                dateTimeParenthesised = match.Value; 
                var dateTimeStr = match.Value.Substring(1, match.Value.Length - 2);
                if (ParseToDateTime(dateTimeStr, out var parsedDateTime))
                {
                    dateTimeParsed = true;
                    break;
                }
            }

            int titleEnd = paramStr.Length;
            if (dateTimeParsed)
            {
                titleEnd = paramStr.IndexOf(dateTimeParenthesised);
                if (titleEnd < titleStart)
                {
                    parsedTitle = null;
                    return false;
                }
            }
            
            parsedTitle = paramStr.Substring(titleStart, titleEnd - titleStart).Trim();
            return true;
        }
        
        /// <summary>
        ///     !수정 918065709668515881 (21/12/20 22:00)
        /// </summary>
        [Command("수정")]
        [Summary("기존 레이드 정보 수정")]
        public async Task ModifyRaid([Remainder] string paramStr)
        {
            var userCommandMessage = Context.Message;

            var parseSuccessed = ParseModifyRaidCommandParam(paramStr, out var modifyRaidCommandParam);
            if (!parseSuccessed)
            {
                ulong sampleId = modifyRaidCommandParam.RaidDataId;
                if (sampleId == 0) sampleId = 123456789123456789;
                await Context.Channel.SendMessageAsync("잘못된 입력값이 들어왔어요! 다음과 같은 형식으로 입력하셔야 해요.(괄호도 포함해서!)\n" +
                    $"!수정 {sampleId.ToString("D18")} ({DateTime.Now.AddHours(1).ToString(@"yy\/MM\/dd HH:mm")})");
                await userCommandMessage.DeleteAsync();
                return;
            }

            if (ServiceHolder.RaidInfo.Exists(modifyRaidCommandParam.RaidDataId) == false)
            {
                await Context.Channel.SendMessageAsync($"수정 코드 {modifyRaidCommandParam.RaidDataId} 값을 가진 데이터를 찾지 못했어요.\n" +
                                                       $"값을 확인해 보시고, 관리자에게 연락하시거나 새로 파셔야 할 거 같아요!");
                await userCommandMessage.DeleteAsync();
                return;
            }

            var modded = ServiceHolder.RaidInfo.ModifyRaid(modifyRaidCommandParam.RaidDataId,
                modifyRaidCommandParam.Title,
                modifyRaidCommandParam.NewDateTime);
            if (!modded)
            {
                await Context.Channel.SendMessageAsync($"수정 코드 {modifyRaidCommandParam.RaidDataId} 값을 가진 데이터의 수정에 실패했어요!\n" +
                                                       $"관리자에게 문의하시는 게 좋을 것 같아요...");
                await userCommandMessage.DeleteAsync();
                return;
            }

            var discordKey = ServiceHolder.RaidInfo.DiscordKeyFromDataId(modifyRaidCommandParam.RaidDataId);
            var targetMessage = await 
                DiscordBotClient.Ins.FindUserMessage(discordKey.ChannelId, discordKey.MessageId);
            if (targetMessage == null)
            {
                await Context.Channel.SendMessageAsync($"수정 코드 {modifyRaidCommandParam.RaidDataId} 값을 가진 메세지를 찾지 못했어요!\n" +
                                                       $"문제가 계속되면 관리자에게 문의하시는 게 좋을 것 같아요...");
                await userCommandMessage.DeleteAsync();
                return;
            }

            await userCommandMessage.DeleteAsync();
            
            await ServiceHolder.RaidInfo.RefreshDiscordRaidMessage(discordKey, targetMessage);
            await ServiceHolder.RaidInfo.RefreshNotionRaidPage(discordKey);

            var threadUid = ServiceHolder.RaidInfo.GetDiscordThreadUid(discordKey);
            var threadChannel = (await Context.Client.GetChannelAsync(threadUid)) as IThreadChannel;
            if (threadChannel != null)
            {
                if(string.IsNullOrWhiteSpace(modifyRaidCommandParam.Title) == false)
                    await threadChannel.ModifyAsync(properties => properties.Name = modifyRaidCommandParam.Title);

                if (modifyRaidCommandParam.NewDateTime == null && string.IsNullOrWhiteSpace(modifyRaidCommandParam.Title) == false)
                {
                    await threadChannel.SendMessageAsync($"레이드 이름이 {modifyRaidCommandParam.Title}로 변경되었어요!");
                }
                else
                {
                    CultureInfo cultures = CultureInfo.CreateSpecificCulture("ko-KR");
                    await threadChannel.SendMessageAsync($"[{ServiceHolder.RaidInfo.GetRaidTitle(discordKey)}] 레이드의 시간을 " +
                                                         $"[{ServiceHolder.RaidInfo.GetRaidTime(discordKey).UtcToKst().ToString("yyyy년 MM월 dd일 ddd요일 HH시 mm분", cultures)}]" +
                                                         $"으로 수정했어요!");
                    
                }
            }
        }

        public async Task AddRaid(string paramStr, RaidInfo.RaidPlayer.Role[] roles)
        {
            var userCommandMessage = Context.Message;
            //만약 스레드 채널일 경우 명령어 거절!
            if (Context.Channel is IThreadChannel)
            {
                await Context.Channel.SendMessageAsync("여기는 스레드 채널이에요!!! " +
                                                       "일정 추가는 스레드 채널에서 진행할 수 없습니다!!");
                return;
            }

            bool parseSuccessed = ParseRaidCommandParam(paramStr, out var parsedParam);
            if (!parseSuccessed)
            {
                await Context.Channel.SendMessageAsync("레이드 일정이 몇 시인지 잘 모르겠어요. 일단은 한 시간 뒤로 설정해둘게요!\n" +
                                                       "도움말이 필요하시면 !도움 명령어를 입력하세요!");
            }

            //참고 - 메세지 Id는 디스코드 앱 전체에서 유니크함이 (거의)보장됨.
            //https://discord.com/developers/docs/reference#snowflakes 
            var raidInfo = RaidInfo.Create(roles.Length, Context.Message.Id, roles);
            raidInfo.Title = parsedParam.Title;
            raidInfo.RaidDateTimeUtc = parsedParam.Time;

            //공대장은 이 메세지를 보낸 유저로 자동 셋팅
            raidInfo.LeaderDiscordUserId = Context.User.Id;
            ServiceHolder.RaidInfo.Add(raidInfo);
            
            //명령어 메세지 삭제
            await userCommandMessage.DeleteAsync();

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