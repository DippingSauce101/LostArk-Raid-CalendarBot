using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DiscordLostArkBot.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RaidInfoCollection
    {
        /// <summary>
        ///     최적화 위해서 추후 Dictionary로 수정하면 되겠지만 귀찮으니 일단 리스트로 구현
        /// </summary>
        [JsonProperty] private List<RaidInfo.RaidInfo> _raidInfos = new();

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

        public RaidInfo.RaidInfo FindRaidInfo(ulong dataId)
        {
            return _raidInfos.Where(info => { return info.DataId == dataId; })
                .FirstOrDefault();
        }

        /// <summary>
        /// 레이드 스레드 id에서 연관된 RaidInfo 불러오기
        /// </summary>
        /// <param name="threadId">스레드 uid</param>
        /// <returns></returns>
        public RaidInfo.RaidInfo FindRaidInfoFromThreadUid(ulong threadId)
        {
            return _raidInfos.Where(info => { return info.DiscordMessageThreadId == threadId; })
                .FirstOrDefault();
        }

        public RaidInfo.RaidInfo ElementAt(int idx)
        {
            return _raidInfos.ElementAt(idx);
        }

        public int GetCount()
        {
            return _raidInfos.Count;
        }
    }
}