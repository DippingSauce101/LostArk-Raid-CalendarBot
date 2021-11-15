using System;
using System.Threading.Tasks;
using Discord;
using DiscordLostArkBot.Constants;
using DiscordLostArkBot.Model;
using DiscordLostArkBot.Model.RaidInfo;
using DiscordLostArkBot.Utilities;

namespace DiscordLostArkBot.Presenter
{
    /// <summary>
    /// RaidInfo를 조작하는 모든 로직은 이 클래스를 통해야 한다!
    /// </summary>
    public partial class RaidInfoPresenter
    {
        private RaidInfoCollection _raidInfoCollection;
        public RaidInfoPresenter(RaidInfoCollection raidInfoCollection)
        {
            _raidInfoCollection = raidInfoCollection;
        }
        
        public void Add(RaidInfo raidInfo)
        {
            _raidInfoCollection.Add(raidInfo);
        }

        public void Remove(RaidInfo raidInfo)
        {
            _raidInfoCollection.Remove(raidInfo);
        }
        
        private RaidInfo FindRaidInfo(ulong discordChannelId, ulong discordMessageId)
        {
            return _raidInfoCollection.FindRaidInfo(discordChannelId, discordMessageId);
        }

        public bool CanAddPlayer(RaidInfo.DiscordKey discordKey, RaidInfo.RaidPlayer.Role requestedRole)
        {
            var raidInfo = FindRaidInfo(discordKey.ChannelId, discordKey.MessageId);
            if (raidInfo == null) return false;
            if (raidInfo.IsRoleFull(requestedRole)) return false;

            return true;
        }

        public async Task RemoveDiscordOldRoleReaction(RaidInfo.DiscordKey discordKey, ulong userId, IUserMessage userMessage, RaidInfo.RaidPlayer.Role newRole)
        {
            if (UserAlreadySeated(discordKey, userId))
            {
                var raidInfo = FindRaidInfo(discordKey.ChannelId, discordKey.MessageId);
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

        public bool UserAlreadySeated(RaidInfo.DiscordKey discordKey, ulong userId)
        {
            var raidInfo = FindRaidInfo(discordKey.ChannelId, discordKey.MessageId);
            if (raidInfo == null) return false;
            return raidInfo.UserAlreadySeated(userId);
        }
        
        public async Task RefreshDiscordRaidMessage(RaidInfo.DiscordKey discordKey, IUserMessage userMessage)
        {
            var raidInfo = FindRaidInfo(discordKey.ChannelId, discordKey.MessageId);
            await userMessage.ModifyAsync(x =>
            {
                var eb = raidInfo.GetEmbedBuilder();
                x.Embed = eb.Build();
            });
        }
        
        public bool AddOrChangePlayerRole(RaidInfo.DiscordKey discordKey, ulong userId, RaidInfo.RaidPlayer.Role role)
        {
            var raidInfo = FindRaidInfo(discordKey.ChannelId, discordKey.MessageId);
            if (raidInfo == null) return false;

            var emptySeatIndex = raidInfo.GetEmptySeatIndex(role);
            if (emptySeatIndex == -1) return false;

            raidInfo.RaidPlayers[emptySeatIndex].UserId = userId;
            raidInfo.RaidPlayers[emptySeatIndex].UserRole = role;
            return true;
        }

        public bool RemovePlayerRole(RaidInfo.DiscordKey discordKey, ulong userId, RaidInfo.RaidPlayer.Role role)
        {
            var raidInfo = FindRaidInfo(discordKey.ChannelId, discordKey.MessageId);
            if (raidInfo == null) return false;
            
            var raidPlayers = raidInfo.RaidPlayers;

            bool removed = false;
            for (var i = 0; i < raidPlayers.Length; i++)
                if (raidPlayers[i].UserId == userId &&
                    raidPlayers[i].UserRole == role)
                {
                    raidPlayers[i].UserId = RaidInfo.RaidPlayer.UserEmpty;
                    removed = true;
                }

            return removed;
        }
    }
}