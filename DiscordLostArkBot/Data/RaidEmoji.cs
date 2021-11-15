using System;
using Discord;

namespace DiscordLostArkBot.Data
{
    public class RaidEmoji
    {
        public static string RoleToKrString(RaidInfo.RaidPlayer.Role role)
        {
            return role == RaidInfo.RaidPlayer.Role.Deal ? "딜러" : "서포터";
        }

        public static string RoleToEmojiString(RaidInfo.RaidPlayer.Role role)
        {
            return role == RaidInfo.RaidPlayer.Role.Deal ? EmojiSwordCrossed : EmojiShield;
        }
        
        
        public const string EmojiSwordCrossed = "⚔️";
        public const string EmojiShield = "🛡️";
        public static bool IsRaidRoleEmote(IEmote emote)
        {
            return emote.Name.Equals(EmojiShield) ||
                   emote.Name.Equals(EmojiSwordCrossed);
        }
        
        public static RaidInfo.RaidPlayer.Role EmojiStringToRole(string emojiStr)
        {
            if (emojiStr.Equals(RaidEmoji.EmojiShield))
                return RaidInfo.RaidPlayer.Role.Support;
            if (emojiStr.Equals(RaidEmoji.EmojiSwordCrossed))
                return RaidInfo.RaidPlayer.Role.Deal;
            throw new NotImplementedException();
        }
    }
}