using System.Collections.Generic;
using System.Linq;

namespace DiscordLostArkBot.Model
{
    public class RaidInfoCollection
    {
        /// <summary>
        ///     최적화 위해서 추후 Dictionary로 수정하면 되겠지만 귀찮으니 일단 리스트로 구현
        /// </summary>
        private readonly List<RaidInfo.RaidInfo> _raidInfos = new();

        public void Add(RaidInfo.RaidInfo raidInfo)
        {
            if (raidInfo == null) return;
            _raidInfos.Add(raidInfo);
        }

        public void Remove(RaidInfo.RaidInfo raidInfo)
        {
            if (raidInfo == null) return;
            _raidInfos.Remove(raidInfo);
        }

        public RaidInfo.RaidInfo FindRaidInfo(ulong discordChannelId, ulong discordMessageId)
        {
            return _raidInfos.Where(info =>
                {
                    return info.DiscordMessageKey.ChannelId == discordChannelId &&
                           info.DiscordMessageKey.MessageId == discordMessageId;
                })
                .FirstOrDefault();
        }
    }
}