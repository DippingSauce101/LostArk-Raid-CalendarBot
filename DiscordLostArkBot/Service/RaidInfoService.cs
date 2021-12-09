using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using DiscordLostArkBot.Constants;
using DiscordLostArkBot.Discord;
using DiscordLostArkBot.Model;
using DiscordLostArkBot.Model.RaidInfo;
using DiscordLostArkBot.Notion;
using Notion.Client;

namespace DiscordLostArkBot.Service
{
    /// <summary>
    ///     RaidInfo를 조작하는 모든 로직은 이 클래스를 통해야 한다!
    /// </summary>
    public class RaidInfoService
    {
        private readonly RaidInfoCollection _raidInfoCollection;

        public RaidInfoService(RaidInfoCollection raidInfoCollection)
        {
            _raidInfoCollection = raidInfoCollection;
        }

        public void Add(RaidInfo raidInfo)
        {
            _raidInfoCollection.Add(raidInfo);
            DB.Ins.SaveToFile(raidInfo);
        }

        public void Remove(RaidInfo raidInfo)
        {
            _raidInfoCollection.Remove(raidInfo);
            DB.Ins.Delete(raidInfo);
        }

        public bool Exists(RaidInfo.DiscordKey discordKey)
        {
            return FindRaidInfo(discordKey.ChannelId, discordKey.MessageId) != null;
        }

        public bool Exists(ulong dataId)
        {
            return FindRaidInfo(dataId) != null;
        }
        
        public RaidInfo.DiscordKey DiscordKeyFromDataId(ulong dataId)
        {
            var raidInfo = FindRaidInfo(dataId);
            if (raidInfo == null) throw new NullReferenceException($"RaidInfo with dataId {dataId} NOT exists!");
            return raidInfo.DiscordMessageKey;
        }

        public bool ModifyRaid(ulong dataId, string title, DateTime? utcDateTime)
        {
            var raidInfo = FindRaidInfo(dataId);
            if (raidInfo == null) return false;
            if (string.IsNullOrWhiteSpace(title) == false)
            {
                raidInfo.Title = title;
            }

            if (utcDateTime.HasValue)
            {
                raidInfo.RaidDateTimeUtc = utcDateTime.Value;
            }
            DB.Ins.SaveToFile(raidInfo);
            return true;
        }

        public bool CanAddPlayer(RaidInfo.DiscordKey discordKey, RaidInfo.RaidPlayer.Role requestedRole)
        {
            var raidInfo = FindRaidInfo(discordKey);
            if (raidInfo == null) return false;
            if (raidInfo.IsRoleFull(requestedRole)) return false;

            return true;
        }
        
        public bool UserAlreadySeated(RaidInfo.DiscordKey discordKey, ulong userId)
        {
            var raidInfo = FindRaidInfo(discordKey);
            if (raidInfo == null) return false;
            return raidInfo.UserAlreadySeated(userId);
        }

        public bool AddOrChangePlayerRole(RaidInfo.DiscordKey discordKey, ulong userId, RaidInfo.RaidPlayer.Role role)
        {
            var raidInfo = FindRaidInfo(discordKey);
            if (raidInfo == null) return false;

            var emptySeatIndex = raidInfo.GetEmptySeatIndex(role);
            if (emptySeatIndex == -1) return false;

            raidInfo.RaidPlayers[emptySeatIndex].UserId = userId;
            raidInfo.RaidPlayers[emptySeatIndex].UserRole = role;

            DB.Ins.SaveToFile(raidInfo);
            return true;
        }

        public bool RemovePlayerRole(RaidInfo.DiscordKey discordKey, ulong userId, RaidInfo.RaidPlayer.Role role)
        {
            var raidInfo = FindRaidInfo(discordKey);
            if (raidInfo == null) return false;

            var raidPlayers = raidInfo.RaidPlayers;

            var removed = false;
            for (var i = 0; i < raidPlayers.Length; i++)
                if (raidPlayers[i].UserId == userId &&
                    raidPlayers[i].UserRole == role)
                {
                    raidPlayers[i].UserId = RaidInfo.RaidPlayer.UserEmpty;
                    removed = true;
                }

            if (removed)
            {            
                DB.Ins.SaveToFile(raidInfo);
            }

            return removed;
        }

        public bool IsUserLeader(RaidInfo.DiscordKey discordKey, ulong userId)
        {
            var raidInfo = FindRaidInfo(discordKey);
            if (raidInfo == null) return false;
            return raidInfo.LeaderDiscordUserId == userId;
        }

        public async Task OnRaidMessageDeleted(RaidInfo.DiscordKey discordKey)
        {
            var raidInfo = FindRaidInfo(discordKey);
            if (raidInfo == null) return;
            
            var notionCalendarPageId = GetNotionCalendarPageId(discordKey);
            Remove(raidInfo);
            await NotionBotClient.Ins.DeletePage(notionCalendarPageId);
        }

        public ulong GetDiscordThreadUid(RaidInfo.DiscordKey discordKey)
        {
            var raidInfo = FindRaidInfo(discordKey);
            if (raidInfo == null) return 0;
            else return raidInfo.DiscordMessageThreadId;
        }

        public string GetRaidTitle(RaidInfo.DiscordKey discordKey)
        {
            var raidInfo = FindRaidInfo(discordKey);
            if (raidInfo == null) return null;
            else return raidInfo.Title;
        }
        
        public DateTime GetRaidTime(RaidInfo.DiscordKey discordKey)
        {
            var raidInfo = FindRaidInfo(discordKey);
            if (raidInfo == null) throw new NullReferenceException("Data not found!");
            else return raidInfo.RaidDateTimeUtc;
        }
        
        #region Notion Logics

        public string GetNotionCalendarPageId(RaidInfo.DiscordKey discordKey)
        {
            var raidInfo = FindRaidInfo(discordKey);
            if (raidInfo == null) return string.Empty;
            return raidInfo.NotionCalenderPageId;
        }

        public Dictionary<string, PropertyValue> GetNotionCalendarPageProperies(RaidInfo.DiscordKey discordKey)
        {
            var raidInfo = FindRaidInfo(discordKey);
            if (raidInfo == null) return null;
            return raidInfo.GetNotionPageProperties();
        }

        public bool SetNotionCalendarPageId(RaidInfo.DiscordKey discordKey, string notionCalendarPageId)
        {
            var raidInfo = FindRaidInfo(discordKey);
            if (raidInfo == null) return false;

            raidInfo.NotionCalenderPageId = notionCalendarPageId;
            DB.Ins.SaveToFile(raidInfo);
            return true;
        }
        
        #endregion
        
        #region Tasks
        
        public async Task RemoveDiscordOldRoleReaction(RaidInfo.DiscordKey discordKey, ulong userId,
            IUserMessage userMessage, RaidInfo.RaidPlayer.Role newRole)
        {
            if (UserAlreadySeated(discordKey, userId))
            {
                var raidInfo = FindRaidInfo(discordKey);
                var currentRole = raidInfo.GetUserRole(userId);
                if (newRole == currentRole)
                    //이유는 모르겠지만 같은 이모지가 두번 추가되고 있네? 그냥 리턴
                    return;

                var emojiToRemove = currentRole == RaidInfo.RaidPlayer.Role.Deal
                    ? new Emoji(RaidEmoji.EmojiSwordCrossed)
                    : new Emoji(RaidEmoji.EmojiShield);
                await userMessage.RemoveReactionAsync(emojiToRemove, userId);
            }
        }

        public async Task RefreshDiscordRaidMessage(RaidInfo.DiscordKey discordKey, IUserMessage userMessage)
        {
            var raidInfo = FindRaidInfo(discordKey);
            if (raidInfo == null) return;
            await userMessage.ModifyAsync(x =>
            {
                var eb = raidInfo.GetEmbedBuilder();
                x.Embed = eb.Build();
            });
        }

        public async Task RefreshNotionRaidPage(RaidInfo.DiscordKey discordKey)
        {
            var raidInfo = FindRaidInfo(discordKey);
            if (raidInfo == null) return;
            await raidInfo.RefreshUserCache();
            await NotionBotClient.Ins.UpdatePage(raidInfo.NotionCalenderPageId, raidInfo.GetNotionPageProperties());
        }
        
        #endregion

        private RaidInfo FindRaidInfo(ulong dataId)
        {
            return _raidInfoCollection.FindRaidInfo(dataId);
        }

        private RaidInfo FindRaidInfo(ulong discordChannelId, ulong discordMessageId)
        {
            return _raidInfoCollection.FindRaidInfo(discordChannelId, discordMessageId);
        }

        private RaidInfo FindRaidInfo(RaidInfo.DiscordKey discordKey)
        {
            return _raidInfoCollection.FindRaidInfo(discordKey.ChannelId, discordKey.MessageId);
        }
    }
}