using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using DiscordLostArkBot.Constants;
using DiscordLostArkBot.Discord.Command;
using DiscordLostArkBot.Discord.Command.Parser;
using DiscordLostArkBot.Model.RaidInfo;
using DiscordLostArkBot.Notion;
using DiscordLostArkBot.Service;
using DiscordLostArkBot.Utilities;

namespace DiscordLostArkBot.Discord
{
    public class RaidSchedulerModule : ModuleBase<SocketCommandContext>
    {
        [Command("도움")]
        [Summary("도움말")]
        public async Task Help()
        {
            var helpText = "실행 가능한 명령어 리스트 및 예시들이에요.\n" +
                           $"!4인일정 쿠크세이튼 노말 트라이팟 ({DateTime.Now.AddHours(1).ToString(@"yy\/MM\/dd HH:mm")})\n" +
                           $"!8인일정 비아하드 트라이(1500이하만) ({DateTime.Now.AddHours(1).ToString(@"yy\/MM\/dd HH:mm")})\n" + 
                           $"!수정 123456789123456789 ({DateTime.Now.AddHours(1).ToString(@"yy\/MM\/dd HH:mm")})\n";
            await Context.Channel.SendMessageAsync(helpText);
        }

        /// <summary>
        ///     !4인일정 쿠크노말(21/12/20 20:00)
        /// </summary>
        [Command("4인일정")]
        [Summary("4인 레이드 일정 제작")]
        public async Task FourRaid([Remainder] string paramStr)
        {
            await AddRaid(paramStr, RaidInfo.FourRaidRoles);
        }
        
        /// <summary>
        ///     멀티커맨드 찾기 귀찮아서 일단 걍 복붙...
        ///     !4인 쿠크노말(21/12/20 20:00)
        /// </summary>
        [Command("4인")]
        [Summary("4인 레이드 일정 제작")]
        public async Task FourRaidShort([Remainder] string paramStr)
        {
            await FourRaid(paramStr);
        }

        /// <summary>
        ///     !8인 쿠크노말(21/12/20 20:00)
        /// </summary>
        [Command("8인일정")]
        [Summary("8인 레이드 일정 제작")]
        public async Task EightRaid([Remainder] string paramStr)
        {
            await AddRaid(paramStr, RaidInfo.EightRaidRoles);
        }
        
        /// <summary>
        ///     멀티커맨드 찾기 귀찮아서 일단 걍 복붙...
        ///     !8인 쿠크노말(21/12/20 20:00)
        /// </summary>
        [Command("8인")]
        [Summary("8인 레이드 일정 제작")]
        public async Task EightRaidShort([Remainder] string paramStr)
        {
            await EightRaid(paramStr);
        }

        /// <summary>
        ///     !수정 918065709668515881 (21/12/20 22:00)
        /// </summary>
        [Command("수정")]
        [Summary("기존 레이드 정보 수정")]
        public async Task ModifyRaid([Remainder] string paramStr)
        {
            var userCommandMessage = Context.Message;

            ModifyRaidCommandParam modifyRaidCommandParam;
            bool parsed;
            if (Context.Channel is IThreadChannel)
            {
                parsed = CommandParser.ModifyRaidInThread.Parse(paramStr, out modifyRaidCommandParam, Context.Channel);
            }
            else
            {
                parsed = CommandParser.ModifyRaid.Parse(paramStr, out modifyRaidCommandParam);
            }

            if (!parsed)
            {
                var sampleId = modifyRaidCommandParam.RaidDataId;
                if (sampleId == 0) sampleId = 123456789123456789;
                await Context.Channel.SendMessageAsync("잘못된 입력값이 들어왔어요! 다음과 같은 형식으로 입력하셔야 해요.(괄호도 포함해서!)\n" +
                                                       $"!수정 {sampleId.ToString("D18")} ({DateTime.Now.AddHours(1).ToString(@"yy\/MM\/dd HH:mm")})");
                await userCommandMessage.DeleteAsync();
                return;
            }

            if (ServiceHolder.RaidInfo.Exists(modifyRaidCommandParam.RaidDataId) == false)
            {
                await Context.Channel.SendMessageAsync(
                    $"수정 코드 {modifyRaidCommandParam.RaidDataId} 값을 가진 데이터를 찾지 못했어요.\n" +
                    "값을 확인해 보시고, 관리자에게 연락하시거나 새로 파셔야 할 거 같아요!");
                await userCommandMessage.DeleteAsync();
                return;
            }

            var modded = ServiceHolder.RaidInfo.ModifyRaid(modifyRaidCommandParam.RaidDataId,
                modifyRaidCommandParam.Title,
                modifyRaidCommandParam.NewDateTime);
            if (!modded)
            {
                await Context.Channel.SendMessageAsync(
                    $"수정 코드 {modifyRaidCommandParam.RaidDataId} 값을 가진 데이터의 수정에 실패했어요!\n" +
                    "관리자에게 문의하시는 게 좋을 것 같아요...");
                await userCommandMessage.DeleteAsync();
                return;
            }

            var discordKey = ServiceHolder.RaidInfo.DiscordKeyFromDataId(modifyRaidCommandParam.RaidDataId);
            var targetMessage = await
                DiscordBotClient.Ins.FindUserMessage(discordKey.ChannelId, discordKey.MessageId);
            if (targetMessage == null)
            {
                await Context.Channel.SendMessageAsync(
                    $"수정 코드 {modifyRaidCommandParam.RaidDataId} 값을 가진 메세지를 찾지 못했어요!\n" +
                    "문제가 계속되면 관리자에게 문의하시는 게 좋을 것 같아요...");
                await userCommandMessage.DeleteAsync();
                return;
            }

            await userCommandMessage.DeleteAsync();

            await ServiceHolder.RaidInfo.RefreshDiscordRaidMessage(discordKey, targetMessage);
            await ServiceHolder.RaidInfo.RefreshNotionRaidPage(discordKey);

            var threadUid = ServiceHolder.RaidInfo.GetDiscordThreadUid(discordKey);
            var threadChannel = await Context.Client.GetChannelAsync(threadUid) as IThreadChannel;
            if (threadChannel != null)
            {
                // if(string.IsNullOrWhiteSpace(modifyRaidCommandParam.Title) == false)
                //     await threadChannel.ModifyAsync(properties => properties.Name = modifyRaidCommandParam.Title);

                if (modifyRaidCommandParam.NewDateTime == null &&
                    string.IsNullOrWhiteSpace(modifyRaidCommandParam.Title) == false)
                {
                    //메세지 보내는거도 Rate Limit에 걸림... 스레드명 변경시 알람 어차피 가니까 추가로 보낼필요 없을듯
                    //await threadChannel.SendMessageAsync($"레이드 이름이 {modifyRaidCommandParam.Title}로 변경되었어요!");
                }
                else
                {
                    var cultures = CultureInfo.CreateSpecificCulture("ko-KR");
                    await threadChannel.SendMessageAsync(
                        $"[{ServiceHolder.RaidInfo.GetRaidTitle(discordKey)}] 레이드의 시간을 " +
                        $"[{ServiceHolder.RaidInfo.GetRaidTime(discordKey).UtcToKst().ToString("yyyy년 MM월 dd일 ddd요일 HH시 mm분", cultures)}]" +
                        "으로 수정했어요!");
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

            var parseSuccessed = CommandParser.CreateRaid.Parse(paramStr, out var parsedParam);
            if (!parseSuccessed)
                await Context.Channel.SendMessageAsync("레이드 일정이 몇 시인지 잘 모르겠어요. 일단은 한 시간 뒤로 설정해둘게요!\n" +
                                                       "도움말이 필요하시면 !도움 명령어를 입력하세요!");

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
            var messageSent =
                await Context.Channel.SendMessageAsync(titleMessage, false, eb.Build(), component: cb.Build());
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
                var textChannel = messageSent.Channel as ITextChannel;
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