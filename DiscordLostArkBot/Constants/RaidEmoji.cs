using System;
using Discord;
using DiscordLostArkBot.Model.RaidInfo;

namespace DiscordLostArkBot.Constants
{
    public class RaidEmoji
    {
        public const string EmojiSwordCrossed = "⚔️";
        public const string EmojiShield = "🛡️";

        public static bool IsRaidRoleEmote(IEmote emote)
        {
            return emote.Name.Equals(EmojiShield) ||
                   emote.Name.Equals(EmojiSwordCrossed);
        }

        public static string RoleToKrString(RaidInfo.RaidPlayer.Role role)
        {
            switch (role)
            {
                case RaidInfo.RaidPlayer.Role.Deal :
                    return "딜러";
                case RaidInfo.RaidPlayer.Role.Support :
                    return "서포터";
                default:
                    return null;
            }
        }

        public static string RoleToEmojiString(RaidInfo.RaidPlayer.Role role)
        {
            switch (role)
            {
                case RaidInfo.RaidPlayer.Role.Deal :
                    return EmojiSwordCrossed;
                case RaidInfo.RaidPlayer.Role.Support :
                    return EmojiShield;
                default:
                    return null;
            }
        }

        public static Emoji RoleToDiscordEmoji(RaidInfo.RaidPlayer.Role role)
        {
            switch (role)
            {
                case RaidInfo.RaidPlayer.Role.Deal :
                    return new Emoji(EmojiSwordCrossed);
                case RaidInfo.RaidPlayer.Role.Support :
                    return new Emoji(EmojiShield);
                default:
                    return null;
            }
        }
        
        public static RaidInfo.RaidPlayer.Role EmojiStringToRole(string emojiStr)
        {
            if (emojiStr.Equals(EmojiShield))
                return RaidInfo.RaidPlayer.Role.Support;
            if (emojiStr.Equals(EmojiSwordCrossed))
                return RaidInfo.RaidPlayer.Role.Deal;
            throw new NotImplementedException();
        }
    }
}