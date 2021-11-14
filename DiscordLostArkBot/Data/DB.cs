using System.Collections.Generic;
using System.Linq;
using Discord;
using DiscordLostArkBot.Utilities;

namespace DiscordLostArkBot.Data
{
    public class Db : Singleton<Db>
    {
        public const string EmojiSwordCrossed = "⚔️";
        public const string EmojiShield = "🛡️";

        /// <summary>
        ///     최적화 위해서 추후 Dictionary로 수정하면 되겠지만 귀찮으니 일단 리스트로 구현
        /// </summary>
        private readonly List<RaidInfo> _raidInfos = new();

        public static bool IsRaidRoleEmote(IEmote emote)
        {
            return emote.Name.Equals(EmojiShield) ||
                   emote.Name.Equals(EmojiSwordCrossed);
        }

        public void AddRaidInfo(RaidInfo raidInfo)
        {
            _raidInfos.Add(raidInfo);
        }

        public RaidInfo GetRaidInfo(ulong channelId, ulong messageId)
        {
            return _raidInfos.Where(info => { return info.ChannelId == channelId && info.MessageId == messageId; })
                .FirstOrDefault();
        }
    }
}